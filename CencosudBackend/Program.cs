using System.Text;
using CencosudBackend.Repositories;
using CencosudBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CencosudBackend.Services;


var builder = WebApplication.CreateBuilder(args);

// CORS
var corsPolicyName = "FrontendPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
        // Si más adelante usas cookies/credenciales: .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repos & Services
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddScoped<ICencosudTiendaRepository, CencosudTiendaRepository>();
builder.Services.AddScoped<ICencosudTiendaService, CencosudTiendaService>();

builder.Services.AddScoped<IOpenAiVisionService, OpenAiVisionService>();

builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IMetasRepository, MetasRepository>();
builder.Services.AddScoped<IMetasService, MetasService>();

builder.Services.AddScoped<CencosudBackend.Repositories.IAdminReportesRepository, CencosudBackend.Repositories.AdminReportesRepository>();
builder.Services.AddScoped<CencosudBackend.Services.IAdminReportesService, CencosudBackend.Services.AdminReportesService>();


// JWT Auth
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not found");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicyName);

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
