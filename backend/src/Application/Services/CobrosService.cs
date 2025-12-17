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

        public CobrosService(ICobroRepository cobroRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _cobroRepository = cobroRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public async Task<CobroStatsDto> GetCobrosStatsAsync()
        {
            var totalCobros = (await _cobroRepository.GetAllAsync()).Count();
            var cobrosPendientes = await _cobroRepository.GetTotalCobrosPendientesAsync();
            var cobrosVencidos = (await _cobroRepository.GetCobrosVencidosAsync()).Count();
            var montoTotalPendiente = await _cobroRepository.GetMontoTotalPendienteAsync();
            var montoTotalCobrado = await _cobroRepository.GetTotalCobradoAsync();
            var cobrosProximosVencer = (await _cobroRepository.GetCobrosProximosVencerAsync(7)).Count();

            return new CobroStatsDto
            {
                TotalCobros = totalCobros,
                CobrosPendientes = cobrosPendientes,
                CobrosPagados = totalCobros - cobrosPendientes,
                CobrosVencidos = cobrosVencidos,
                MontoTotalPendiente = montoTotalPendiente,
                MontoTotalCobrado = montoTotalCobrado,
                PorcentajeCobrado = totalCobros > 0 ? (decimal)(totalCobros - cobrosPendientes) / totalCobros * 100 : 0,
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
                NumeroPoliza = poliza.NumeroPoliza,
                ClienteNombreCompleto = poliza.NombreAsegurado,
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
    }
}
