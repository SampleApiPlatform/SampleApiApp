//NEW
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDataAccessClient, DataAccessClientDapr>();

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






