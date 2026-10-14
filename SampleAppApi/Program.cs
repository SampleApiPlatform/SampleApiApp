//NEW
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDataAccessClient, DataAccessClientDapr>();

// Register services
//builder.Services.AddScoped<IMovieRepository, MovieRepository>();
//builder.Services.AddScoped<IMovieService, MovieService>();
//builder.Services.AddScoped<IValidator<Movie>, MovieValidator>();

// EF Core SQL Azure with retry in case that there are transient connection issues
//builder.Services.AddDbContext<MoviesDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("MoviesDb"),
//        sqlOptions => sqlOptions.EnableRetryOnFailure(
//            maxRetryCount: 5,
//            maxRetryDelay: TimeSpan.FromSeconds(10),
//            errorNumbersToAdd: null
//        )
//    )
//);
//Another database from another area of the business
//builder.Services.AddDbContext<BillingDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        sqlOptions => sqlOptions.EnableRetryOnFailure(
//            maxRetryCount: 5,
//            maxRetryDelay: TimeSpan.FromSeconds(10),
//            errorNumbersToAdd: null
//        )
//    )
//);

//Swagger
builder.Services.AddSwaggerDocumentation();

// Controllers
builder.Services.AddControllers();

// App->App
//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.Authority = "https://login.microsoftonline.com/9ca003dc-da5d-4b31-808b-9d52337fbf6a/v2.0";
//        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//        {
//            ValidateAudience = true,
//            ValidAudience = "api://c2c22245-dc03-4cb8-ad4e-4d3471f04ab6"
//        };
//    });
// Authorization
//builder.Services.AddAuthorization();

// JWT from microsoft account
// App->App  (accepts the same token sampleappapi received)
var clientId = builder.Configuration["AzureAd:ClientId"];
var appIdUri = builder.Configuration["AzureAd:Audience"];
var tenantId = builder.Configuration["AzureAd:TenantId"];
var scope    = builder.Configuration["AzureAd:Scope"] ?? "access_as_user";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
//
//builder.Configuration.AddAzureKeyVault(
//    new Uri(keyVaultUri),
//    new DefaultAzureCredential());    

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




//OLD
/*using Azure.Identity;
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;
using SampleApi.Options;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;


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

builder.Services.AddHttpContextAccessor();
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
    
var clientId = builder.Configuration["AzureAd:ClientId"];
var appIdUri = builder.Configuration["AzureAd:Audience"];

// ---------------------------------------------------------
// 1) OIDC = Browser-Login (Redirect, Cookie-Session)
// ---------------------------------------------------------

var clientSecret = builder.Configuration["AzureAdClientSecret1"];


if (string.IsNullOrWhiteSpace(clientSecret))
{
    throw new InvalidOperationException("ClientSecret not found in Key Vault.");
}
builder.Configuration["AzureAd:ClientSecret"] = clientSecret;



//Authetication logging into Microsoft and getting to get the JWT
builder.Configuration["AzureAd:CallbackPath"] = "/signin-oidc";
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<OpenIdConnectOptions>(
    OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.Scope.Add("api://f9a3d163-e6e5-481b-9ea9-869076084bc7/access_as_user");
        // Surface login failures as readable text instead of a bare 500.
        options.Events.OnRemoteFailure = ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "text/plain";
            return ctx.Response.WriteAsync($"Login failed: {ctx.Failure?.Message}");
        };
    });



builder.Services.AddTokenAcquisition();
builder.Services.AddInMemoryTokenCaches();

builder.Services.AddAuthentication()
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
            microsoftIdentityOptions.TenantId  = builder.Configuration["AzureAd:TenantId"];
            microsoftIdentityOptions.ClientId  = builder.Configuration["AzureAd:ClientId"];
        },
        JwtBearerDefaults.AuthenticationScheme);

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
*/

