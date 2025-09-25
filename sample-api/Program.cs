using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

builder.Services.AddHttpClient("Keycloak")
    .ConfigurePrimaryHttpMessageHandler(() => handler);

var configuration = builder.Configuration;

// URL du realm Keycloak
var keycloakIssuer = configuration["KeycloakIssuer"];
var keycloakAudience = configuration["ClientID"];

// Authentification avec JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakIssuer;
    options.Audience = keycloakAudience;

    // Pour éviter les erreurs en local si Keycloak n’est pas en HTTPS
    options.RequireHttpsMetadata = false;

    // 👉 le point important pour accepter le certificat auto-signé
    options.BackchannelHttpHandler = handler;

    // Validation supplémentaire possible
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidAudience = keycloakAudience,
        ValidateIssuer = false,
        ValidIssuer = keycloakIssuer,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false
    };

    // 👉 Ajout du handler d’événements pour loguer les erreurs
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Auth failed: {context.Exception}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("URL: " + context.Request.Host.Value);
                Console.WriteLine("Response: " + context.Response.StatusCode);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validé pour: {context.Principal.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
});


builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) => "Hello World ! " + context.User.FindFirst("name")?.Value).RequireAuthorization();

app.Run();
