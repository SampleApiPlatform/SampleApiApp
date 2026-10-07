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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();
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
