using Azure.Identity;
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;
using SampleApi.Options;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);
// ---------------------------------------------------------
// 1KeyVaultUri loaded from the Container App Secret
// ---------------------------------------------------------
                                         
var keyVaultUri = builder.Configuration["keyvaulturi"];

if (string.IsNullOrWhiteSpace(keyVaultUri))
{
    throw new InvalidOperationException("KeyVaultUri configuration value is missing.");
}
// ---------------------------------------------------------
// Key Vault integrate
// ---------------------------------------------------
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential());

builder.Services.AddScoped<IDataAccessClient, DataAccessClientDapr>();

//Swagger
builder.Services.AddSwaggerDocumentation();

// Controllers
builder.Services.AddControllers();

// ---------------------------------------------------------
// ServiceTokenOptions load (Tokens from the Key Vault)
// ---------------------------------------------------------
builder.Services.Configure<ServiceTokenOptions>(options =>
{
    var token1 = builder.Configuration["sampleapiplatform-token-client1"];

    if (string.IsNullOrWhiteSpace(token1))
    {
        throw new InvalidOperationException("Token 'sampleapiplatform-token-client1' is missing from Key Vault.");
    }

    options.ValidTokens.Add(token1);
});

// ---------------------------------------------------------
// Token-Middleware registrieren
// ---------------------------------------------------------
builder.Services.AddSingleton<TokenValidationMiddleware>();

// ⭐ Register Authentication + JWT Bearer

// Authorization
builder.Services.AddControllersWithViews();
    //.AddMicrosoftIdentityUI();
//builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        jwtBearerOptions =>
        {
            // Set audiences INSIDE this callback so they aren't overwritten later
            jwtBearerOptions.TokenValidationParameters.ValidAudiences = new[]
            {
                "api://f9a3d163-e6e5-481b-9ea9-869076084bc7",  // v1 tokens (App ID URI)
                "f9a3d163-e6e5-481b-9ea9-869076084bc7"          // v2 tokens (client ID)
            };
            jwtBearerOptions.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
        },
        microsoftIdentityOptions =>
        {
            microsoftIdentityOptions.Instance = builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
            microsoftIdentityOptions.TenantId = builder.Configuration["AzureAd:TenantId"];
            microsoftIdentityOptions.ClientId = builder.Configuration["AzureAd:ClientId"];
        });
// Accept BOTH the App ID URI (v1 tokens) and the Client ID (v2 tokens).
// This makes validation robust regardless of which token version a client requests.
//builder.Services.Configure<JwtBearerOptions>(
//    JwtBearerDefaults.AuthenticationScheme,
//    options =>
//    {
//        var clientId = builder.Configuration["AzureAd:ClientId"];
//        var appIdUri = builder.Configuration["AzureAd:Audience"];
//
//        options.TokenValidationParameters.ValidAudiences = new[]
//        {
//            appIdUri,   // api://f9a3d163-e6e5-481b-9ea9-869076084bc7  (v1 tokens)
//            clientId    // f9a3d163-e6e5-481b-9ea9-869076084bc7          (v2 tokens)
//        };
//
//        // Fail fast on clock skew rather than silently accepting stale tokens
//        options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
//    });


//builder.Services.AddAuthorization();
// 3. Register the scope policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessAsUser", policy =>
        policy.RequireAssertion(ctx =>
        {
            // Check both 'scp' (v1) and 'scp2' (v2) claims
            var scopeClaim = ctx.User.FindFirst(c => c.Type == "scp" || c.Type == "scp2");
            if (scopeClaim == null) return false;

            // Split by space and check if 'access_as_user' is one of the scopes
            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return scopes.Contains("access_as_user", StringComparer.OrdinalIgnoreCase);
        }));
});
builder.Services.AddHttpClient("dapr", client =>
{
    client.BaseAddress = new Uri("http://localhost:3500/");
    client.DefaultRequestHeaders.Add("Dapr-TimeoutInSeconds", "30");
}); 


var app = builder.Build();
//app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Here we check if the token is valid or not
//put it before the authentication and Authorization
//blocks bots/scanners before they hit the auth logic
app.UseMiddleware<TokenValidationMiddleware>();

// ⭐ Authentication + Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
