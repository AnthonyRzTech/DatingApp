using WebMatcha.Components;
using WebMatcha.Services;
using WebMatcha.Hubs;
using WebMatcha.Models;
using WebMatcha.Middleware;
using DotNetEnv;
using Dapper;

// Load environment variables
Env.Load();

// Initialize Dapper type handlers
DapperConfig.Initialize();

var builder = WebApplication.CreateBuilder(args);

// Add logging with detailed configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

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
builder.Services.AddScoped<DatabaseOptimizationService>();
builder.Services.AddScoped<DatabaseSchemaService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CompleteAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IBlockReportService, BlockReportService>();
builder.Services.AddScoped<IProfileViewService, ProfileViewService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ServerSessionService>();

// Add HttpContextAccessor for server sessions
builder.Services.AddHttpContextAccessor();

// Add session support for temporary auth (will replace with JWT later)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Strict CSRF protection
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.Name = ".WebMatcha.Session";
});

// Add CORS with strict policy (CRITICAL security)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7036", "http://localhost:5192")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// TODO: Add these services when database is ready
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// builder.Services.AddFastEndpoints()

var app = builder.Build();

// Configure the HTTP request pipeline
// Global exception handler (IMPORTANT - improves error handling and security)
app.UseMiddleware<GlobalExceptionHandler>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Security headers (CRITICAL - subject requirement)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(self), camera=(), microphone=()");

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;");
    }

    await next();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles();
app.UseSession();
app.UseAntiforgery();

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

// User count endpoint
app.MapGet("/api/users/count", async (UserService userService) =>
{
    var count = await userService.GetUsersCountAsync();
    return Results.Ok(new { userCount = count });
});

// Debug endpoint to check users
app.MapGet("/api/debug/users", async (UserService userService) =>
{
    var allUsers = await userService.GetAllUsersAsync();
    var users = allUsers.Take(5).ToList();

    return Results.Ok(new
    {
        users = users.Select(u => new { u.Id, u.Username, u.Email })
    });
});

// Debug endpoint to check session
app.MapGet("/api/debug/session", (IHttpContextAccessor httpContextAccessor) =>
{
    var session = httpContextAccessor.HttpContext?.Session;
    var userId = session?.GetInt32("CurrentUserId");
    var username = session?.GetString("CurrentUsername");
    
    return Results.Ok(new 
    { 
        hasSession = session != null,
        isAvailable = session?.IsAvailable ?? false,
        userId = userId,
        username = username,
        sessionId = session?.Id
    });
});

// Email verification endpoint
app.MapGet("/api/verify-email/{token}", async (string token, CompleteAuthService authService) =>
{
    var success = await authService.VerifyEmailAsync(token);
    return success 
        ? Results.Ok(new { message = "Email verified successfully" })
        : Results.BadRequest(new { error = "Invalid or expired verification token" });
});

// Login endpoint for testing
app.MapPost("/api/login", async (HttpContext context, CompleteAuthService authService, ServerSessionService sessionService) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        return Results.BadRequest(new { error = "Username and password are required" });
    }

    var loginRequest = new LoginRequest { Username = username, Password = password };
    var result = await authService.LoginAsync(loginRequest);

    if (result.Success && result.User != null)
    {
        // Set session
        sessionService.SetCurrentUser(result.User);
        return Results.Ok(new {
            success = true,
            message = "Login successful",
            user = new { result.User.Id, result.User.Username, result.User.Email }
        });
    }

    return Results.BadRequest(new { error = result.Errors.FirstOrDefault() ?? "Login failed" });
});

// Password reset request endpoint
app.MapPost("/api/password-reset", async (HttpContext context, CompleteAuthService authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    
    if (string.IsNullOrEmpty(email))
    {
        return Results.BadRequest(new { error = "Email is required" });
    }
    
    await authService.SendPasswordResetAsync(email);
    return Results.Ok(new { message = "If an account exists with that email, reset instructions have been sent" });
});

// Password reset endpoint
app.MapPost("/api/reset-password", async (HttpContext context, CompleteAuthService authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var token = form["token"].ToString();
    var password = form["password"].ToString();
    
    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(password))
    {
        return Results.BadRequest(new { error = "Token and password are required" });
    }
    
    var success = await authService.ResetPasswordAsync(token, password);
    return success
        ? Results.Ok(new { message = "Password reset successfully" })
        : Results.BadRequest(new { error = "Invalid or expired reset token" });
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

// Login endpoint (updated to use CompleteAuthService)
app.MapPost("/auth/login", async (HttpContext context, CompleteAuthService authService, ServerSessionService sessionService) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var redirect = form["redirect"].ToString();

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        return Results.BadRequest(new { error = "Username and password are required" });
    }

    var loginRequest = new LoginRequest { Username = username, Password = password };
    var result = await authService.LoginAsync(loginRequest);

    if (result.Success && result.User != null)
    {
        // Set session using ServerSessionService (works properly in middleware context)
        sessionService.SetCurrentUser(result.User);

        // Redirect if requested, otherwise return JSON
        if (!string.IsNullOrEmpty(redirect))
        {
            return Results.Redirect(redirect);
        }

        return Results.Ok(new { success = true, redirectUrl = "/" });
    }

    return Results.BadRequest(new { error = string.Join(", ", result.Errors) });
});

// Logout endpoint
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    // Clear session data (simple approach for now)
    context.Session.Clear();
    return Results.Ok(new { message = "Logged out successfully" });
});

// Apply schema, optimizations and seed database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        // Ensure database schema exists
        var schemaService = scope.ServiceProvider.GetRequiredService<DatabaseSchemaService>();
        await schemaService.EnsureDatabaseSchemaAsync();
        Console.WriteLine("Database schema ensured successfully");

        // Apply database optimizations (indexes)
        var optimizer = scope.ServiceProvider.GetRequiredService<DatabaseOptimizationService>();
        await optimizer.ApplyOptimizationsAsync();
        Console.WriteLine("Database optimizations applied");

        // Seed database with 500 profiles if needed (CRITICAL - subject requirement)
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
        await seeder.SeedDatabaseAsync(500);
        Console.WriteLine("Database seeding completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during startup: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        Console.WriteLine($"Full exception: {ex}");
    }
}

app.Run();