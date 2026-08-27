
using SampleAppApi.Extensions;
using SampleAppApi.Interfaces.ExternalServices;
using SampleAppApi.Services.External;
using NuGet.SampleSharedModels.Interfaces;
using NuGet.SampleSharedModels.Services;
var builder = WebApplication.CreateBuilder(args);

// Load JWT settings
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings.GetValue<string>("Key")
    ?? throw new Exception("JWT Key is missing in configuration");

var issuer = jwtSettings.GetValue<string>("Issuer")
    ?? throw new Exception("JWT Issuer is missing in configuration");

var audience = jwtSettings.GetValue<string>("Audience")
    ?? throw new Exception("JWT Audience is missing in configuration");



// Register services
var sharedServicesUrl = builder.Configuration["ServiceUrls:SharedServices"];
ArgumentException.ThrowIfNullOrWhiteSpace(sharedServicesUrl);
builder.Services.AddHttpClient<ISharedServicesClient, SharedServicesClient>(client =>
{
    client.BaseAddress = new Uri(sharedServicesUrl);
});
// Register services
var dataAccessUrl = builder.Configuration["ServiceUrls:DataAccessServices"];
ArgumentException.ThrowIfNullOrWhiteSpace(dataAccessUrl);
builder.Services.AddHttpClient<IDataAccessClient, DataAccessClient>(client =>
{
    client.BaseAddress = new Uri(dataAccessUrl);
});


//builder.Services.AddScoped<IMovieService, MovieService>();
//builder.Services.AddScoped<IValidator<Movie>, MovieValidator>();
//builder.Services.AddScoped<IValidator<User>, UserValidator>();
//builder.Services.AddScoped<IMovieRepository, MovieRepository>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();

//builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();


//Register AuthService
//builder.Services.AddScoped<IAuthService, AuthService>();

// EF Core InMemory
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseInMemoryDatabase("SampleDb"));

// EF Core SQL Azure
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("sampleApi")));

// EF Core SQL Azure with retry in case that there are transient connection issues
//builder.Services.AddDbContext<AppDbContext>(options =>
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

// Loging: This ensures:
// Console logs → Azure Container Apps,
// Azure diagnostics → Azure Monitor
// Everything → Log Analytics Workspace
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.AddAzureWebAppDiagnostics();






// Controllers
builder.Services.AddControllers();

// ⭐ Register Authentication + JWT Bearer


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
         ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = issuer,
        ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(key)
        )
    };
});

// Authorization
builder.Services.AddAuthorization();

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

//Test the has of the database on creation to see if it is the same as when is invoked by the user
// different values indicate that many instances exist and records can be not found.
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    Console.WriteLine("Startup DbContext hash: " + db.GetHashCode());
//}


// Test if user exist : uncomment this line to verify if you cannot login
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//
//    db.Database.EnsureDeleted();   // optional, but useful while debugging
//    db.Database.EnsureCreated();   // this is what triggers HasData
//
//    var users = db.Users.ToList();
//    Console.WriteLine($"Seeded users: {users.Count}");
//    foreach (var user in users)
//    {
//        Console.WriteLine("---- USER ----");
//        Console.WriteLine("Id: " + user.Id);
//        Console.WriteLine("Email: " + user.Email);
//        Console.WriteLine("Salt: " + user.Salt);
//        Console.WriteLine("PasswordHash: " + user.PasswordHash);
//    }

//}


app.Run();
