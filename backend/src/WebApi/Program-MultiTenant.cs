// Program.cs - Configuración Multi-Tenant para SIINADSEG

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using WebApi.Data.MultiTenant;
using WebApi.Services.MultiTenant;
using WebApi.Middleware;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// ================================
// LOGGING CONFIGURATION
// ================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/siinadseg-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ================================
// SERVICES CONFIGURATION
// ================================

// Database Contexts
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDatabase") ??
        "Server=(localdb)\\mssqllocaldb;Database=SinsegMaster;Trusted_Connection=true;MultipleActiveResultSets=true"));

// TenantDbContext se registra como Scoped para inyección dinámica
builder.Services.AddDbContext<TenantDbContext>((serviceProvider, options) =>
{
    // La configuración real se hace en OnConfiguring del TenantDbContext
    // basado en el tenant actual
});

// Multi-Tenant Services
builder.Services.AddTenantServices();
builder.Services.AddScoped<TenantDbContextFactory>();

// Memory Cache para tenant caching
builder.Services.AddMemoryCache();

// HTTP Context Accessor (requerido para TenantContext)
builder.Services.AddHttpContextAccessor();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "https://*.siinadseg.com",
            "https://siinadseg.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-here-make-it-long-and-complex";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SIINADSEG";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SIINADSEG-Users";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTenant", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("tenant_id", c => !string.IsNullOrEmpty(c))));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));

    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("SuperAdmin"));
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIINADSEG Multi-Tenant API",
        Version = "v1",
        Description = "API para el sistema de seguros SIINADSEG con soporte multi-empresa",
        Contact = new OpenApiContact
        {
            Name = "SIINADSEG Support",
            Email = "support@siinadseg.com"
        }
    });

    // JWT Authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include tenant information in Swagger
    c.DocumentFilter<TenantSwaggerDocumentFilter>();
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContext<MasterDbContext>()
    .AddCheck<TenantHealthCheck>("tenant-health");

// AutoMapper (mantener configuración existente)
builder.Services.AddAutoMapper(typeof(Program));

// Application Services (agregar tus servicios existentes aquí)
// builder.Services.AddScoped<IPolizaService, PolizaService>();
// builder.Services.AddScoped<ICobroService, CobroService>();
// etc.

// Encoding provider for UTF-8 support
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// ================================
// APPLICATION PIPELINE
// ================================

var app = builder.Build();

// Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIINADSEG Multi-Tenant API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowSpecificOrigins");

// Authentication & Authorization
app.UseAuthentication();

// CRITICAL: Tenant Middleware MUST be after Authentication but before Authorization
app.UseTenantMiddleware();

app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// Default route for tenant detection testing
app.MapGet("/", async (ITenantContext tenantContext) =>
{
    if (tenantContext.HasTenant)
    {
        return Results.Ok(new
        {
            message = "SIINADSEG Multi-Tenant API",
            tenant = tenantContext.CurrentTenant?.CompanyName,
            tenantId = tenantContext.TenantId,
            timestamp = DateTime.UtcNow
        });
    }
    
    return Results.Ok(new
    {
        message = "SIINADSEG Multi-Tenant API",
        tenant = "No tenant detected",
        timestamp = DateTime.UtcNow
    });
});

// ================================
// DATABASE INITIALIZATION
// ================================

// Ensure Master Database is created and migrated
using (var scope = app.Services.CreateScope())
{
    try
    {
        var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        await masterContext.Database.EnsureCreatedAsync();
        
        Log.Information("Master Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to initialize Master Database");
        throw;
    }
}

// ================================
// START APPLICATION
// ================================

try
{
    Log.Information("Starting SIINADSEG Multi-Tenant API");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ================================
// HELPER CLASSES
// ================================

/// <summary>
/// Health check for tenant system
/// </summary>
public class TenantHealthCheck : IHealthCheck
{
    private readonly ITenantService _tenantService;

    public TenantHealthCheck(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we can retrieve tenants
            var tenants = await _tenantService.GetAllTenantsAsync();
            
            var data = new Dictionary<string, object>
            {
                { "totalTenants", tenants.Count },
                { "activeTenants", tenants.Count(t => t.IsActive) }
            };

            return HealthCheckResult.Healthy("Tenant system is working", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Tenant system is not working", ex);
        }
    }
}

/// <summary>
/// Swagger document filter to include tenant information
/// </summary>
public class TenantSwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Info.Description += "\n\n**Multi-Tenant Configuration:**\n" +
            "- Use subdomain: `{tenant}.siinadseg.com`\n" +
            "- Or header: `X-Tenant-ID: {tenant}`\n" +
            "- Or query param: `?tenant={tenant}`\n\n" +
            "**Default Tenant:** `empresa-default`";

        // Add tenant parameter to all operations
        var tenantParameter = new OpenApiParameter
        {
            Name = "X-Tenant-ID",
            In = ParameterLocation.Header,
            Description = "Tenant identifier",
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        };

        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Parameters ??= new List<OpenApiParameter>();
                
                // No agregar el parámetro si ya existe
                if (!operation.Parameters.Any(p => p.Name == "X-Tenant-ID"))
                {
                    operation.Parameters.Add(tenantParameter);
                }
            }
        }
    }
}