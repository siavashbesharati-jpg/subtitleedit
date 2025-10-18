using CaptionFlowApi.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CaptionFlow API",
        Version = "v1",
        Description = "RESTful API for video subtitle extraction and SRT export",
        Contact = new OpenApiContact
        {
            Name = "CaptionFlow",
            Url = new Uri("https://github.com/siavashbesharati-jpg/subtitleedit")
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Register services
builder.Services.AddSingleton<SubtitleProcessingService>();

// Add CORS for web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CaptionFlow API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Welcome endpoint
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║          CaptionFlow RESTful API v1.0           ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("📹 Video Subtitle Extraction API");
Console.WriteLine("🚀 API Documentation: https://localhost:5001/swagger");
Console.WriteLine();

app.Run();
