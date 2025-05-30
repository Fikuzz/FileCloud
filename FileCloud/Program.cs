using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using FileCloud.DataAccess;
using Microsoft.EntityFrameworkCore;
using FileCloud.Core.Abstractions;
using FileCloud.Application.Services;
using FileCloud.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FileCloudDbContext>(
    options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(FileCloudDbContext)));
    });



builder.Services.AddScoped<IFilesService, FileService>();
builder.Services.AddScoped<IFilesRepositories, FileRepositories>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
