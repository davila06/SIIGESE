using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Application.Mappings;
using Application.Services;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;

namespace UnitTests.Services
{
    // ═══════════════════════════════════════════════════════════════════════════
    // AUTH SERVICE TESTS
    // ═══════════════════════════════════════════════════════════════════════════
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository>                _mockUserRepo;
        private readonly Mock<IRoleRepository>                _mockRoleRepo;
        private readonly Mock<IPasswordResetTokenRepository>  _mockTokenRepo;
        private readonly Mock<IEmailService>                  _mockEmailService;
        private readonly Mock<IConfiguration>                 _mockConfig;
        private readonly Mock<ILogger<AuthService>>           _mockLogger;
        private readonly Mock<ITokenBlacklistService>         _mockBlacklist;
        private readonly AuthService                          _authService;

        public AuthServiceTests()
        {
            _mockUserRepo     = new Mock<IUserRepository>();
            _mockRoleRepo     = new Mock<IRoleRepository>();
            _mockTokenRepo    = new Mock<IPasswordResetTokenRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockConfig       = new Mock<IConfiguration>();
            _mockLogger       = new Mock<ILogger<AuthService>>();
            _mockBlacklist    = new Mock<ITokenBlacklistService>();

            _mockConfig.Setup(x => x["Jwt:Secret"]).Returns("test-secret-key-for-unit-testing-min32chars!!");
            _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfig.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
            _mockConfig.Setup(x => x["Jwt:ExpirationHours"]).Returns("8");

            _authService = new AuthService(
                _mockUserRepo.Object, _mockRoleRepo.Object, _mockTokenRepo.Object,
                _mockEmailService.Object, _mockConfig.Object, _mockLogger.Object,
                _mockBlacklist.Object);
        }

        /// <summary>
        /// SHA-256 base64 hash — matches the AuthService.HashPassword() implementation.
        /// </summary>
        private static string Sha256Base64(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private User BuildActiveUser(string email, string password) => new()
        {
            Id           = 1,
            Email        = email,
            PasswordHash = Sha256Base64(password),
            IsActive     = true,
            UserRoles    = new List<UserRole>()
        };

        // ── Login — success ──────────────────────────────────────────────────
        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnTokenAndUser()
        {
            var request = new LoginRequestDto { Email = "admin@test.com", Password = "Admin123!" };
            var user    = BuildActiveUser(request.Email, request.Password);

            _mockUserRepo.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _mockUserRepo.Setup(x => x.GetUserWithRolesAsync(user.Id)).ReturnsAsync(user);

            var result = await _authService.LoginAsync(request);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.User.Email.Should().Be(request.Email);
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        // ── Login — wrong password ───────────────────────────────────────────
        [Fact]
        public async Task LoginAsync_WrongPassword_ShouldThrowUnauthorizedAccessException()
        {
            var request = new LoginRequestDto { Email = "admin@test.com", Password = "WrongPassword!" };
            var user    = BuildActiveUser(request.Email, "CorrectPassword!");

            _mockUserRepo.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*incorrectos*");
        }

        // ── Login — email not found ──────────────────────────────────────────
        [Fact]
        public async Task LoginAsync_EmailNotFound_ShouldThrowUnauthorizedAccessException()
        {
            var request = new LoginRequestDto { Email = "ghost@nowhere.com", Password = "AnyPass1!" };
            _mockUserRepo.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

            Func<Task> act = async () => await _authService.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*incorrectos*");
        }

        // ── Login — inactive user ────────────────────────────────────────────
        [Fact]
        public async Task LoginAsync_InactiveUser_ShouldThrowUnauthorizedAccessException()
        {
            var request = new LoginRequestDto { Email = "inactive@test.com", Password = "Pass123!" };
            var user = BuildActiveUser(request.Email, request.Password);
            user.IsActive = false;

            _mockUserRepo.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*inactivo*");
        }

        // ── Login — soft-deleted user ────────────────────────────────────────
        [Fact]
        public async Task LoginAsync_DeletedUser_ShouldThrowUnauthorizedAccessException()
        {
            var request = new LoginRequestDto { Email = "deleted@test.com", Password = "Pass123!" };
            var user = BuildActiveUser(request.Email, request.Password);
            user.IsDeleted = true;

            _mockUserRepo.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // ── Logout — token blacklisted ───────────────────────────────────────
        [Fact]
        public async Task LogoutAsync_ShouldBlacklistTokenWithTtl()
        {
            _mockBlacklist.Setup(x => x.BlacklistTokenAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                          .Returns(Task.CompletedTask);

            await _authService.LogoutAsync("some.jwt.token");

            _mockBlacklist.Verify(
                x => x.BlacklistTokenAsync("some.jwt.token", It.Is<TimeSpan>(ts => ts.TotalHours > 0)),
                Times.Once);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COBROS SERVICE TESTS
    // ═══════════════════════════════════════════════════════════════════════════
    public class CobrosServiceTests
    {
        private readonly Mock<ICobroRepository> _mockCobroRepo;
        private readonly Mock<IUnitOfWork>       _mockUow;
        private readonly Mock<IEmailService>     _mockEmailService;
        private readonly IMapper                 _mapper;
        private readonly CobrosService           _cobrosService;

        public CobrosServiceTests()
        {
            _mockCobroRepo    = new Mock<ICobroRepository>();
            _mockUow          = new Mock<IUnitOfWork>();
            _mockEmailService = new Mock<IEmailService>();

            // Use real AutoMapper with actual profiles — no fake mapping
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<CotizacionMappingProfile>();
            });
            _mapper = config.CreateMapper();

            _cobrosService = new CobrosService(
                _mockCobroRepo.Object, _mockUow.Object, _mapper, _mockEmailService.Object);
        }

        private static Cobro BuildCobro(int id = 1, EstadoCobro estado = EstadoCobro.Pendiente) => new()
        {
            Id                    = id,
            NumeroRecibo          = $"REC-202510-{id:D4}",
            PolizaId              = 1,
            NumeroPoliza          = "POL-001",
            ClienteNombreCompleto = "Juan Pérez",
            MontoTotal            = 2500.00m,
            MontoCobrado          = 0,
            FechaVencimiento      = DateTime.UtcNow.AddMonths(1),
            FechaCobro            = DateTime.MinValue,
            Estado                = estado,
            MetodoPago            = MetodoPago.Efectivo,
            Moneda                = "CRC",
            CreatedBy             = "Test"
        };

        // ── GetAllCobrosAsync ────────────────────────────────────────────────
        [Fact]
        public async Task GetAllCobrosAsync_ShouldReturnMappedDtos()
        {
            var cobros = new List<Cobro> { BuildCobro(1), BuildCobro(2) };
            _mockCobroRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cobros);

            var result = (await _cobrosService.GetAllCobrosAsync()).ToList();

            result.Should().HaveCount(2);
            result[0].NumeroRecibo.Should().Be("REC-202510-0001");
            result[1].NumeroRecibo.Should().Be("REC-202510-0002");
        }

        // ── GetCobroByIdAsync — found ────────────────────────────────────────
        [Fact]
        public async Task GetCobroByIdAsync_ExistingId_ShouldReturnDto()
        {
            _mockCobroRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(BuildCobro(1));

            var result = await _cobrosService.GetCobroByIdAsync(1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Estado.Should().Be(EstadoCobro.Pendiente.ToString());
        }

        // ── GetCobroByIdAsync — not found ────────────────────────────────────
        [Fact]
        public async Task GetCobroByIdAsync_NonExistingId_ShouldReturnNull()
        {
            _mockCobroRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Cobro?)null);

            var result = await _cobrosService.GetCobroByIdAsync(999);

            result.Should().BeNull();
        }

        // ── RegistrarCobroAsync — success ────────────────────────────────────
        [Fact]
        public async Task RegistrarCobroAsync_ValidRequest_ShouldSetEstadoPagado()
        {
            var cobro = BuildCobro(1, EstadoCobro.Pendiente);
            var request = new RegistrarCobroRequestDto
            {
                CobroId       = 1,
                MontoCobrado  = 2500.00m,
                FechaCobro    = DateTime.UtcNow,
                MetodoPago    = MetodoPago.Transferencia,
                Observaciones = "Pago en tiempo"
            };

            _mockCobroRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cobro);
            _mockCobroRepo.Setup(x => x.UpdateAsync(It.IsAny<Cobro>())).Returns(Task.CompletedTask);

            var result = await _cobrosService.RegistrarCobroAsync(request);

            result.Should().NotBeNull();
            result.Estado.Should().Be(EstadoCobro.Pagado.ToString());
            result.MontoCobrado.Should().Be(2500.00m);
        }

        // ── RegistrarCobroAsync — cobro not found ────────────────────────────
        [Fact]
        public async Task RegistrarCobroAsync_CobroNotFound_ShouldThrowArgumentException()
        {
            _mockCobroRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Cobro?)null);

            var request = new RegistrarCobroRequestDto { CobroId = 999, MontoCobrado = 1000m, FechaCobro = DateTime.UtcNow };

            Func<Task> act = async () => await _cobrosService.RegistrarCobroAsync(request);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*999*");
        }

        // ── CancelarCobroAsync — success ─────────────────────────────────────
        [Fact]
        public async Task CancelarCobroAsync_PendingCobro_ShouldSetEstadoCancelado()
        {
            var cobro = BuildCobro(1, EstadoCobro.Pendiente);
            _mockCobroRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cobro);
            _mockCobroRepo.Setup(x => x.UpdateAsync(It.IsAny<Cobro>())).Returns(Task.CompletedTask);

            var result = await _cobrosService.CancelarCobroAsync(1, "Error de facturación");

            result.Estado.Should().Be(EstadoCobro.Cancelado.ToString());
        }

        // ── CancelarCobroAsync — already paid ────────────────────────────────
        [Fact]
        public async Task CancelarCobroAsync_PaidCobro_ShouldThrowInvalidOperationException()
        {
            var cobro = BuildCobro(1, EstadoCobro.Pagado);
            _mockCobroRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cobro);

            Func<Task> act = async () => await _cobrosService.CancelarCobroAsync(1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*pagado*");
        }

        // ── CancelarCobroAsync — not found ───────────────────────────────────
        [Fact]
        public async Task CancelarCobroAsync_NotFound_ShouldThrowArgumentException()
        {
            _mockCobroRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Cobro?)null);

            Func<Task> act = async () => await _cobrosService.CancelarCobroAsync(999);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*999*");
        }

        // ── GetCobrosStatsAsync ──────────────────────────────────────────────
        [Fact]
        public async Task GetCobrosStatsAsync_ShouldReturnAggregatedStats()
        {
            _mockCobroRepo.Setup(x => x.GetTotalCountAsync()).ReturnsAsync(10);
            _mockCobroRepo.Setup(x => x.GetTotalCobrosPendientesAsync()).ReturnsAsync(4);
            _mockCobroRepo.Setup(x => x.GetCobrosVencidosCountAsync()).ReturnsAsync(2);
            _mockCobroRepo.Setup(x => x.GetMontoTotalPendienteAsync()).ReturnsAsync(5000m);
            _mockCobroRepo.Setup(x => x.GetTotalCobradoAsync()).ReturnsAsync(15000m);
            _mockCobroRepo.Setup(x => x.GetCobrosProximosVencerCountAsync(7)).ReturnsAsync(3);

            var result = await _cobrosService.GetCobrosStatsAsync();

            result.TotalCobros.Should().Be(10);
            result.CobrosPendientes.Should().Be(4);
            result.CobrosPagados.Should().Be(6);
            result.CobrosVencidos.Should().Be(2);
            result.MontoTotalPendiente.Should().Be(5000m);
            result.MontoTotalCobrado.Should().Be(15000m);
            result.PorcentajeCobrado.Should().Be(60m);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECLAMO SERVICE TESTS
    // ═══════════════════════════════════════════════════════════════════════════
    public class ReclamoServiceTests
    {
        private readonly Mock<IReclamoRepository> _mockReclamoRepo;
        private readonly Mock<IUnitOfWork>         _mockUow;
        private readonly Mock<IPolizaRepository>   _mockPolizaRepo;
        private readonly IMapper                   _mapper;
        private readonly ReclamoService            _reclamoService;

        public ReclamoServiceTests()
        {
            _mockReclamoRepo = new Mock<IReclamoRepository>();
            _mockUow         = new Mock<IUnitOfWork>();
            _mockPolizaRepo  = new Mock<IPolizaRepository>();

            _mockUow.Setup(u => u.Polizas).Returns(_mockPolizaRepo.Object);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<CotizacionMappingProfile>();
            });
            _mapper = config.CreateMapper();

            _reclamoService = new ReclamoService(
                _mockReclamoRepo.Object, _mockUow.Object, _mapper);
        }

        private static Reclamo BuildReclamo(int id = 1, EstadoReclamo estado = EstadoReclamo.Pendiente) => new()
        {
            Id                    = id,
            NumeroReclamo         = $"REC-2025-{id:D5}",
            NumeroPoliza          = "POL-001",
            TipoReclamo           = TipoReclamo.Reclamo,
            Descripcion           = "Accidente de tránsito",
            FechaReclamo          = DateTime.UtcNow,
            Estado                = estado,
            Prioridad             = PrioridadReclamo.Media,
            MontoReclamado        = 1500.00m,
            NombreAsegurado       = "María García",
            ClienteNombreCompleto = "María García",
            Moneda                = "CRC",
            CreatedBy             = "Test"
        };

        // ── GetAllReclamosAsync ──────────────────────────────────────────────
        [Fact]
        public async Task GetAllReclamosAsync_ShouldReturnMappedDtos()
        {
            _mockReclamoRepo.Setup(x => x.GetAllAsync())
                            .ReturnsAsync(new List<Reclamo> { BuildReclamo(1), BuildReclamo(2) });

            var result = (await _reclamoService.GetAllReclamosAsync()).ToList();

            result.Should().HaveCount(2);
            result[0].Estado.Should().Be(EstadoReclamo.Pendiente.ToString());
        }

        // ── CreateReclamoAsync — poliza exists, nombre from poliza ───────────
        [Fact]
        public async Task CreateReclamoAsync_WhenPolizaExists_ShouldUsePolizaNombreAsegurado()
        {
            var poliza = new Poliza { Id = 1, NumeroPoliza = "POL-001", NombreAsegurado = "Carlos López", CreatedBy = "Test" };
            _mockPolizaRepo.Setup(x => x.GetByNumeroPolizaAsync("POL-001")).ReturnsAsync(poliza);

            var createdReclamo = BuildReclamo(1);
            createdReclamo.NombreAsegurado = "Carlos López";
            createdReclamo.ClienteNombreCompleto = "Carlos López";
            _mockReclamoRepo.Setup(x => x.AddAsync(It.IsAny<Reclamo>())).ReturnsAsync(createdReclamo);
            _mockReclamoRepo.Setup(x => x.GenerateNumeroReclamoAsync()).ReturnsAsync("REC-2025-00001");

            var request = new CreateReclamoDto
            {
                NumeroPoliza  = "POL-001",
                TipoReclamo   = TipoReclamo.Reclamo,
                Descripcion   = "Daños al vehículo",
                MontoReclamado = 1500m,
                Prioridad     = PrioridadReclamo.Media
            };

            var result = await _reclamoService.CreateReclamoAsync(request);

            result.Should().NotBeNull();
            _mockReclamoRepo.Verify(x => x.AddAsync(It.Is<Reclamo>(r =>
                r.NombreAsegurado == "Carlos López" && r.ClienteNombreCompleto == "Carlos López")), Times.Once);
        }

        // ── CreateReclamoAsync — no poliza, uses provided nombre ─────────────
        [Fact]
        public async Task CreateReclamoAsync_WhenPolizaNotFound_ShouldUseDtoNombreAsegurado()
        {
            _mockPolizaRepo.Setup(x => x.GetByNumeroPolizaAsync(It.IsAny<string>())).ReturnsAsync((Poliza?)null);

            var createdReclamo = BuildReclamo(1);
            createdReclamo.ClienteNombreCompleto = "Ana Torres";
            _mockReclamoRepo.Setup(x => x.AddAsync(It.IsAny<Reclamo>())).ReturnsAsync(createdReclamo);
            _mockReclamoRepo.Setup(x => x.GenerateNumeroReclamoAsync()).ReturnsAsync("REC-2025-00002");

            var request = new CreateReclamoDto
            {
                NumeroPoliza          = "POL-INEXISTENTE",
                TipoReclamo           = TipoReclamo.Reclamo,
                Descripcion           = "Daños por calamidad",
                MontoReclamado        = 2000m,
                NombreAsegurado       = "Ana Torres",
                ClienteNombreCompleto = "Ana Torres",
                Prioridad             = PrioridadReclamo.Alta
            };

            var result = await _reclamoService.CreateReclamoAsync(request);

            result.Should().NotBeNull();
        }

        // ── CambiarEstadoAsync — success ─────────────────────────────────────
        [Fact]
        public async Task CambiarEstadoAsync_ValidReclamo_ShouldUpdateEstado()
        {
            var reclamo = BuildReclamo(1, EstadoReclamo.Pendiente);
            _mockReclamoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reclamo);
            _mockReclamoRepo.Setup(x => x.UpdateAsync(It.IsAny<Reclamo>())).Returns(Task.CompletedTask);

            var result = await _reclamoService.CambiarEstadoAsync(1, EstadoReclamo.EnProceso);

            result.Estado.Should().Be(EstadoReclamo.EnProceso.ToString());
            _mockReclamoRepo.Verify(x => x.UpdateAsync(It.Is<Reclamo>(r => r.Estado == EstadoReclamo.EnProceso)), Times.Once);
        }

        // ── CambiarEstadoAsync — not found ───────────────────────────────────
        [Fact]
        public async Task CambiarEstadoAsync_ReclamoNotFound_ShouldThrowArgumentException()
        {
            _mockReclamoRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Reclamo?)null);

            Func<Task> act = async () => await _reclamoService.CambiarEstadoAsync(999, EstadoReclamo.EnProceso);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*999*");
        }

        // ── ResolverReclamoAsync — success ───────────────────────────────────
        [Fact]
        public async Task ResolverReclamoAsync_ShouldSetEstadoResueltoAndMontoAprobado()
        {
            var reclamo = BuildReclamo(1, EstadoReclamo.EnProceso);
            _mockReclamoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reclamo);
            _mockReclamoRepo.Setup(x => x.UpdateAsync(It.IsAny<Reclamo>())).Returns(Task.CompletedTask);

            var result = await _reclamoService.ResolverReclamoAsync(1, 1200m, "Aprobado parcialmente");

            result.Estado.Should().Be(EstadoReclamo.Resuelto.ToString());
            _mockReclamoRepo.Verify(x => x.UpdateAsync(It.Is<Reclamo>(r =>
                r.Estado == EstadoReclamo.Resuelto &&
                r.MontoAprobado == 1200m &&
                r.FechaResolucion.HasValue)), Times.Once);
        }

        // ── ResolverReclamoAsync — not found ─────────────────────────────────
        [Fact]
        public async Task ResolverReclamoAsync_ReclamoNotFound_ShouldThrowArgumentException()
        {
            _mockReclamoRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Reclamo?)null);

            Func<Task> act = async () => await _reclamoService.ResolverReclamoAsync(999, null, "");

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*999*");
        }

        // ── GetReclamosStatsAsync ────────────────────────────────────────────
        [Fact]
        public async Task GetReclamosStatsAsync_ShouldReturnCorrectTasaAprobacion()
        {
            _mockReclamoRepo.Setup(x => x.GetTotalCountAsync()).ReturnsAsync(20);
            _mockReclamoRepo.Setup(x => x.GetCountByEstadoAsync(EstadoReclamo.Pendiente)).ReturnsAsync(5);
            _mockReclamoRepo.Setup(x => x.GetCountByEstadoAsync(EstadoReclamo.EnProceso)).ReturnsAsync(8);
            _mockReclamoRepo.Setup(x => x.GetCountByEstadoAsync(EstadoReclamo.Resuelto)).ReturnsAsync(6);
            _mockReclamoRepo.Setup(x => x.GetCountByEstadoAsync(EstadoReclamo.Rechazado)).ReturnsAsync(1);
            _mockReclamoRepo.Setup(x => x.GetMontoTotalReclamadoAsync()).ReturnsAsync(100_000m);
            _mockReclamoRepo.Setup(x => x.GetMontoTotalAprobadoAsync()).ReturnsAsync(75_000m);

            var result = await _reclamoService.GetReclamosStatsAsync();

            result.TotalReclamos.Should().Be(20);
            result.TasaAprobacion.Should().Be(75m);
        }
    }
}
