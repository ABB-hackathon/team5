var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddHttpClient<TrainingService>();

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

app.MapControllers();

app.Run();

