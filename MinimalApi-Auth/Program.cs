using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MinimalApi_Auth.Data;
using MinimalApi_Auth.Models.Endpoints;
using MinimalApi_Auth.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Opstartvalidatie: crash vroeg als secrets niet geconfigureerd zijn ──
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey == "SET_VIA_USER_SECRETS_OR_ENV_VAR")
{
    throw new InvalidOperationException(
        "JWT Key is niet geconfigureerd. Stel deze in via:\n" +
        "  dotnet user-secrets set \"Jwt:Key\" \"<jouw-secret>\"\n" +
        "of via een omgevingsvariabele: Jwt__Key");
}
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT Key moet minstens 32 tekens lang zijn.");
}

// ── Register Services ────────────────────────────────────────
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<HashService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddEndpointsApiExplorer();

// ── JWT Authentication ───────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        // Geen interne foutdetails lekken naar de client
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.Headers["WWW-Authenticate"] = "Bearer";
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ── Build App ────────────────────────────────────────────────
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// ── Map Endpoints ────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapKeysEndpoints();

app.Run();
