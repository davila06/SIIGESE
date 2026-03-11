using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services
{
    public class CobrosService : ICobrosService
    {
        private readonly ICobroRepository _cobroRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public CobrosService(ICobroRepository cobroRepository, IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _cobroRepository = cobroRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<IEnumerable<CobroDto>> GetAllCobrosAsync()
        {
            var cobros = await _cobroRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<CobroDto> GetCobroByIdAsync(int id)
        {
            var cobro = await _cobroRepository.GetByIdAsync(id);
            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<CobroDto> GetCobroByNumeroReciboAsync(string numeroRecibo)
        {
            var cobro = await _cobroRepository.GetByNumeroReciboAsync(numeroRecibo);
            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosByPolizaIdAsync(int polizaId)
        {
            var cobros = await _cobroRepository.GetCobrosByPolizaIdAsync(polizaId);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosByEstadoAsync(EstadoCobro estado)
        {
            var cobros = await _cobroRepository.GetCobrosByEstadoAsync(estado);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosVencidosAsync()
        {
            var cobros = await _cobroRepository.GetCobrosVencidosAsync();
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosProximosVencerAsync(int dias)
        {
            var cobros = await _cobroRepository.GetCobrosProximosVencerAsync(dias);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosProximosPorPeriodicidadAsync()
        {
            var cobros = await _cobroRepository.GetCobrosProximosPorPeriodicidadAsync();
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosByFrecuenciaAsync(string frecuencia)
        {
            var cobros = await _cobroRepository.GetCobrosByFrecuenciaAsync(frecuencia);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<CobroStatsDto> GetCobrosStatsAsync()
        {
            // Use dedicated SQL COUNT/SUM queries — no full table scan
            var totalCobros         = await _cobroRepository.GetTotalCountAsync();
            var cobrosPendientes    = await _cobroRepository.GetTotalCobrosPendientesAsync();
            var cobrosVencidos      = await _cobroRepository.GetCobrosVencidosCountAsync();
            var montoTotalPendiente = await _cobroRepository.GetMontoTotalPendienteAsync();
            var montoTotalCobrado   = await _cobroRepository.GetTotalCobradoAsync();
            var cobrosProximosVencer = await _cobroRepository.GetCobrosProximosVencerCountAsync(7);

            return new CobroStatsDto
            {
                TotalCobros         = totalCobros,
                CobrosPendientes    = cobrosPendientes,
                CobrosPagados       = totalCobros - cobrosPendientes,
                CobrosVencidos      = cobrosVencidos,
                MontoTotalPendiente = montoTotalPendiente,
                MontoTotalCobrado   = montoTotalCobrado,
                PorcentajeCobrado   = totalCobros > 0
                    ? (decimal)(totalCobros - cobrosPendientes) / totalCobros * 100
                    : 0,
                CobrosProximosVencer = cobrosProximosVencer
            };
        }

        public async Task<CobroDto> CreateCobroAsync(CobroRequestDto request)
        {
            // Obtener la póliza para llenar los datos del cobro
            var poliza = await _unitOfWork.Polizas.GetByIdAsync(request.PolizaId);
            if (poliza == null)
                throw new ArgumentException($"Póliza con ID {request.PolizaId} no encontrada");

            // Generar número de recibo
            var numeroRecibo = await GenerateNumeroReciboAsync();

            // Crear el cobro con datos de la póliza
            var cobro = new Cobro
            {
                NumeroRecibo = numeroRecibo,
                PolizaId = poliza.Id,
                NumeroPoliza = poliza.NumeroPoliza ?? string.Empty,
                ClienteNombreCompleto = poliza.NombreAsegurado ?? string.Empty,
                CorreoElectronico = request.CorreoElectronico ?? poliza.Correo,
                MontoTotal = request.MontoTotal,
                MontoCobrado = 0,
                FechaVencimiento = request.FechaVencimiento,
                FechaCobro = DateTime.MinValue,
                Estado = EstadoCobro.Pendiente,
                MetodoPago = request.MetodoPago,
                Moneda = request.Moneda,
                Observaciones = request.Observaciones ?? string.Empty,
                UsuarioCobroId = 0,
                UsuarioCobroNombre = string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Sistema",
                IsDeleted = false
            };

            var createdCobro = await _cobroRepository.AddAsync(cobro);
            return _mapper.Map<CobroDto>(createdCobro);
        }

        public async Task<CobroDto> UpdateCobroAsync(int id, ActualizarCobroDto request)
        {
            var cobro = await _cobroRepository.GetByIdAsync(id);
            if (cobro == null)
                throw new ArgumentException($"Cobro con ID {id} no encontrado");

            _mapper.Map(request, cobro);
            cobro.UpdatedAt = DateTime.UtcNow;
            cobro.UpdatedBy = "Sistema";

            await _cobroRepository.UpdateAsync(cobro);
            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<CobroDto> RegistrarCobroAsync(RegistrarCobroRequestDto request)
        {
            var cobro = await _cobroRepository.GetByIdAsync(request.CobroId);
            if (cobro == null)
                throw new ArgumentException($"Cobro con ID {request.CobroId} no encontrado");

            cobro.MontoCobrado = request.MontoCobrado;
            cobro.FechaCobro = request.FechaCobro;
            cobro.Estado = EstadoCobro.Pagado;
            cobro.MetodoPago = request.MetodoPago;
            cobro.Observaciones = request.Observaciones;
            cobro.UpdatedAt = DateTime.UtcNow;
            cobro.UpdatedBy = "Sistema";

            await _cobroRepository.UpdateAsync(cobro);
            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<string> GenerateNumeroReciboAsync()
        {
            var fecha = DateTime.Now.ToString("yyyyMM");
            var ultimoNumero = 1;
            
            // Lógica para generar número consecutivo
            var ultimoCobro = (await _cobroRepository.GetAllAsync())
                .Where(c => c.NumeroRecibo.StartsWith($"REC-{fecha}"))
                .OrderByDescending(c => c.NumeroRecibo)
                .FirstOrDefault();

            if (ultimoCobro != null && ultimoCobro.NumeroRecibo.Length > 11)
            {
                var numeroString = ultimoCobro.NumeroRecibo.Substring(11);
                if (int.TryParse(numeroString, out var numero))
                {
                    ultimoNumero = numero + 1;
                }
            }

            return $"REC-{fecha}-{ultimoNumero:D4}";
        }

        public async Task DeleteCobroAsync(int id)
        {
            await _cobroRepository.DeleteAsync(id);
        }

        public async Task<(bool Success, string Message)> EnviarEmailCobroAsync(int id)
        {
            var cobro = await _cobroRepository.GetByIdAsync(id);
            if (cobro == null)
                throw new KeyNotFoundException($"Cobro con ID {id} no encontrado");

            if (string.IsNullOrWhiteSpace(cobro.CorreoElectronico))
                return (false, "El cobro no tiene correo electrónico asociado. Edite el cobro para agregar un correo.");

            var isConfigured = await _emailService.IsConfiguredAsync();
            if (!isConfigured)
                return (false, "El servicio de email no está configurado. Configure el SMTP en Configuración.");

            var diasMora = cobro.Estado == EstadoCobro.Vencido
                ? Math.Max(0, (int)(DateTime.UtcNow.Date - cobro.FechaVencimiento.Date).TotalDays)
                : 0;

            var dto = new CobroVencidoDto
            {
                CobroId    = cobro.Id,
                NumeroPoliza  = cobro.NumeroPoliza,
                ClienteEmail  = cobro.CorreoElectronico,
                ClienteNombre = cobro.ClienteNombreCompleto,
                MontoVencido  = cobro.MontoTotal,
                FechaVencimiento = cobro.FechaVencimiento,
                DiasMora = diasMora,
                Concepto = $"Recibo #{cobro.NumeroRecibo}"
            };

            await _emailService.SendCobroVencidoNotificationAsync(dto);
            return (true, $"Correo enviado exitosamente a {cobro.CorreoElectronico}");
        }

        public async Task<CobroDto> CancelarCobroAsync(int id, string? motivo = null)
        {
            var cobro = await _cobroRepository.GetByIdAsync(id);
            if (cobro == null)
                throw new ArgumentException($"Cobro con ID {id} no encontrado");

            if (cobro.Estado == EstadoCobro.Pagado)
                throw new InvalidOperationException("No se puede cancelar un cobro ya pagado");

            cobro.Estado = EstadoCobro.Cancelado;
            if (!string.IsNullOrWhiteSpace(motivo))
                cobro.Observaciones = motivo;
            cobro.UpdatedAt = DateTime.UtcNow;
            cobro.UpdatedBy = "Sistema";

            await _cobroRepository.UpdateAsync(cobro);
            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<GenerarCobrosResultDto> GenerarCobrosAutomaticosAsync(int mesesAdelante = 3)
        {
            var resultado = new GenerarCobrosResultDto();
            var polizas = await _unitOfWork.Polizas.GetActivasAsync();

            foreach (var poliza in polizas)
            {
                try
                {
                    var cobrosGenerados = await GenerarCobrosPorPolizaInternoAsync(poliza, mesesAdelante);
                    resultado.PolizasProcesadas++;
                    resultado.CobrosGenerados += cobrosGenerados.Count;
                    resultado.CobrosCreados.AddRange(cobrosGenerados);
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Error en póliza {poliza.NumeroPoliza}: {ex.Message}");
                    resultado.PolizasSaltadas++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return resultado;
        }

        public async Task<GenerarCobrosResultDto> GenerarCobrosPorPolizaAsync(int polizaId, int mesesAdelante = 3)
        {
            var resultado = new GenerarCobrosResultDto();
            var poliza = await _unitOfWork.Polizas.GetByIdAsync(polizaId);

            if (poliza == null)
            {
                resultado.Errores.Add($"Póliza con ID {polizaId} no encontrada");
                return resultado;
            }

            try
            {
                var cobrosGenerados = await GenerarCobrosPorPolizaInternoAsync(poliza, mesesAdelante);
                resultado.PolizasProcesadas = 1;
                resultado.CobrosGenerados = cobrosGenerados.Count;
                resultado.CobrosCreados.AddRange(cobrosGenerados);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Error: {ex.Message}");
                resultado.PolizasSaltadas = 1;
            }

            return resultado;
        }

        private async Task<List<CobroGeneradoDto>> GenerarCobrosPorPolizaInternoAsync(Domain.Entities.Poliza poliza, int mesesAdelante)
        {
            var cobrosGenerados = new List<CobroGeneradoDto>();

            // Validar que la póliza tenga los datos necesarios
            if (string.IsNullOrEmpty(poliza.Frecuencia))
            {
                throw new InvalidOperationException("La póliza no tiene frecuencia de pago definida");
            }

            if (poliza.Prima <= 0)
            {
                throw new InvalidOperationException("La póliza no tiene prima definida");
            }

            // Obtener cobros existentes para esta póliza
            var cobrosExistentes = await _cobroRepository.GetCobrosByPolizaIdAsync(poliza.Id);
            var fechasExistentes = cobrosExistentes
                .Select(c => c.FechaVencimiento.Date)
                .ToHashSet();

            // Calcular fechas de vencimiento según la frecuencia
            var fechasVencimiento = CalcularFechasVencimiento(poliza.FechaVigencia, poliza.Frecuencia, mesesAdelante);

            foreach (var fechaVencimiento in fechasVencimiento)
            {
                // Saltar si ya existe un cobro para esta fecha
                if (fechasExistentes.Contains(fechaVencimiento.Date))
                {
                    continue;
                }

                // Generar número de recibo
                var numeroRecibo = await GenerateNumeroReciboAsync();

                // Crear el cobro
                var cobro = new Cobro
                {
                    NumeroRecibo = numeroRecibo,
                    PolizaId = poliza.Id,
                    NumeroPoliza = poliza.NumeroPoliza ?? string.Empty,
                    ClienteNombreCompleto = poliza.NombreAsegurado ?? string.Empty,
                    CorreoElectronico = poliza.Correo,
                    MontoTotal = poliza.Prima,
                    MontoCobrado = 0,
                    FechaVencimiento = fechaVencimiento,
                    FechaCobro = DateTime.MinValue,
                    Estado = EstadoCobro.Pendiente,
                    MetodoPago = MetodoPago.NoDefinido,
                    Moneda = poliza.Moneda ?? "CRC",
                    Observaciones = $"Cobro generado automáticamente - Frecuencia: {poliza.Frecuencia}",
                    UsuarioCobroId = 0,
                    UsuarioCobroNombre = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Sistema - Auto Generación",
                    IsDeleted = false
                };

                await _cobroRepository.AddAsync(cobro);

                cobrosGenerados.Add(new CobroGeneradoDto
                {
                    NumeroRecibo = numeroRecibo,
                    NumeroPoliza = poliza.NumeroPoliza ?? string.Empty,
                    FechaVencimiento = fechaVencimiento,
                    MontoTotal = poliza.Prima,
                    Moneda = poliza.Moneda ?? "CRC"
                });
            }

            return cobrosGenerados;
        }

        private List<DateTime> CalcularFechasVencimiento(DateTime fechaInicio, string frecuencia, int mesesAdelante)
        {
            var fechas = new List<DateTime>();
            var fechaActual = DateTime.UtcNow.Date;
            var fechaBase = fechaInicio.Date;

            // Ajustar fecha base si es anterior a hoy
            while (fechaBase < fechaActual)
            {
                fechaBase = AgregarPeriodo(fechaBase, frecuencia);
            }

            // Generar fechas hacia adelante
            var fechaLimite = fechaActual.AddMonths(mesesAdelante);
            var fechaGeneracion = fechaBase;

            while (fechaGeneracion <= fechaLimite)
            {
                fechas.Add(fechaGeneracion);
                fechaGeneracion = AgregarPeriodo(fechaGeneracion, frecuencia);
            }

            return fechas;
        }

        private DateTime AgregarPeriodo(DateTime fecha, string frecuencia)
        {
            var frecuenciaNormalizada = frecuencia.ToUpperInvariant().Trim();

            return frecuenciaNormalizada switch
            {
                "MENSUAL" or "MONTHLY" or "MES" or "MONTH" => fecha.AddMonths(1),
                "BIMESTRAL" or "BIMONTHLY" or "2 MESES" => fecha.AddMonths(2),
                "TRIMESTRAL" or "QUARTERLY" or "3 MESES" or "QUARTER" => fecha.AddMonths(3),
                "CUATRIMESTRAL" or "4 MESES" => fecha.AddMonths(4),
                "SEMESTRAL" or "SEMIANNUAL" or "6 MESES" or "SEMESTER" => fecha.AddMonths(6),
                "ANUAL" or "ANNUAL" or "YEARLY" or "AÑO" or "YEAR" or "ANO" => fecha.AddYears(1),
                "QUINCENAL" or "BIWEEKLY" or "15 DIAS" => fecha.AddDays(15),
                "SEMANAL" or "WEEKLY" or "WEEK" or "SEMANA" => fecha.AddDays(7),
                _ => fecha.AddMonths(1) // Por defecto mensual
            };
        }
    }
}
