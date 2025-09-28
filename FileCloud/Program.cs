using FileCloud.Application.Services;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Options;
using FileCloud.DataAccess;
using FileCloud.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // ��������� �������� ��� JWT � Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // ��������� ���������� ������������ ��� ���� endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer"
            },
            Array.Empty<string>()
        }
    });
});

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
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IFilesRepository, FileRepository>();
builder.Services.AddScoped<IFolderRepository, FolderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<PreviewService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<ILogger<FileService>, Logger<FileService>>();
builder.Services.AddScoped<ILogger<FolderService>, Logger<FolderService>>();
builder.Services.AddScoped<ILogger<StorageService>, Logger<StorageService>>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// 1. ��������� ������� �������������� � ����������� JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // ���������, ��� ���������� ����� Bearer (JWT)
    .AddJwtBearer(options => // ������������� ��������� �������� JWT
    {
        // ��� ��������� ������ ��������� � ����, ��� ������������ � JwtService ��� ���������!
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // ��������� ��������?
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // ��� �������� �������� ���������
            ValidateAudience = true, // ��������� ���������?
            ValidAudience = builder.Configuration["Jwt:Audience"], // ��� ���� ������������ �����
            ValidateLifetime = true, // ��������� ���� ��������?
            ValidateIssuerSigningKey = true, // ��������� ���� �������?
            IssuerSigningKey = new SymmetricSecurityKey( // ��� �� ����� ����, ��� � ��� �������
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            )
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var iatClaim = context.Principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(iatClaim))
                {
                    context.Fail("Invalid token claims");
                    return;
                }

                var userId = Guid.Parse(userIdClaim);
                var tokenIssuedAt = long.Parse(iatClaim);
                var tokenIssuedAtDateTime = DateTimeOffset.FromUnixTimeSeconds(tokenIssuedAt).UtcDateTime;

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<FileCloudDbContext>();
                var user = await dbContext.Users.FindAsync(userId);

                if(user == null)
                {
                    context.Fail("User account no longer exists");
                }
                // ���� ����������� ���� "������� �����" � ����� ������� �� ���� ����
                if (user?.TokensValidAfter != null && tokenIssuedAtDateTime < user.TokensValidAfter.Value)
                {
                    context.Fail("Token revoked");
                }
            }
        };
    });

// 2. ��������� ������� �����������
builder.Services.AddAuthorization(); // ��������� ������������ �������� ����� [Authorize]

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

// --- MIDDLEWARE ---
// 1. CORS
app.UseCors("AllowAll");

// 2. �������������� (������� ������, ��� ������������)
app.UseAuthentication();

// 3. ����������� (����� ���������, ��� ��� ����� ������)
app.UseAuthorization();
// -----------------------------------------------

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
