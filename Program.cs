using WebMatcha.Components;
using WebMatcha.Services;
using WebMatcha.Data;
using WebMatcha.Hubs;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR for real-time features (will be used later)
builder.Services.AddSignalR();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add Mock Data Service (singleton for demo)
builder.Services.AddSingleton<MockDataService>();

// Add Database services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<DataSeederService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ServerSessionService>();

// Add HttpContextAccessor for server sessions
builder.Services.AddHttpContextAccessor();

// Add Database Context
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
    
// Add DbContextFactory for Blazor components (replaces AddDbContext for Blazor Server)
builder.Services.AddDbContextFactory<MatchaDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());
           
// Also add DbContext for services that need it directly
builder.Services.AddScoped<MatchaDbContext>(p => 
    p.GetRequiredService<IDbContextFactory<MatchaDbContext>>().CreateDbContext());

// Add session support for temporary auth (will replace with JWT later)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// TODO: Add these services when database is ready
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// builder.Services.AddFastEndpoints()

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseSession();

// TODO: Add these middlewares when ready
// app.UseAuthentication();
// app.UseAuthorization();
// app.UseFastEndpoints();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hubs
app.MapHub<ChatHub>("/hubs/chat");

// Health check endpoint
app.MapGet("/api/health", () => new
{
    status = "healthy",
    application = "WebMatcha",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
});

// Seed database endpoint (for development)
app.MapGet("/api/seed", async (DataSeederService seeder) =>
{
    try
    {
        await seeder.SeedDatabaseAsync(500);
        return Results.Ok(new { message = "Database seeded successfully with 500 users" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Logout endpoint
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    // Clear session data (simple approach for now)
    context.Session.Clear();
    return Results.Ok(new { message = "Logged out successfully" });
});

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MatchaDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

app.Run();