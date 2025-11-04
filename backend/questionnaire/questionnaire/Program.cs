using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Text;

using questionnaire.questionnaire.Authentication;
using questionnaire.questionnaire.Models;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Services.AddControllers();

// �������� ����������� ��� �������
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ��������� Swagger (������������ API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "������� ����� � �������: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// �������� �������������� JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidAudience = AuthOptions.AUDIENCE,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        };
    });

// ��������� �����������
builder.Services.AddAuthorization();

// ��������� ����������� � ��
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
/*if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("������ ����������� 'DefaultConnection' �� �������.");
}*/

builder.Services.AddDbContext<QuestionnaireContext>(options =>
    options.UseSqlServer(connectionString));

// ����������� ������� ��� ������ � ��������
builder.Services.AddSingleton<TokenService>(provider =>
{
    return new TokenService(
        jwtKey: AuthOptions.KEY,
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        lifetimeMinutes: AuthOptions.LIFETIME
    );
});

// ��������� CORS-��������
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // �����, ���� ����� ���������� ���� ��� �����
    });
});

/*builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://justcause8.github.io ")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});*/

// ������ ����������
var app = builder.Build();

// Middleware (����������� ��������)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// ������� middleware �����! CORS ������ ���� �� ��������������
app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();

app.UseAuthentication(); // �������� �������� �������
app.UseAuthorization();  // �������� �����������

// �������� ������ (������� ��� ������� 500-� ������)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError("������ �������: {0}", context.Response.StatusCode);
    });
});

app.MapControllers();

// ���������, �� ����� URL/������ ����� �������� ������
app.Urls.Add("http://0.0.0.0:5000");      // HTTP � ����� �������� ��� ���������� �������
// app.Urls.Add("https://0.0.0.0:7109");     // HTTPS � �������� ����
//app.Urls.Add("https://5.129.207.189:443"); // �������� � ����������� IP �� ����� 443

await app.RunAsync();