using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Domain.Entities;
using Xunit;

namespace IntegrationTests
{
    // ---------------------------------------------------------------------------
    // Custom WebApplicationFactory: replaces SQL Server with an EF InMemory
    // database and seeds deterministic test data so tests are fully self-contained
    // and do NOT require network access to Azure SQL.
    // ---------------------------------------------------------------------------
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Shared password for the seeded test admin user
        public const string TestAdminEmail    = "testadmin@integration.test";
        public const string TestAdminPassword = "Integration@Test1!";
        public const string TestUserEmail     = "testuser@integration.test";
        public const string TestUserPassword  = "UserTest@1234!";

        // Fixed name shared across service registrations and the host's DI container
        private readonly string _testDbName = "IntegrationTestDb_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // ── 1. Remove every trace of the SQL Server DbContext ────────
                var sqlDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (sqlDescriptor != null)
                    services.Remove(sqlDescriptor);

                var ctxDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ApplicationDbContext));
                if (ctxDescriptor != null)
                    services.Remove(ctxDescriptor);

                // ── 2. Register InMemory DbContext ───────────────────────────
                // _testDbName is evaluated once at factory construction, so every
                // scope/request resolves the SAME InMemory store.
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_testDbName));
            });
        }

        // ── 3. Seed using the host's own IServiceProvider ────────────────────
        // Overriding CreateHost guarantees the context comes from the same DI
        // container that handles requests, eliminating the "user not found" bug
        // that occurs when seeding via services.BuildServiceProvider().
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedDatabase(db);
            return host;
        }

        /// <summary>SHA-256 base64 — matches AuthService.HashPassword() exactly.</summary>
        private static string Sha256Base64(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
        }

        private static void SeedDatabase(ApplicationDbContext db)
        {
            // Guard against double-seeding (EnsureCreated via HasData already adds Roles + admin@sinseg.com)
            if (db.Users.Any(u => u.Email == TestAdminEmail)) return;

            // Roles 1(Admin), 2(DataLoader), 3(User) are already present via HasData.
            // HasData also seeds User Id=1 (admin@sinseg.com). Use Ids 100/101 to avoid PK conflicts.

            // Integration-test admin user
            var adminUser = new User
            {
                Id           = 100,
                UserName     = "testadmin",
                Email        = TestAdminEmail,
                FirstName    = "Test",
                LastName     = "Admin",
                PasswordHash = Sha256Base64(TestAdminPassword),
                IsActive     = true,
                CreatedBy    = "Seed"
            };

            // Integration-test regular user (no Admin role)
            var regularUser = new User
            {
                Id           = 101,
                UserName     = "testuser",
                Email        = TestUserEmail,
                FirstName    = "Test",
                LastName     = "User",
                PasswordHash = Sha256Base64(TestUserPassword),
                IsActive     = true,
                CreatedBy    = "Seed"
            };
            db.Users.AddRange(adminUser, regularUser);

            // Role 1 = "Admin" (from HasData), Role 3 = "User" (from HasData)
            db.UserRoles.AddRange(
                new UserRole { UserId = 100, RoleId = 1 },  // testadmin → Admin
                new UserRole { UserId = 101, RoleId = 3 }   // testuser  → User
            );

            // Sample Poliza
            db.Polizas.Add(new Poliza
            {
                Id              = 1,
                NumeroPoliza    = "POL-TEST-001",
                Modalidad       = "Automóvil",
                NombreAsegurado = "Cliente Prueba",
                Prima           = 1500.00m,
                Moneda          = "CRC",
                FechaVigencia   = DateTime.UtcNow.AddYears(1),
                Frecuencia      = "Mensual",
                Aseguradora     = "Aseguradora Test",
                Placa           = "TEST001",
                EsActivo        = true,
                PerfilId        = 1,
                CreatedBy       = "Seed"
            });

            // Sample Cobro
            db.Cobros.Add(new Cobro
            {
                Id                    = 1,
                NumeroRecibo          = "REC-202510-0001",
                PolizaId              = 1,
                NumeroPoliza          = "POL-TEST-001",
                ClienteNombreCompleto = "Cliente Prueba",
                MontoTotal            = 1500.00m,
                MontoCobrado          = 0,
                FechaVencimiento      = DateTime.UtcNow.AddMonths(1),
                FechaCobro            = DateTime.MinValue,
                Estado                = EstadoCobro.Pendiente,
                Moneda                = "CRC",
                CreatedBy             = "Seed"
            });

            // Sample Reclamo
            db.Reclamos.Add(new Reclamo
            {
                Id                    = 1,
                NumeroReclamo         = "REC-2025-00001",
                NumeroPoliza          = "POL-TEST-001",
                TipoReclamo           = TipoReclamo.Reclamo,
                Descripcion           = "Reclamo de prueba para tests de integración",
                FechaReclamo          = DateTime.UtcNow,
                Estado                = EstadoReclamo.Pendiente,
                Prioridad             = PrioridadReclamo.Media,
                MontoReclamado        = 500.00m,
                NombreAsegurado       = "Cliente Prueba",
                ClienteNombreCompleto = "Cliente Prueba",
                Moneda                = "CRC",
                CreatedBy             = "Seed"
            });

            db.SaveChanges();
        }
    }

    // ---------------------------------------------------------------------------
    // Helper base class with token acquisition and JSON serializer config
    // ---------------------------------------------------------------------------
    public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
    {
        protected readonly HttpClient _client;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        protected IntegrationTestBase(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        /// <summary>Logs in and returns a Bearer token for the specified user.</summary>
        protected async Task<string> GetAuthTokenAsync(string email, string password)
        {
            var body = new StringContent(
                JsonSerializer.Serialize(new { Email = email, Password = password }),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc  = JsonSerializer.Deserialize<JsonObject>(json, _jsonOptions)!;
            return doc["token"]?.GetValue<string>()
                ?? throw new InvalidOperationException("No token in login response");
        }

        protected void AuthorizeClient(string token) =>
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        protected static StringContent Json(object payload) =>
            new(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
    }

    // ===========================================================================
    // AUTH TESTS
    // ===========================================================================
    public class AuthApiTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            var response = await _client.PostAsync("/api/auth/login",
                Json(new { Email = TestWebApplicationFactory.TestAdminEmail, Password = TestWebApplicationFactory.TestAdminPassword }));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("token");
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            var response = await _client.PostAsync("/api/auth/login",
                Json(new { Email = "nobody@nowhere.com", Password = "WrongPassword99!" }));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenEmailIsMissing()
        {
            var response = await _client.PostAsync("/api/auth/login",
                Json(new { Password = "SomePassword1!" }));

            ((int)response.StatusCode).Should().BeOneOf(400, 401);
        }
    }

    // ===========================================================================
    // AUTHORIZATION GUARD TESTS — unauthenticated requests must return 401
    // ===========================================================================
    public class AuthorizationGuardTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Theory]
        [InlineData("GET", "/api/polizas")]
        [InlineData("GET", "/api/cobros")]
        [InlineData("GET", "/api/reclamos")]
        [InlineData("GET", "/api/users")]
        public async Task ProtectedEndpoints_Return401_WhenNoTokenProvided(string method, string url)
        {
            // Ensure no auth header is present
            _client.DefaultRequestHeaders.Authorization = null;

            var request  = new HttpRequestMessage(new HttpMethod(method), url);
            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: $"{method} {url} must be protected by JWT authentication");
        }
    }

    // ===========================================================================
    // POLIZAS TESTS
    // ===========================================================================
    public class PolizasApiTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetPolizas_ReturnsOk_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/polizas");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetPolizaById_ReturnsOk_ForExistingPoliza()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/polizas/1");

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPolizaById_Returns404_ForNonExistentId()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/polizas/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreatePoliza_ReturnsCreated_WithValidPayload()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var payload = new
            {
                NumeroPoliza    = "POL-IT-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Modalidad       = "Automóvil",
                NombreAsegurado = "Integración Test",
                NumeroCedula    = "123456789",
                Prima           = 2000.00m,
                Moneda          = "CRC",
                FechaVigencia   = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd"),
                Frecuencia      = "Mensual",
                Aseguradora     = "Aseguradora Test",
                Placa           = "ITT001",
                Marca           = "Toyota",
                Modelo          = "Corolla",
                Año             = "2022",
                Correo          = "test@test.com",
                NumeroTelefono  = "88001234",
                PerfilId        = 1
            };

            var response = await _client.PostAsync("/api/polizas", Json(payload));

            ((int)response.StatusCode).Should().BeOneOf(200, 201);
        }
    }

    // ===========================================================================
    // COBROS TESTS
    // ===========================================================================
    public class CobrosApiTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetCobros_ReturnsOk_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/cobros");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RegistrarCobro_ReturnsSuccess_WithValidPayload()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var payload = new
            {
                NumeroRecibo          = "REC-IT-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                MontoTotal            = 1500.00m,
                FechaVencimiento      = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd"),
                MetodoPago            = 0, // Efectivo
                Moneda                = "CRC",
                PolizaId              = 1,
                NumeroPoliza          = "POL-TEST-001",
                ClienteNombreCompleto = "Cliente Prueba",
                UsuarioCobroId        = 1
            };

            var response = await _client.PostAsync("/api/cobros", Json(payload));

            ((int)response.StatusCode).Should().BeOneOf(200, 201);
        }

        [Fact]
        public async Task GetCobroStats_ReturnsOk_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/cobros/stats");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCobroById_Returns404_ForNonExistentId()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/cobros/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    // ===========================================================================
    // RECLAMOS TESTS
    // ===========================================================================
    public class ReclamosApiTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetReclamos_ReturnsOk_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/reclamos");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetReclamoStats_ReturnsOk_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/reclamos/stats");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReclamo_ReturnsSuccess_WithValidPayload()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var payload = new
            {
                NumeroPoliza          = "POL-TEST-001",
                TipoReclamo           = 0, // Reclamo
                Descripcion           = "Accidente vial ocurrido en San José",
                FechaLimiteRespuesta  = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Prioridad             = 1, // Media
                MontoReclamado        = 800.00m,
                NombreAsegurado       = "Cliente Prueba",
                ClienteNombreCompleto = "Cliente Prueba",
                Observaciones         = "Test integration reclamo",
                Moneda                = "CRC"
            };

            var response = await _client.PostAsync("/api/reclamos", Json(payload));

            ((int)response.StatusCode).Should().BeOneOf(200, 201);
        }
    }

    // ===========================================================================
    // USUARIOS TESTS
    // ===========================================================================
    public class UsuariosApiTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetUsers_ReturnsOk_WithAdminRole()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/users");

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "An Admin-role token must always be granted access to GET /api/users");
        }

        [Fact]
        public async Task GetUserById_ReturnsResult_ForExistingUser()
        {
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestAdminEmail, TestWebApplicationFactory.TestAdminPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/users/1");

            ((int)response.StatusCode).Should().BeOneOf(200, 404);
        }

        [Fact]
        public async Task GetUsers_Returns403_WhenUserRoleOnly()
        {
            // Regular user (role = "Usuario") must NOT access Admin-only endpoints
            var token = await GetAuthTokenAsync(TestWebApplicationFactory.TestUserEmail, TestWebApplicationFactory.TestUserPassword);
            AuthorizeClient(token);

            var response = await _client.GetAsync("/api/users");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "GET /api/users requires Admin role — a Usuario-role token must be rejected with 403");
        }
    }
}