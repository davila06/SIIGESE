using Application.DTOs;
using Application.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services;

public interface IAnalyticsReportsService
{
    Task<ReportFileDto> GenerateCarteraAseguradoraExcelAsync(string? aseguradora);
    Task<ReportFileDto> GenerateCarteraAseguradoraPdfAsync(string? aseguradora);
    Task<ReportFileDto> GenerateMorosidadExcelAsync();
    Task<ReportFileDto> GenerateReclamosSlaPdfAsync();
    Task<ReportFileDto> GenerateEstadoPortafolioPdfAsync();
    Task<int> SendMorosidadReportByEmailAsync(IEnumerable<string>? recipients = null);
    Task<int> SendEstadoPortafolioReportToAdminsAsync();
    Task RunMonthlyAutomationAsync();
}

public sealed class AnalyticsReportsService : IAnalyticsReportsService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsReportsService> _logger;

    private static readonly Dictionary<string, decimal> FrecuenciaMultiplier = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MENSUAL", 1m },
        { "BIMESTRAL", 1m / 2m },
        { "TRIMESTRAL", 1m / 3m },
        { "CUATRIMESTRAL", 1m / 4m },
        { "SEMESTRAL", 1m / 6m },
        { "ANUAL", 1m / 12m }
    };

    public AnalyticsReportsService(
        ApplicationDbContext db,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AnalyticsReportsService> logger)
    {
        _db = db;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ReportFileDto> GenerateCarteraAseguradoraExcelAsync(string? aseguradora)
    {
        var rows = await BuildCarteraRowsAsync(aseguradora);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Cartera");

        ws.Cell(1, 1).Value = "Aseguradora";
        ws.Cell(1, 2).Value = "NumeroPoliza";
        ws.Cell(1, 3).Value = "Cliente";
        ws.Cell(1, 4).Value = "PrimaMensual";
        ws.Cell(1, 5).Value = "MontoEsperado";
        ws.Cell(1, 6).Value = "MontoCobrado";
        ws.Cell(1, 7).Value = "TasaCobroPct";
        ws.Cell(1, 8).Value = "ReclamosPendientes";
        ws.Cell(1, 9).Value = "EstadoCobros";

        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = r.Aseguradora;
            ws.Cell(row, 2).Value = r.NumeroPoliza;
            ws.Cell(row, 3).Value = r.Cliente;
            ws.Cell(row, 4).Value = r.PrimaMensual;
            ws.Cell(row, 5).Value = r.MontoEsperado;
            ws.Cell(row, 6).Value = r.MontoCobrado;
            ws.Cell(row, 7).Value = r.TasaCobroPct;
            ws.Cell(row, 8).Value = r.ReclamosPendientes;
            ws.Cell(row, 9).Value = r.EstadoCobros;
        }

        ws.Range(1, 1, 1, 9).Style.Font.Bold = true;
        ws.Column(4).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(5).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(7).Style.NumberFormat.Format = "0.0";
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = string.IsNullOrWhiteSpace(aseguradora) ? "todas" : SanitizeName(aseguradora);
        return new ReportFileDto
        {
            FileName = $"reporte_cartera_{suffix}_{date}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Content = ms.ToArray()
        };
    }

    public async Task<ReportFileDto> GenerateCarteraAseguradoraPdfAsync(string? aseguradora)
    {
        var rows = await BuildCarteraRowsAsync(aseguradora);
        var title = "Reporte de Cartera por Aseguradora";
        var lines = new List<string>
        {
            $"Fecha UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
            $"Filtro aseguradora: {(string.IsNullOrWhiteSpace(aseguradora) ? "Todas" : aseguradora)}",
            $"Total polizas: {rows.Count}",
            $"Prima mensual total: {rows.Sum(r => r.PrimaMensual):N0}",
            $"Monto esperado: {rows.Sum(r => r.MontoEsperado):N0}",
            $"Monto cobrado: {rows.Sum(r => r.MontoCobrado):N0}",
            string.Empty,
            "Top 10 polizas por monto esperado"
        };

        foreach (var r in rows.OrderByDescending(x => x.MontoEsperado).Take(10))
        {
            lines.Add($"- {r.NumeroPoliza} | {r.Cliente} | Cobro {r.TasaCobroPct:N1}% | Reclamos pend: {r.ReclamosPendientes}");
        }

        var pdf = SimplePdfBuilder.BuildSinglePage(title, lines);
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = string.IsNullOrWhiteSpace(aseguradora) ? "todas" : SanitizeName(aseguradora);

        return new ReportFileDto
        {
            FileName = $"reporte_cartera_{suffix}_{date}.pdf",
            ContentType = "application/pdf",
            Content = pdf
        };
    }

    public async Task<ReportFileDto> GenerateMorosidadExcelAsync()
    {
        var rows = await BuildMorosidadRowsAsync();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Morosidad");

        ws.Cell(1, 1).Value = "NumeroPoliza";
        ws.Cell(1, 2).Value = "Cliente";
        ws.Cell(1, 3).Value = "Correo";
        ws.Cell(1, 4).Value = "Telefono";
        ws.Cell(1, 5).Value = "DiasMora";
        ws.Cell(1, 6).Value = "MontoVencido";
        ws.Cell(1, 7).Value = "FechaVencimiento";
        ws.Cell(1, 8).Value = "HistorialCobros";
        ws.Cell(1, 9).Value = "HistorialCobrado";

        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = r.NumeroPoliza;
            ws.Cell(row, 2).Value = r.Cliente;
            ws.Cell(row, 3).Value = r.Correo;
            ws.Cell(row, 4).Value = r.Telefono;
            ws.Cell(row, 5).Value = r.DiasMora;
            ws.Cell(row, 6).Value = r.MontoVencido;
            ws.Cell(row, 7).Value = r.FechaVencimiento;
            ws.Cell(row, 8).Value = r.HistorialCobros;
            ws.Cell(row, 9).Value = r.HistorialCobrado;
        }

        ws.Range(1, 1, 1, 9).Style.Font.Bold = true;
        ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(7).Style.DateFormat.Format = "yyyy-MM-dd";
        ws.Column(9).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        return new ReportFileDto
        {
            FileName = $"reporte_morosidad_{DateTime.UtcNow:yyyyMMdd}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Content = ms.ToArray()
        };
    }

    public async Task<ReportFileDto> GenerateReclamosSlaPdfAsync()
    {
        var now = DateTime.UtcNow;
        var reclamos = await _db.Reclamos
            .Where(r => !r.IsDeleted)
            .Select(r => new
            {
                r.Estado,
                r.FechaReclamo,
                r.FechaResolucion,
                r.FechaLimiteRespuesta,
                r.MontoReclamado,
                r.MontoAprobado
            })
            .ToListAsync();

        var total = reclamos.Count;
        var resueltosConSla = reclamos.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
        var dentroSla = resueltosConSla.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
        var pctSla = resueltosConSla.Count > 0 ? Math.Round((double)dentroSla / resueltosConSla.Count * 100, 1) : 100;
        var tiempoProm = resueltosConSla.Count > 0
            ? Math.Round(resueltosConSla.Average(r => (r.FechaResolucion!.Value - r.FechaReclamo).TotalDays), 1)
            : 0;

        var lines = new List<string>
        {
            $"Fecha UTC: {now:yyyy-MM-dd HH:mm}",
            $"Total reclamos: {total}",
            $"Cumplimiento SLA: {pctSla:N1}%",
            $"Tiempo promedio resolucion (dias): {tiempoProm:N1}",
            $"Monto total reclamado: {reclamos.Sum(r => r.MontoReclamado):N0}",
            $"Monto total aprobado: {reclamos.Sum(r => r.MontoAprobado ?? 0):N0}",
            string.Empty,
            "Reclamos por estado"
        };

        foreach (var g in reclamos.GroupBy(r => r.Estado.ToString()).OrderByDescending(g => g.Count()))
        {
            lines.Add($"- {g.Key}: {g.Count()}");
        }

        var pdf = SimplePdfBuilder.BuildSinglePage("Reporte de Reclamos (SLA Compliance)", lines);
        return new ReportFileDto
        {
            FileName = $"reporte_reclamos_sla_{DateTime.UtcNow:yyyyMMdd}.pdf",
            ContentType = "application/pdf",
            Content = pdf
        };
    }

    public async Task<ReportFileDto> GenerateEstadoPortafolioPdfAsync()
    {
        var now = DateTime.UtcNow;
        var mesInicio = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var carteraActiva = await _db.Polizas.CountAsync(p => p.EsActivo && !p.IsDeleted);
        var polizasActivas = await _db.Polizas
            .Where(p => p.EsActivo && !p.IsDeleted)
            .Select(p => new { p.Prima, p.Frecuencia })
            .ToListAsync();

        var primaMensual = polizasActivas.Sum(p =>
            FrecuenciaMultiplier.TryGetValue(p.Frecuencia ?? "MENSUAL", out var m)
                ? p.Prima * m
                : p.Prima);

        var cobrosMes = await _db.Cobros
            .Where(c => !c.IsDeleted && c.FechaVencimiento >= mesInicio && c.FechaVencimiento < mesInicio.AddMonths(1))
            .Select(c => new { c.MontoTotal, c.MontoCobrado, c.Estado })
            .ToListAsync();

        var esperado = cobrosMes.Sum(c => c.MontoTotal);
        var cobrado = cobrosMes.Sum(c => c.MontoCobrado);
        var tasaCobro = esperado > 0 ? Math.Round(cobrado / esperado * 100m, 1) : 0;

        var montoRiesgo = await _db.Cobros
            .Where(c => !c.IsDeleted && (c.Estado == EstadoCobro.Pendiente || c.Estado == EstadoCobro.Vencido))
            .SumAsync(c => c.MontoTotal);

        var reclamosActivos = await _db.Reclamos
            .CountAsync(r => !r.IsDeleted && r.Estado != EstadoReclamo.Resuelto && r.Estado != EstadoReclamo.Cerrado);

        var cotizacionesMes = await _db.Cotizaciones
            .Where(c => !c.IsDeleted && c.FechaCreacion >= mesInicio)
            .Select(c => c.Estado)
            .ToListAsync();

        var tasaConversion = cotizacionesMes.Count > 0
            ? Math.Round((decimal)cotizacionesMes.Count(e => e == "CONVERTIDA") / cotizacionesMes.Count * 100m, 1)
            : 0;

        var emailLogsMes = await _db.EmailLogs
            .Where(e => !e.IsDeleted && e.SentAt >= mesInicio)
            .Select(e => e.IsSuccess)
            .ToListAsync();

        var tasaEmail = emailLogsMes.Count > 0
            ? Math.Round((decimal)emailLogsMes.Count(x => x) / emailLogsMes.Count * 100m, 1)
            : 100;

        var reclamosSla = await _db.Reclamos
            .Where(r => !r.IsDeleted && r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue)
            .Select(r => new { r.FechaResolucion, r.FechaLimiteRespuesta })
            .ToListAsync();

        var tasaSla = reclamosSla.Count > 0
            ? Math.Round((decimal)reclamosSla.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value) / reclamosSla.Count * 100m, 1)
            : 100;

        var lines = new List<string>
        {
            $"Fecha UTC: {now:yyyy-MM-dd HH:mm}",
            string.Empty,
            $"Cartera activa: {carteraActiva}",
            $"Prima mensual bruta: {primaMensual:N0}",
            $"Tasa de cobro: {tasaCobro:N1}%",
            $"Monto en riesgo: {montoRiesgo:N0}",
            $"Reclamos activos: {reclamosActivos}",
            $"Tasa de conversion: {tasaConversion:N1}%",
            $"Email exitoso: {tasaEmail:N1}%",
            $"SLA reclamos: {tasaSla:N1}%"
        };

        var pdf = SimplePdfBuilder.BuildSinglePage("Estado del Portafolio", lines);
        return new ReportFileDto
        {
            FileName = $"estado_portafolio_{DateTime.UtcNow:yyyyMMdd}.pdf",
            ContentType = "application/pdf",
            Content = pdf
        };
    }

    public async Task<int> SendMorosidadReportByEmailAsync(IEnumerable<string>? recipients = null)
    {
        var targetRecipients = (recipients ?? GetMorosidadRecipients())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (targetRecipients.Count == 0)
        {
            _logger.LogWarning("No hay destinatarios para reporte de morosidad");
            return 0;
        }

        var report = await GenerateMorosidadExcelAsync();
        var subject = $"SINSEG - Reporte de Morosidad ({DateTime.UtcNow:yyyy-MM-dd})";
        var body = "Adjunto se remite el reporte de morosidad ordenado por monto vencido.";

        var sent = 0;
        foreach (var recipient in targetRecipients)
        {
            try
            {
                await _emailService.SendGenericEmailWithAttachmentsAsync(
                    recipient,
                    subject,
                    body,
                    new[]
                    {
                        new EmailAttachmentDto
                        {
                            FileName = report.FileName,
                            ContentType = report.ContentType,
                            Content = report.Content
                        }
                    });

                await RegisterEmailLogAsync(recipient, subject, "ReporteMorosidadGerencia", true, null);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando reporte de morosidad a {Recipient}", recipient);
                await RegisterEmailLogAsync(recipient, subject, "ReporteMorosidadGerencia", false, ex.Message);
            }
        }

        return sent;
    }

    public async Task<int> SendEstadoPortafolioReportToAdminsAsync()
    {
        var adminRecipients = await GetAdminEmailsAsync();
        if (adminRecipients.Count == 0)
        {
            _logger.LogWarning("No hay usuarios Admin con email para reporte mensual de portafolio");
            return 0;
        }

        var report = await GenerateEstadoPortafolioPdfAsync();
        var subject = $"SINSEG - Estado del Portafolio ({DateTime.UtcNow:yyyy-MM-dd})";
        var body = "Adjunto se remite el estado mensual del portafolio con KPIs ejecutivos.";

        var sent = 0;
        foreach (var recipient in adminRecipients)
        {
            try
            {
                await _emailService.SendGenericEmailWithAttachmentsAsync(
                    recipient,
                    subject,
                    body,
                    new[]
                    {
                        new EmailAttachmentDto
                        {
                            FileName = report.FileName,
                            ContentType = report.ContentType,
                            Content = report.Content
                        }
                    });

                await RegisterEmailLogAsync(recipient, subject, "ReportePortafolioMensualAdmin", true, null);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando estado de portafolio a {Recipient}", recipient);
                await RegisterEmailLogAsync(recipient, subject, "ReportePortafolioMensualAdmin", false, ex.Message);
            }
        }

        return sent;
    }

    public async Task RunMonthlyAutomationAsync()
    {
        var enabled = _configuration.GetValue<bool?>("Reports:AutomationEnabled") ?? true;
        if (!enabled)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (now.Day != 1)
        {
            return;
        }

        var alreadySent = await _db.EmailLogs.AnyAsync(e =>
            !e.IsDeleted &&
            e.EmailType == "ReportePortafolioMensualAdmin" &&
            e.IsSuccess &&
            e.SentAt.Year == now.Year &&
            e.SentAt.Month == now.Month);

        if (alreadySent)
        {
            _logger.LogInformation("Automatizacion mensual ya ejecutada para {Year}-{Month}", now.Year, now.Month);
            return;
        }

        _logger.LogInformation("Ejecutando automatizacion mensual de reportes M8");
        await SendEstadoPortafolioReportToAdminsAsync();
        await SendMorosidadReportByEmailAsync();
    }

    private async Task<List<CarteraAseguradoraRowDto>> BuildCarteraRowsAsync(string? aseguradora)
    {
        var polizasQuery = _db.Polizas.Where(p => p.EsActivo && !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(aseguradora))
        {
            polizasQuery = polizasQuery.Where(p => p.Aseguradora == aseguradora);
        }

        var polizas = await polizasQuery
            .Select(p => new
            {
                NumeroPoliza = p.NumeroPoliza ?? string.Empty,
                Cliente = p.NombreAsegurado ?? "Sin Nombre",
                Aseguradora = p.Aseguradora ?? "OTROS",
                p.Prima,
                p.Frecuencia
            })
            .ToListAsync();

        var numerosPoliza = polizas.Select(p => p.NumeroPoliza).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

        var cobros = await _db.Cobros
            .Where(c => !c.IsDeleted && numerosPoliza.Contains(c.NumeroPoliza))
            .Select(c => new { c.NumeroPoliza, c.MontoTotal, c.MontoCobrado, c.Estado })
            .ToListAsync();

        var reclamosPendientes = await _db.Reclamos
            .Where(r => !r.IsDeleted
                        && numerosPoliza.Contains(r.NumeroPoliza)
                        && r.Estado != EstadoReclamo.Resuelto
                        && r.Estado != EstadoReclamo.Cerrado)
            .GroupBy(r => r.NumeroPoliza)
            .Select(g => new { NumeroPoliza = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.NumeroPoliza, x => x.Cantidad);

        var rows = polizas
            .Select(p =>
            {
                var cobrosPoliza = cobros.Where(c => c.NumeroPoliza == p.NumeroPoliza).ToList();
                var esperado = cobrosPoliza.Sum(c => c.MontoTotal);
                var cobrado = cobrosPoliza.Sum(c => c.MontoCobrado);
                var tasa = esperado > 0 ? Math.Round(cobrado / esperado * 100m, 1) : 0m;
                var estado = "Sin Cobros";
                if (cobrosPoliza.Any(c => c.Estado == EstadoCobro.Vencido))
                {
                    estado = "Vencido";
                }
                else if (cobrosPoliza.Any(c => c.Estado == EstadoCobro.Pendiente))
                {
                    estado = "Pendiente";
                }
                else if (cobrosPoliza.Count > 0)
                {
                    estado = "Al Dia";
                }

                var primaMensual = FrecuenciaMultiplier.TryGetValue(p.Frecuencia ?? "MENSUAL", out var m)
                    ? p.Prima * m
                    : p.Prima;

                return new CarteraAseguradoraRowDto
                {
                    Aseguradora = p.Aseguradora,
                    NumeroPoliza = p.NumeroPoliza,
                    Cliente = p.Cliente,
                    PrimaMensual = Math.Round(primaMensual, 2),
                    MontoEsperado = Math.Round(esperado, 2),
                    MontoCobrado = Math.Round(cobrado, 2),
                    TasaCobroPct = tasa,
                    ReclamosPendientes = reclamosPendientes.GetValueOrDefault(p.NumeroPoliza, 0),
                    EstadoCobros = estado
                };
            })
            .OrderBy(r => r.Aseguradora)
            .ThenBy(r => r.NumeroPoliza)
            .ToList();

        return rows;
    }

    private async Task<List<MorosidadReportRowDto>> BuildMorosidadRowsAsync()
    {
        var now = DateTime.UtcNow;
        var vencidos = await _db.Cobros
            .Where(c => !c.IsDeleted && c.Estado == EstadoCobro.Vencido)
            .Select(c => new
            {
                c.NumeroPoliza,
                c.ClienteNombreCompleto,
                c.CorreoElectronico,
                c.FechaVencimiento,
                c.MontoTotal
            })
            .OrderByDescending(c => c.MontoTotal)
            .ToListAsync();

        var numerosPoliza = vencidos.Select(v => v.NumeroPoliza).Distinct().ToList();

        var polizas = await _db.Polizas
            .Where(p => !p.IsDeleted && numerosPoliza.Contains(p.NumeroPoliza ?? string.Empty))
            .Select(p => new { NumeroPoliza = p.NumeroPoliza ?? string.Empty, Telefono = p.NumeroTelefono ?? string.Empty })
            .ToDictionaryAsync(x => x.NumeroPoliza, x => x.Telefono);

        var historialCobros = await _db.Cobros
            .Where(c => !c.IsDeleted && numerosPoliza.Contains(c.NumeroPoliza))
            .GroupBy(c => c.NumeroPoliza)
            .Select(g => new
            {
                NumeroPoliza = g.Key,
                Cantidad = g.Count(),
                Cobrado = g.Sum(x => x.MontoCobrado)
            })
            .ToDictionaryAsync(x => x.NumeroPoliza, x => (x.Cantidad, x.Cobrado));

        return vencidos.Select(v =>
        {
            var (cantidad, cobrado) = historialCobros.GetValueOrDefault(v.NumeroPoliza, (0, 0m));
            return new MorosidadReportRowDto
            {
                NumeroPoliza = v.NumeroPoliza,
                Cliente = v.ClienteNombreCompleto,
                Correo = v.CorreoElectronico ?? string.Empty,
                Telefono = polizas.GetValueOrDefault(v.NumeroPoliza, string.Empty),
                DiasMora = Math.Max(0, (int)(now.Date - v.FechaVencimiento.Date).TotalDays),
                MontoVencido = v.MontoTotal,
                HistorialCobros = cantidad,
                HistorialCobrado = cobrado,
                FechaVencimiento = v.FechaVencimiento
            };
        }).ToList();
    }

    private async Task<List<string>> GetAdminEmailsAsync()
    {
        return await _db.Users
            .Where(u => !u.IsDeleted && u.IsActive && !string.IsNullOrWhiteSpace(u.Email))
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin"))
            .Select(u => u.Email)
            .Distinct()
            .ToListAsync();
    }

    private IEnumerable<string> GetMorosidadRecipients()
    {
        var section = _configuration.GetSection("Reports:MorosidadRecipients").Get<string[]>() ?? Array.Empty<string>();
        if (section.Length > 0)
        {
            return section;
        }

        var raw = _configuration["Reports:MorosidadRecipientsCsv"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private async Task RegisterEmailLogAsync(string to, string subject, string emailType, bool success, string? error)
    {
        _db.EmailLogs.Add(new EmailLog
        {
            ToEmail = to,
            Subject = subject,
            Body = string.Empty,
            EmailType = emailType,
            IsSuccess = success,
            ErrorMessage = error,
            SentAt = DateTime.UtcNow,
            SenderName = "SINSEG"
        });
        await _db.SaveChangesAsync();
    }

    private static string SanitizeName(string input)
    {
        var clean = new string(input.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
        return string.IsNullOrWhiteSpace(clean) ? "aseguradora" : clean.ToLowerInvariant();
    }
}

public sealed class MonthlyReportsHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MonthlyReportsHostedService> _logger;

    public MonthlyReportsHostedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<MonthlyReportsHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollMinutes = _configuration.GetValue<int?>("Reports:PollIntervalMinutes") ?? 120;
        if (pollMinutes < 15)
        {
            pollMinutes = 15;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAnalyticsReportsService>();
                await service.RunMonthlyAutomationAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en scheduler mensual de reportes");
            }

            await Task.Delay(TimeSpan.FromMinutes(pollMinutes), stoppingToken);
        }
    }
}

internal static class SimplePdfBuilder
{
    public static byte[] BuildSinglePage(string title, IEnumerable<string> lines)
    {
        const int startY = 780;
        const int lineStep = 16;
        var contentLines = new List<string>
        {
            "BT",
            "/F1 16 Tf",
            $"72 {startY} Td",
            $"({Escape(title)}) Tj",
            "ET"
        };

        var y = startY - 28;
        foreach (var line in lines.Take(38))
        {
            contentLines.Add("BT");
            contentLines.Add("/F1 11 Tf");
            contentLines.Add($"72 {y} Td");
            contentLines.Add($"({Escape(line)}) Tj");
            contentLines.Add("ET");
            y -= lineStep;
        }

        var content = string.Join("\n", contentLines) + "\n";

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {System.Text.Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream"
        };

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, System.Text.Encoding.ASCII, 1024, true);

        writer.Write("%PDF-1.4\n");
        writer.Flush();

        var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(ms.Position);
            writer.Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            writer.Flush();
        }

        var xrefPos = ms.Position;
        writer.Write($"xref\n0 {objects.Count + 1}\n");
        writer.Write("0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
        {
            writer.Write($"{offsets[i]:D10} 00000 n \n");
        }

        writer.Write("trailer\n");
        writer.Write($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        writer.Write("startxref\n");
        writer.Write($"{xrefPos}\n");
        writer.Write("%%EOF");
        writer.Flush();

        return ms.ToArray();
    }

    private static string Escape(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }
}
