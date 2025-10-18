using CaptionFlowApi.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for large file uploads (2GB limit)
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 2_147_483_648; // 2GB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
});

// Configure IIS for large file uploads
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 2_147_483_648; // 2GB
});

// Configure form options for large file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 2_147_483_648; // 2GB
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

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
// Enable Swagger in all environments for easy testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CaptionFlow API v1");
    options.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger
    options.DocumentTitle = "CaptionFlow API Documentation";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Welcome endpoint - root redirects to swagger
app.MapGet("/", () => 
{
    return Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>CaptionFlow API</title>
    <style>
        body { font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; }
        .container { text-align: center; }
        h1 { font-size: 3em; margin-bottom: 20px; }
        .links { margin-top: 30px; }
        .links a { display: inline-block; margin: 10px 20px; padding: 15px 30px; background: white; color: #667eea; text-decoration: none; border-radius: 5px; font-weight: bold; transition: transform 0.2s; }
        .links a:hover { transform: scale(1.1); }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🎬 CaptionFlow API</h1>
        <p>RESTful API for Video Subtitle Extraction</p>
        <div class='links'>
            <a href='/swagger'>📖 API Documentation</a>
            <a href='/api/subtitle/health'>🏥 Health Check</a>
        </div>
        <p style='margin-top: 40px; opacity: 0.8; font-size: 0.9em;'>Version 1.0 | Running on HTTPS</p>
    </div>
</body>
</html>
", "text/html");
}).ExcludeFromDescription();

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║          CaptionFlow RESTful API v1.0           ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("📹 Video Subtitle Extraction API");
Console.WriteLine("🚀 API Documentation: https://localhost:5001/swagger");
Console.WriteLine("🏠 Home Page: https://localhost:5001/");
Console.WriteLine();

app.Run();
