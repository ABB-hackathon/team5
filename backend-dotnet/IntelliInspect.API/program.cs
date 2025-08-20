using IntelliInspect.API.Services;  // so DateRangeService is visible

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Register DateRangeService with DI container
builder.Services.AddScoped<DateRangeService>();
builder.Services.AddHttpClient<TrainingService>();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
