import pandas as pd
import numpy as np
import io, base64
import matplotlib.pyplot as plt
from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Dict, Optional

# ML imports
from sklearn.model_selection import StratifiedKFold
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, precision_score, recall_score, f1_score, confusion_matrix
from xgboost import XGBClassifier
from lightgbm import LGBMClassifier
from catboost import CatBoostClassifier

# FastAPI app
app = FastAPI()

# Global storage for trained models
trained_models = {}
meta_model = None
train_threshold = 0.7

# Request/Response Schemas
class TrainRequest(BaseModel):
    trainData: List[Dict]
    testData: List[Dict]
    threshold: Optional[float] = 0.7
    downsample: Optional[bool] = False

class TrainResponse(BaseModel):
    accuracy: float
    precision: float
    recall: float
    f1Score: float
    trainingChartBase64: Optional[str]
    confusionMatrixBase64: Optional[str]
    status: str
    message: str

class PredictRequest(BaseModel):
    rows: List[Dict]

class PredictResponse(BaseModel):
    timestamp: str
    sample_id: Optional[str]
    prediction: str
    confidence: float
    temperature: Optional[float]
    pressure: Optional[float]
    humidity: Optional[float]

# Helper: plot to base64
def fig_to_base64(fig):
    buf = io.BytesIO()
    plt.savefig(buf, format="png")
    plt.close(fig)
    return base64.b64encode(buf.getvalue()).decode("utf-8")

# Health and root endpoints (for integration checks)
@app.get("/")
def read_root():
    return {"message": "ML Service is running!"}

@app.get("/health")
def health_check():
    return {"status": "healthy"}

# Train endpoint
@app.post("/train", response_model=TrainResponse)
def train_model(req: TrainRequest):
    global trained_models, meta_model, train_threshold

    # Convert lists to DataFrame
    df_train = pd.DataFrame(req.trainData)
    df_test = pd.DataFrame(req.testData)
    threshold = req.threshold
    train_threshold = threshold

    # Find target column
    target_col = next((c for c in df_train.columns if c.lower() == "response"), None)
    if not target_col:
        return TrainResponse(status="Error", message="No Response column found",
                             accuracy=0, precision=0, recall=0, f1Score=0,
                             trainingChartBase64=None, confusionMatrixBase64=None)

    # Optional downsampling
    if req.downsample:
        min_size = df_train[target_col].value_counts().min()
        df_train = pd.concat([
            df_train[df_train[target_col] == 0].sample(min_size, random_state=42),
            df_train[df_train[target_col] == 1].sample(min_size, random_state=42)
        ]).sample(frac=1, random_state=42)  # shuffle

    # Prepare features and target
    drop_cols = [target_col, "synthetic_timestamp"]
    X_train_full = df_train.drop(columns=[c for c in drop_cols if c in df_train.columns])
    X_test_full = df_test.drop(columns=[c for c in drop_cols if c in df_test.columns])
    y_train = df_train[target_col]
    y_test = df_test[target_col]

    # Automatically drop columns with >70% NA in training set
    na_drop_thresh = 0.7
    na_frac_train = X_train_full.isna().mean()
    cols_to_keep = na_frac_train[na_frac_train <= na_drop_thresh].index.tolist()

    X_train = X_train_full[cols_to_keep].select_dtypes(include=["number"])
    X_test = X_test_full[[c for c in cols_to_keep if c in X_test_full.columns]].select_dtypes(include=["number"])

    # Fill remaining missing values with -999
    X_train = X_train.fillna(-999)
    X_test = X_test.fillna(-999)

    # Base models
    base_models = [
        ("xgb", XGBClassifier(use_label_encoder=False, eval_metric="logloss", random_state=42)),
        ("lgbm", LGBMClassifier(random_state=42)),
        ("cat", CatBoostClassifier(verbose=0, random_state=42))
    ]

    # OOF stacking
    skf = StratifiedKFold(n_splits=5, shuffle=True, random_state=42)
    oof_preds = np.zeros((len(X_train), len(base_models)))
    test_preds = np.zeros((len(X_test), len(base_models)))

    for i, (name, model) in enumerate(base_models):
        fold_preds = np.zeros(len(X_test))
        for train_idx, val_idx in skf.split(X_train, y_train):
            X_tr, X_val = X_train.iloc[train_idx], X_train.iloc[val_idx]
            y_tr, y_val = y_train.iloc[train_idx], y_train.iloc[val_idx]
            model.fit(X_tr, y_tr)
            oof_preds[val_idx, i] = model.predict_proba(X_val)[:, 1]
            fold_preds += model.predict_proba(X_test)[:, 1] / skf.n_splits
        test_preds[:, i] = fold_preds
        trained_models[name] = model  # keep trained base models

    # Meta model
    meta_model = RandomForestClassifier(n_estimators=100, random_state=42)
    meta_model.fit(oof_preds, y_train)

    # Predictions with threshold
    y_pred_prob = meta_model.predict_proba(test_preds)[:, 1]
    y_pred = (y_pred_prob >= threshold).astype(int)

    # Metrics
    acc = accuracy_score(y_test, y_pred)
    prec = precision_score(y_test, y_pred, zero_division=0)
    rec = recall_score(y_test, y_pred, zero_division=0)
    f1 = f1_score(y_test, y_pred, zero_division=0)
    cm = confusion_matrix(y_test, y_pred)

    # Charts
    fig, ax = plt.subplots()
    ax.bar(["Acc", "Prec", "Rec", "F1"], [acc, prec, rec, f1])
    chart_b64 = fig_to_base64(fig)

    fig, ax = plt.subplots()
    im = ax.imshow(cm, cmap="viridis")
    plt.colorbar(im)
    ax.set_title("Confusion Matrix")
    cm_b64 = fig_to_base64(fig)

    return TrainResponse(
        accuracy=acc,
        precision=prec,
        recall=rec,
        f1Score=f1,
        trainingChartBase64=chart_b64,
        confusionMatrixBase64=cm_b64,
        status="Success",
        message=f"Stacking model trained on {len(X_train)} rows, tested on {len(X_test)} rows"
    )

# Predict endpoint
@app.post("/predict", response_model=List[PredictResponse])
def predict(req: PredictRequest):
    global trained_models, meta_model, train_threshold
    if not meta_model or not trained_models:
        return []  # model not trained yet

    # Convert rows to DataFrame
    df = pd.DataFrame(req.rows)
    drop_cols = ["response", "Response", "synthetic_timestamp"]
    X = df.drop(columns=[c for c in drop_cols if c in df.columns]).select_dtypes(include=["number"])

    # Fill missing with -999 for consistency
    X = X.fillna(-999)

    # Base model predictions
    base_preds = np.zeros((len(X), len(trained_models)))
    for i, (name, model) in enumerate(trained_models.items()):
        base_preds[:, i] = model.predict_proba(X)[:, 1]

    # Meta model predictions
    y_pred_prob = meta_model.predict_proba(base_preds)[:, 1]
    y_pred = (y_pred_prob >= train_threshold).astype(int)

    responses = []
    for idx, row in df.iterrows():
        pred_label = y_pred[idx]
        conf = y_pred_prob[idx] * 100  # confidence as percentage
        responses.append(PredictResponse(
            timestamp=str(row.get("synthetic_timestamp", "")),
            sample_id=row.get("Id"),
            prediction="Pass" if pred_label == 1 else "Fail",
            confidence=round(conf, 2),
            temperature=row.get("temperature"),
            pressure=row.get("pressure"),
            humidity=row.get("humidity")
        ))
    return responses
