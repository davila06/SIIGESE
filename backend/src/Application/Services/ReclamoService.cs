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
    public class ReclamoService : IReclamoService
    {
        private readonly IReclamoRepository _reclamoRepository;
        private readonly IMapper _mapper;

        public ReclamoService(IReclamoRepository reclamoRepository, IMapper mapper)
        {
            _reclamoRepository = reclamoRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReclamoDto>> GetAllReclamosAsync()
        {
            var reclamos = await _reclamoRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<ReclamoDto> GetReclamoByIdAsync(int id)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(id);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByPolizaAsync(string numeroPoliza)
        {
            var reclamos = await _reclamoRepository.GetReclamosByPolizaIdAsync(numeroPoliza);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByPolizaIdAsync(string numeroPoliza)
        {
            var reclamos = await _reclamoRepository.GetReclamosByPolizaIdAsync(numeroPoliza);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByEstadoAsync(EstadoReclamo estado)
        {
            var reclamos = await _reclamoRepository.GetReclamosByEstadoAsync(estado);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosVencidosAsync()
        {
            var reclamos = await _reclamoRepository.GetReclamosVencidosAsync();
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByFiltroAsync(ReclamoFilterDto filtro)
        {
            var reclamos = await _reclamoRepository.GetAllAsync();
            
            // Aplicar filtros
            var query = reclamos.AsQueryable();

            if (!string.IsNullOrEmpty(filtro.NumeroPoliza))
                query = query.Where(r => r.NumeroPoliza.Contains(filtro.NumeroPoliza));

            if (!string.IsNullOrEmpty(filtro.NumeroReclamo))
                query = query.Where(r => r.NumeroReclamo.Contains(filtro.NumeroReclamo));

            if (filtro.Estado.HasValue)
                query = query.Where(r => r.Estado == filtro.Estado.Value);

            if (filtro.TipoReclamo.HasValue)
                query = query.Where(r => r.TipoReclamo == filtro.TipoReclamo.Value);

            if (filtro.Prioridad.HasValue)
                query = query.Where(r => r.Prioridad == filtro.Prioridad.Value);

            if (filtro.FechaDesde.HasValue)
                query = query.Where(r => r.FechaReclamo >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(r => r.FechaReclamo <= filtro.FechaHasta.Value);

            if (filtro.UsuarioAsignadoId.HasValue)
                query = query.Where(r => r.UsuarioAsignadoId == filtro.UsuarioAsignadoId.Value);

            if (!string.IsNullOrEmpty(filtro.ClienteNombre))
                query = query.Where(r => r.ClienteNombre.Contains(filtro.ClienteNombre));

            // Paginación
            var resultado = query
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            return _mapper.Map<IEnumerable<ReclamoDto>>(resultado);
        }

        public async Task<ReclamoStatsDto> GetReclamosStatsAsync()
        {
            var reclamos = await _reclamoRepository.GetAllAsync();
            var totalReclamos = reclamos.Count();
            var pendientes = reclamos.Count(r => r.Estado == EstadoReclamo.Pendiente);
            var enProceso = reclamos.Count(r => r.Estado == EstadoReclamo.EnProceso);
            var resueltos = reclamos.Count(r => r.Estado == EstadoReclamo.Resuelto);
            var rechazados = reclamos.Count(r => r.Estado == EstadoReclamo.Rechazado);
            var montoTotalReclamado = reclamos.Sum(r => r.MontoReclamado);
            var montoTotalAprobado = reclamos.Where(r => r.MontoAprobado.HasValue).Sum(r => r.MontoAprobado ?? 0);

            return new ReclamoStatsDto
            {
                TotalReclamos = totalReclamos,
                ReclamosPendientes = pendientes,
                ReclamosEnProceso = enProceso,
                ReclamosResueltos = resueltos,
                ReclamosRechazados = rechazados,
                MontoTotalReclamado = montoTotalReclamado,
                MontoTotalAprobado = montoTotalAprobado,
                TasaAprobacion = montoTotalReclamado > 0 ? (montoTotalAprobado / montoTotalReclamado) * 100 : 0
            };
        }

        public async Task<string> GenerateNumeroReclamoAsync()
        {
            return await _reclamoRepository.GenerateNumeroReclamoAsync();
        }

        public async Task<ReclamoDto> CreateReclamoAsync(CreateReclamoDto request)
        {
            var reclamo = _mapper.Map<Reclamo>(request);
            
            if (string.IsNullOrEmpty(reclamo.NumeroReclamo))
            {
                reclamo.NumeroReclamo = await GenerateNumeroReclamoAsync();
            }

            reclamo.FechaReclamo = DateTime.UtcNow;
            reclamo.CreatedAt = DateTime.UtcNow;
            reclamo.CreatedBy = "Sistema";

            var createdReclamo = await _reclamoRepository.AddAsync(reclamo);
            return _mapper.Map<ReclamoDto>(createdReclamo);
        }

        public async Task<ReclamoDto> UpdateReclamoAsync(int id, UpdateReclamoDto request)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(id);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {id} no encontrado");

            _mapper.Map(request, reclamo);
            reclamo.UpdatedAt = DateTime.UtcNow;
            reclamo.UpdatedBy = "Sistema";

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> AsignarUsuarioAsync(int reclamoId, int usuarioId)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            reclamo.UsuarioAsignadoId = usuarioId;
            reclamo.UpdatedAt = DateTime.UtcNow;
            reclamo.UpdatedBy = "Sistema";

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> CambiarEstadoAsync(int reclamoId, EstadoReclamo nuevoEstado)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            reclamo.Estado = nuevoEstado;
            reclamo.UpdatedAt = DateTime.UtcNow;
            reclamo.UpdatedBy = "Sistema";

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> ResolverReclamoAsync(int reclamoId, decimal? montoAprobado, string observaciones)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            reclamo.Estado = EstadoReclamo.Resuelto;
            reclamo.MontoAprobado = montoAprobado;
            reclamo.FechaResolucion = DateTime.UtcNow;
            reclamo.Observaciones = observaciones ?? reclamo.Observaciones;
            reclamo.UpdatedAt = DateTime.UtcNow;
            reclamo.UpdatedBy = "Sistema";

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task DeleteReclamoAsync(int id)
        {
            await _reclamoRepository.DeleteAsync(id);
        }
    }
}