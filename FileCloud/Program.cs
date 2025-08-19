using FileCloud.Application.Services;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Options;
using FileCloud.DataAccess;
using FileCloud.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<FileCloudDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(FileCloudDbContext)));
});

// File upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000; // 500MB
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 524_288_000; // 500MB
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var env = builder.Environment;
string basePath = Path.Combine(env.WebRootPath, "Files");
if (!Directory.Exists(basePath))
    Directory.CreateDirectory(basePath);

builder.Services.Configure<StorageOptions>(options =>
{
    options.BasePath = basePath;
});

// DI for repositories and services
builder.Services.AddScoped<IFilesService, FileService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IStorageService,  StorageService>();
builder.Services.AddScoped<IFilesRepositories, FileRepositories>();
builder.Services.AddScoped<IFolderRepositories, FolderRepositories>();
builder.Services.AddScoped<PreviewService>();
builder.Services.AddScoped<ILogger<FileService>, Logger<FileService>>();
builder.Services.AddScoped<ILogger<FolderService>, Logger<FolderService>>();
builder.Services.AddScoped<ILogger<StorageService>, Logger<StorageService>>();


var app = builder.Build();

// Middleware order
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapHub<FileCloud.Hubs.FileHub>("/fileHub");

// Conventional routing (optional)
app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action}/{id?}");

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FileCloudDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
