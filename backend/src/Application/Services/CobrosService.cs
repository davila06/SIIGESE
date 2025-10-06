using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CobrosService : ICobrosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CobrosService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CobroDto>> GetAllCobrosAsync()
        {
            var cobros = await _unitOfWork.Cobros.GetAllAsync();
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<CobroDto?> GetCobroByIdAsync(int id)
        {
            var cobro = await _unitOfWork.Cobros.GetByIdAsync(id);
            return cobro != null ? _mapper.Map<CobroDto>(cobro) : null;
        }

        public async Task<CobroDto?> GetCobroByNumeroReciboAsync(string numeroRecibo)
        {
            var cobro = await _unitOfWork.Cobros.GetByNumeroReciboAsync(numeroRecibo);
            return cobro != null ? _mapper.Map<CobroDto>(cobro) : null;
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosByPolizaIdAsync(int polizaId)
        {
            var cobros = await _unitOfWork.Cobros.GetCobrosByPolizaIdAsync(polizaId);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosByEstadoAsync(string estado)
        {
            if (!Enum.TryParse<EstadoCobro>(estado, true, out var estadoCobro))
            {
                throw new ArgumentException($"Estado de cobro inválido: {estado}");
            }

            var cobros = await _unitOfWork.Cobros.GetCobrosByEstadoAsync(estadoCobro);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosVencidosAsync()
        {
            var cobros = await _unitOfWork.Cobros.GetCobrosVencidosAsync();
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<IEnumerable<CobroDto>> GetCobrosProximosVencerAsync(int dias = 7)
        {
            var cobros = await _unitOfWork.Cobros.GetCobrosProximosVencerAsync(dias);
            return _mapper.Map<IEnumerable<CobroDto>>(cobros);
        }

        public async Task<CobroStatsDto> GetCobrosStatsAsync()
        {
            var totalCobros = (await _unitOfWork.Cobros.GetAllAsync()).Count();
            var cobrosPendientes = await _unitOfWork.Cobros.GetTotalCobrosPendientesAsync();
            var cobrosCobrados = (await _unitOfWork.Cobros.GetCobrosByEstadoAsync(EstadoCobro.Cobrado)).Count();
            var cobrosVencidos = (await _unitOfWork.Cobros.GetCobrosVencidosAsync()).Count();
            
            var montoTotalPendiente = await _unitOfWork.Cobros.GetMontoTotalPendienteAsync();
            var montoTotalCobrado = await _unitOfWork.Cobros.GetTotalCobradoAsync();
            
            var cobrosVencidosList = await _unitOfWork.Cobros.GetCobrosVencidosAsync();
            var montoTotalVencido = cobrosVencidosList.Sum(c => c.MontoTotal);
            
            var cobrosProximosVencer = await _unitOfWork.Cobros.GetCobrosProximosVencerAsync();

            return new CobroStatsDto
            {
                TotalCobros = totalCobros,
                CobrosPendientes = cobrosPendientes,
                CobrosCobrados = cobrosCobrados,
                CobrosVencidos = cobrosVencidos,
                MontoTotalPendiente = montoTotalPendiente,
                MontoTotalCobrado = montoTotalCobrado,
                MontoTotalVencido = montoTotalVencido,
                CobrosProximosVencer = _mapper.Map<IEnumerable<CobroDto>>(cobrosProximosVencer)
            };
        }

        public async Task<CobroDto> CreateCobroAsync(CobroRequestDto request)
        {
            // Validar que la póliza existe
            var poliza = await _unitOfWork.Polizas.GetByIdAsync(request.PolizaId);
            if (poliza == null)
            {
                throw new KeyNotFoundException($"Póliza con ID {request.PolizaId} no encontrada");
            }

            // Generar número de recibo único
            var numeroRecibo = await GenerateNumeroReciboAsync();

            var cobro = new Cobro
            {
                NumeroRecibo = numeroRecibo,
                PolizaId = request.PolizaId,
                NumeroPoliza = poliza.NumeroPoliza,
                ClienteNombre = poliza.NombreAsegurado.Split(' ').FirstOrDefault() ?? "",
                ClienteApellido = string.Join(" ", poliza.NombreAsegurado.Split(' ').Skip(1)),
                FechaVencimiento = request.FechaVencimiento,
                MontoTotal = request.MontoTotal,
                Estado = EstadoCobro.Pendiente,
                Observaciones = request.Observaciones,
                CreatedBy = "System", // TODO: Obtener del usuario actual
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Cobros.AddAsync(cobro);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<CobroDto?> UpdateCobroAsync(int id, ActualizarCobroDto request)
        {
            var cobro = await _unitOfWork.Cobros.GetByIdAsync(id);
            if (cobro == null)
            {
                return null;
            }

            // Solo permitir actualización si el cobro está pendiente
            if (cobro.Estado != EstadoCobro.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden actualizar cobros en estado pendiente");
            }

            if (request.FechaVencimiento.HasValue)
            {
                cobro.FechaVencimiento = request.FechaVencimiento.Value;
            }

            if (request.MontoTotal.HasValue)
            {
                cobro.MontoTotal = request.MontoTotal.Value;
            }

            if (request.Observaciones != null)
            {
                cobro.Observaciones = request.Observaciones;
            }

            cobro.UpdatedAt = DateTime.UtcNow;
            cobro.UpdatedBy = "System"; // TODO: Obtener del usuario actual

            await _unitOfWork.Cobros.UpdateAsync(cobro);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<CobroDto?> RegistrarCobroAsync(RegistrarCobroRequestDto request)
        {
            var cobro = await _unitOfWork.Cobros.GetByIdAsync(request.CobroId);
            if (cobro == null)
            {
                return null;
            }

            if (cobro.Estado != EstadoCobro.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden registrar cobros en estado pendiente");
            }

            if (!Enum.IsDefined(typeof(MetodoPago), request.MetodoPago))
            {
                throw new ArgumentException($"Método de pago inválido: {request.MetodoPago}");
            }

            var metodoPago = (MetodoPago)request.MetodoPago;

            cobro.FechaCobro = request.FechaCobro;
            cobro.MontoCobrado = request.MontoCobrado;
            cobro.Estado = EstadoCobro.Cobrado;
            cobro.MetodoPago = metodoPago;
            cobro.Observaciones = request.Observaciones;
            cobro.UpdatedAt = DateTime.UtcNow;
            cobro.UpdatedBy = "System"; // TODO: Obtener del usuario actual
            cobro.UsuarioCobroId = 1; // TODO: Obtener del usuario actual
            cobro.UsuarioCobroNombre = "Admin"; // TODO: Obtener del usuario actual

            await _unitOfWork.Cobros.UpdateAsync(cobro);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CobroDto>(cobro);
        }

        public async Task<bool> DeleteCobroAsync(int id)
        {
            var cobro = await _unitOfWork.Cobros.GetByIdAsync(id);
            if (cobro == null)
            {
                return false;
            }

            // Solo permitir eliminar cobros pendientes
            if (cobro.Estado != EstadoCobro.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden eliminar cobros en estado pendiente");
            }

            await _unitOfWork.Cobros.DeleteAsync(cobro);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<string> GenerateNumeroReciboAsync()
        {
            string numeroRecibo;
            bool existe;

            do
            {
                // Formato: REC-YYYYMMDD-XXXX
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var random = new Random().Next(1000, 9999);
                numeroRecibo = $"REC-{fecha}-{random}";
                
                existe = await _unitOfWork.Cobros.ExisteNumeroReciboAsync(numeroRecibo);
            } 
            while (existe);

            return numeroRecibo;
        }
    }
}