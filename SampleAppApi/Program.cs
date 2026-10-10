using Azure.Identity;
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;
using SampleApi.Options;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);


//This is for the redirect microsoft login
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Container Apps sits behind Envoy with dynamic internal IPs,
    // so we must clear the known proxy/network allow-lists.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
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

//DEBUG
var test = builder.Configuration["AzureAdClientSecret1"];
File.WriteAllText("/tmp/secret_check.txt", $"Len={test?.Length ?? -1}");
Console.WriteLine($"[KV] AzureAdClientSecret1 visible: {(string.IsNullOrEmpty(test) ? "NO" : $"YES (len={test.Length})")}");
    


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
// 1. Authentication & JWT Bearer
var clientId = builder.Configuration["AzureAd:ClientId"];
var appIdUri = builder.Configuration["AzureAd:Audience"];
//var scope    = builder.Configuration["AzureAd:Scope"] ?? "access_as_user";

/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        jwtBearerOptions =>
        {
            jwtBearerOptions.MapInboundClaims = false;

            jwtBearerOptions.TokenValidationParameters.ValidAudiences = new[] { appIdUri, clientId };
            jwtBearerOptions.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
        },
        microsoftIdentityOptions =>
        {
            microsoftIdentityOptions.Instance = builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
            microsoftIdentityOptions.TenantId = builder.Configuration["AzureAd:TenantId"];
            microsoftIdentityOptions.ClientId = builder.Configuration["AzureAd:ClientId"];
        });*/

// ---------------------------------------------------------
// 1) OIDC = Browser-Login (Redirect, Cookie-Session)
// ---------------------------------------------------------

var clientSecret = builder.Configuration["AzureAdClientSecret1"];

//DEBUG
Console.WriteLine($"[DIAG] KV secret read: {(string.IsNullOrEmpty(clientSecret) ? "EMPTY" : $"len={clientSecret.Length}")}");
//
if (string.IsNullOrWhiteSpace(clientSecret))
{
    throw new InvalidOperationException("ClientSecret not found in Key Vault.");
}
builder.Configuration["AzureAd:ClientSecret"] = clientSecret;

//DEBUG
var verify = builder.Configuration["AzureAd:ClientSecret"];
Console.WriteLine($"[DIAG] AzureAd:ClientSecret set: {(string.IsNullOrEmpty(verify) ? "EMPTY" : $"len={verify.Length}")}");
//
//builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
//    .EnableTokenAcquisitionToCallDownstreamApi(new[] { "api://f9a3d163-e6e5-481b-9ea9-869076084bc7/access_as_user" })
//    .AddInMemoryTokenCaches();

builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApi(
        jwtBearerOptions =>
        {
            jwtBearerOptions.MapInboundClaims = false;
            jwtBearerOptions.TokenValidationParameters.ValidAudiences = new[] { appIdUri, clientId };
            jwtBearerOptions.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
        },
        microsoftIdentityOptions => { },
        JwtBearerDefaults.AuthenticationScheme);




// 2. Authorization Policy
/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessAsUser", policy =>
        policy.RequireAssertion(ctx =>
        {
            var scopeClaim = ctx.User.FindFirst(c => c.Type == "scp" || c.Type == "scp2");
            if (scopeClaim == null) return false;

            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
        }));
});*/


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
/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessAsUser", policy =>
        policy.RequireAssertion(ctx =>
        {
            // Debug: Print all claims to the console
            if (ctx.User.Identity?.IsAuthenticated == true)
            {
                var claims = string.Join(", ", ctx.User.Claims.Select(c => $"{c.Type}: '{c.Value}'"));
                //Console.WriteLine($"DEBUG CLAIMS: {claims}");
                
                var scp = ctx.User.FindFirst("scp")?.Value;
                //Console.WriteLine($"DEBUG scp: '{scp}'");
            }

            var scopeClaim = ctx.User.FindFirst(c => c.Type == "scp" || c.Type == "scp2");
            
            if (scopeClaim == null) return false;

            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return scopes.Contains("access_as_user", StringComparer.OrdinalIgnoreCase);
        }));
});*/
var scope = builder.Configuration["AzureAd:Scope"] ?? "access_as_user";

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



builder.Services.AddHttpClient("dapr", client =>
{
    client.BaseAddress = new Uri("http://localhost:3500/");
    client.DefaultRequestHeaders.Add("Dapr-TimeoutInSeconds", "30");
}); 

var app = builder.Build();

//DEBUG
// TEMPORARY: Allow /check-secret to bypass auth entirely
app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/check-secret"),
    appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            var secret = context.RequestServices.GetRequiredService<IConfiguration>()["AzureAdClientSecret1"];
            await context.Response.WriteAsync($"Len={secret?.Length ?? -1}");
        });
    }
);
//

// For the microsoft login ⭐ MUST be first — fixes Scheme/Host from X-Forwarded-* headers
app.UseForwardedHeaders();

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
