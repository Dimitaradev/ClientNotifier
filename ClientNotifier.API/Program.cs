using ClientNotifier.Core.Services;
using ClientNotifier.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//DB connection
builder.Services.AddDbContext<NotifierContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<ExcelImportService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(ClientNotifier.Core.Mappings.AutoMapperProfile).Assembly);

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

//controllers and swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ClientNotifier API", Version = "v1" });
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var databasePath = configuration["DatabaseSettings:DatabasePath"] ?? "Data";
        
        // Convert relative path to absolute path
        if (!Path.IsPathRooted(databasePath))
        {
            databasePath = Path.Combine(Directory.GetCurrentDirectory(), databasePath);
        }
        
        // Ensure the database directory exists
        if (!Directory.Exists(databasePath))
        {
            Directory.CreateDirectory(databasePath);
            logger.LogInformation($"Created database directory at: {databasePath}");
        }
        
        // Update connection string with absolute path
        var context = scope.ServiceProvider.GetRequiredService<NotifierContext>();
        var dbPath = Path.Combine(databasePath, "ClientNotifier.db");
        context.Database.SetConnectionString($"Data Source={dbPath}");
        
        // Initialize database if auto-migrations are enabled
        var enableAutoMigrations = configuration.GetValue<bool>("DatabaseSettings:EnableAutoMigrations", true);
        if (enableAutoMigrations)
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync();
            
            // Seed test data if configured
            var seedTestData = configuration.GetValue<bool>("DatabaseSettings:SeedTestData", false);
            if (seedTestData || app.Environment.IsDevelopment())
            {
                await dbInitializer.SeedTestDataAsync();
            }
        }
        
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization");
        throw;
    }
}

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentPolicy");
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();