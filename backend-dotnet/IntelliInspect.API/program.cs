using IntelliInspect.API.Services;  // so DateRangeService is visible

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Register DateRangeService with DI container
builder.Services.AddScoped<DateRangeService>();
builder.Services.AddHttpClient<TrainingService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
// Use CORS
app.UseCors("AllowAll");

app.Run();
