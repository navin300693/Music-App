var builder = WebApplication.CreateBuilder(args);


// Add SQL Server & Cosmos 
builder.AddSqlServerDbContext<MusicDbContext>("MusicDb");
builder.AddAzureBlobClient("Blobs");
// Add Cosmos DB Client with custom serialization options
// builder.AddAzureCosmosClient("Cosmos", configureClientOptions: options =>
// {
//     // This is the direct hook into the Cosmos SDK options within Aspire
//     options.SerializerOptions = new CosmosSerializationOptions
//     {
//         PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
//     };
// });
// Aspire injects the Endpoint URL into this specific key
// It will look like: https://dev-music-cfg.azconfig.io
// var appConfigEndpoint = builder.Configuration.GetConnectionString("AppConfig");

// if (!string.IsNullOrEmpty(appConfigEndpoint))
// {
//     //Add Azure App Configuration using Managed Identity (MSI)
//     builder.Configuration.AddAzureAppConfiguration(options =>
//     {
//         options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
//                // This allows your MSI to also pull secrets from Key Vault if needed
//                .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
//     });

//     Console.WriteLine($"--> Connected to Azure App Config (MSI): {appConfigEndpoint}");
// }
// else
// {
//     // Fallback: If no endpoint, .NET naturally uses appsettings.json
//     Console.WriteLine("--> No AppConfig endpoint found. Using local appsettings.json.");
// }

// // Add Versioning Services
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    // This tells .NET to look for the version in the URL: /v1/resource
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Formats version as 'v1', 'v2', etc.
    options.SubstituteApiVersionInUrl = true;
});

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();
// Adds the logic for /login, /register, etc.
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<MusicDbContext>();
builder.Services.AddAuthorization();
// Register Repositories (Data Access Layer)
// We use Scoped because we want one instance per HTTP request
builder.Services.AddScoped<ISongRepository, SongRepository>();
// Register Services (Business Logic Layer)
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IPolicyResolver, PolicyResolver>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IPlaybackService, PlaybackService>();
// --- [Creational Pattern: Keyed Factory via DI] ---
// Register different policies with a unique Key (PlanType)
builder.Services.AddKeyedScoped<IPlaybackPolicy, FreePlaybackPolicy>(PlanType.Free);
builder.Services.AddKeyedScoped<IPlaybackPolicy, PremiumPlaybackPolicy>(PlanType.Premium);
builder.Services.AddKeyedScoped<IPlaybackPolicy, ArtistPlaybackPolicy>(PlanType.Artist);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/debug/dp", (IDataProtectionProvider dp) =>
{
    var protector = dp.CreateProtector("test");
    var token = protector.Protect("hello");
    var result = protector.Unprotect(token);
    return new { token, result };
});

app.UseAuthentication();
app.UseAuthorization();
var identityGroup = app.MapGroup("api/v1/identity");
identityGroup.MapIdentityApi<ApplicationUser>();
app.MapHealthChecks("/health");
app.MapControllers();
app.UseExceptionHandler();
app.UseStatusCodePages(); // Returns 404/etc. instead of looping
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
    app.MapOpenApi();
    // 1. Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Music App API v1");
    });
    // 2. Scalar UI
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Music App API v1");
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });
    Console.WriteLine($"--> API PID : {Environment.ProcessId}");
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MusicDbContext>();
        // This creates the database and all tables defined in your DbContext
        // Note: This does NOT handle migrations, it just ensures the schema exists.
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.Run();