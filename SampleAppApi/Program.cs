//NEW
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NuGet.SampleSharedModels.Interfaces.ServiceBus;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;
using SampleAppApi.Services.External;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedScoped<IDataAccessClient, DataAccessClientDapr>("dapr");
builder.Services.AddKeyedScoped<IDataAccessClient, DataAccessClientBus>("bus");
//builder.Services.AddScoped<IDataAccessClient, DataAccessClientDapr>();
//builder.Services.AddScoped<IDataAccessBusClient, DataAccessClientBus>();
builder.Services.AddScoped<IBusSender, BusSender>();

// Azure Service Bus
builder.Services.AddSingleton(sp =>
{
    // reading values from appsettings.json
    var cfg = sp.GetRequiredService<IConfiguration>();
    var ns  = cfg["ServiceBus:FullyQualifiedNamespace"]
              ?? throw new InvalidOperationException("ServiceBus:FullyQualifiedNamespace missing");
    var q   = cfg["ServiceBus:QueueName"]
              ?? throw new InvalidOperationException("ServiceBus:QueueName missing");

    var client = new ServiceBusClient(ns, new DefaultAzureCredential());
    return client.CreateSender(q);      // disposed when the application shuts down
});

//Swagger
builder.Services.AddSwaggerDocumentation();

// Controllers
builder.Services.AddControllers();

// JWT from microsoft account
// App->App  (accepts the same token sampleappapi received)
var clientId = builder.Configuration["AzureAd:ClientId"];
var appIdUri = builder.Configuration["AzureAd:Audience"];
var tenantId = builder.Configuration["AzureAd:TenantId"];
var scope    = builder.Configuration["AzureAd:Scope"] ?? "access_as_user";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudiences   = new[] { appIdUri, clientId },
            ClockSkew        = TimeSpan.FromMinutes(2)
        };
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessAsUser", policy =>
        policy.RequireAssertion(ctx =>
        {
            var scopeClaim = ctx.User.FindFirst(c => c.Type == "scp");
            if (scopeClaim is null) return false;

            return scopeClaim.Value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains(scope, StringComparer.OrdinalIgnoreCase);
        }));
});
// we read the KeyVaultUrui from the environment variables
// the value reference to a url to the Key Vault
var keyVaultUri = builder.Configuration["KeyVaultUri"];


if (string.IsNullOrWhiteSpace(keyVaultUri))
{
    throw new InvalidOperationException("KeyVaultUri configuration value is missing.");
}
// ---------------------------------------------------------
// Key Vault integrate
// We access the key vault with the System Identity
// Then we can query durectly values from the Key Vault as:
// var clientSecret = builder.Configuration["AzureAdClientSecret2"];
// ---------------------------------------------------
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential());    

var ai_connectionString = builder.Configuration["applications-insights-connection-string"];
// Zusätzliche Instrumentierung VOR dem Distro registrieren
builder.Services.ConfigureOpenTelemetryTracerProvider((sp, b) => b
    .AddSource("sampleapiplatform")
    .AddEntityFrameworkCoreInstrumentation());

builder.Services.ConfigureOpenTelemetryMeterProvider((sp, b) => b
    .AddRuntimeInstrumentation());

// Distro bringt ASP.NET Core, HttpClient, SqlClient, ILogger, Standard-Metriken
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(o => o.ConnectionString = ai_connectionString);


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("dapr", client =>
{
    client.BaseAddress = new Uri("http://localhost:3500/");
    client.DefaultRequestHeaders.Add("Dapr-TimeoutInSeconds", "30");
}); 

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();


if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ⭐ Authentication + Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();






