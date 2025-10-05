using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

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

        public async Task<ReclamoDto?> GetReclamoByIdAsync(int id)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(id);
            return reclamo != null ? _mapper.Map<ReclamoDto>(reclamo) : null;
        }

        public async Task<ReclamoDto> CreateReclamoAsync(CreateReclamoDto createReclamoDto)
        {
            var reclamo = _mapper.Map<Reclamo>(createReclamoDto);
            
            // Generar número de reclamo automáticamente
            reclamo.NumeroReclamo = await _reclamoRepository.GenerateNumeroReclamoAsync();
            
            // Establecer fecha límite de respuesta si no se especificó (30 días por defecto)
            if (!reclamo.FechaLimiteRespuesta.HasValue)
            {
                reclamo.FechaLimiteRespuesta = DateTime.UtcNow.AddDays(30);
            }

            await _reclamoRepository.AddAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto?> UpdateReclamoAsync(int id, UpdateReclamoDto updateReclamoDto)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(id);
            if (reclamo == null) return null;

            // Mapear solo las propiedades permitidas para actualización
            reclamo.Estado = (EstadoReclamo)updateReclamoDto.Estado;
            
            if (!string.IsNullOrEmpty(updateReclamoDto.Descripcion))
                reclamo.Descripcion = updateReclamoDto.Descripcion;
            
            if (updateReclamoDto.MontoAprobado.HasValue)
                reclamo.MontoAprobado = updateReclamoDto.MontoAprobado;
            
            if (updateReclamoDto.Prioridad.HasValue)
                reclamo.Prioridad = (PrioridadReclamo)updateReclamoDto.Prioridad.Value;
            
            if (!string.IsNullOrEmpty(updateReclamoDto.Observaciones))
                reclamo.Observaciones = updateReclamoDto.Observaciones;
            
            if (!string.IsNullOrEmpty(updateReclamoDto.DocumentosAdjuntos))
                reclamo.DocumentosAdjuntos = updateReclamoDto.DocumentosAdjuntos;
            
            if (updateReclamoDto.UsuarioAsignadoId.HasValue)
                reclamo.UsuarioAsignadoId = updateReclamoDto.UsuarioAsignadoId;
            
            if (updateReclamoDto.FechaLimiteRespuesta.HasValue)
                reclamo.FechaLimiteRespuesta = updateReclamoDto.FechaLimiteRespuesta;

            // Si se resuelve o cierra, establecer fecha de resolución
            if (reclamo.Estado == EstadoReclamo.Resuelto || reclamo.Estado == EstadoReclamo.Cerrado)
            {
                reclamo.FechaResolucion = DateTime.UtcNow;
            }

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<bool> DeleteReclamoAsync(int id)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(id);
            if (reclamo == null) return false;

            await _reclamoRepository.DeleteAsync(reclamo);
            return true;
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByPolizaIdAsync(int polizaId)
        {
            var reclamos = await _reclamoRepository.GetReclamosByPolizaIdAsync(polizaId);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByEstadoAsync(EstadoReclamo estado)
        {
            var reclamos = await _reclamoRepository.GetReclamosByEstadoAsync(estado);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByTipoAsync(TipoReclamo tipo)
        {
            var reclamos = await _reclamoRepository.GetReclamosByTipoAsync(tipo);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByPrioridadAsync(PrioridadReclamo prioridad)
        {
            var reclamos = await _reclamoRepository.GetReclamosByPrioridadAsync(prioridad);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByUsuarioAsignadoAsync(int usuarioId)
        {
            var reclamos = await _reclamoRepository.GetReclamosByUsuarioAsignadoAsync(usuarioId);
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosVencidosAsync()
        {
            var reclamos = await _reclamoRepository.GetReclamosVencidosAsync();
            return _mapper.Map<IEnumerable<ReclamoDto>>(reclamos);
        }

        public async Task<IEnumerable<ReclamoDto>> GetReclamosByFiltroAsync(ReclamoFilterDto filtro)
        {
            // Implementar la lógica de filtrado directamente en el servicio
            var reclamos = await _reclamoRepository.GetAllAsync();
            
            var query = reclamos.Where(r => !r.IsDeleted);

            if (filtro.Estado.HasValue)
                query = query.Where(r => (int)r.Estado == filtro.Estado.Value);

            if (filtro.TipoReclamo.HasValue)
                query = query.Where(r => (int)r.TipoReclamo == filtro.TipoReclamo.Value);

            if (filtro.Prioridad.HasValue)
                query = query.Where(r => (int)r.Prioridad == filtro.Prioridad.Value);

            if (!string.IsNullOrEmpty(filtro.ClienteNombre))
                query = query.Where(r => r.ClienteNombre.Contains(filtro.ClienteNombre) ||
                                        r.ClienteApellido.Contains(filtro.ClienteNombre));

            if (!string.IsNullOrEmpty(filtro.NumeroPoliza))
                query = query.Where(r => r.NumeroPoliza.Contains(filtro.NumeroPoliza));

            if (filtro.FechaDesde.HasValue)
                query = query.Where(r => r.FechaReclamo >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(r => r.FechaReclamo <= filtro.FechaHasta.Value);

            if (filtro.UsuarioAsignadoId.HasValue)
                query = query.Where(r => r.UsuarioAsignadoId == filtro.UsuarioAsignadoId.Value);

            if (filtro.SoloVencidos == true)
            {
                var fechaActual = DateTime.UtcNow;
                query = query.Where(r => r.FechaLimiteRespuesta.HasValue && 
                                        r.FechaLimiteRespuesta.Value < fechaActual &&
                                        r.Estado != EstadoReclamo.Resuelto &&
                                        r.Estado != EstadoReclamo.Cerrado);
            }

            if (!string.IsNullOrEmpty(filtro.Moneda))
                query = query.Where(r => r.Moneda == filtro.Moneda);

            var resultado = query.OrderByDescending(r => r.FechaReclamo).ToList();
            return _mapper.Map<IEnumerable<ReclamoDto>>(resultado);
        }

        public async Task<ReclamoStatsDto> GetReclamosStatsAsync()
        {
            var reclamos = await _reclamoRepository.GetAllAsync();
            var reclamosList = reclamos.Where(r => !r.IsDeleted).ToList();

            var fechaActual = DateTime.UtcNow;
            var reclamosVencidos = reclamosList.Where(r => r.FechaLimiteRespuesta.HasValue && 
                                                      r.FechaLimiteRespuesta.Value < fechaActual &&
                                                      r.Estado != EstadoReclamo.Resuelto &&
                                                      r.Estado != EstadoReclamo.Cerrado).Count();

            return new ReclamoStatsDto
            {
                TotalReclamos = reclamosList.Count,
                ReclamosAbiertos = reclamosList.Count(r => r.Estado == EstadoReclamo.Abierto),
                ReclamosEnProceso = reclamosList.Count(r => r.Estado == EstadoReclamo.EnProceso),
                ReclamosResueltos = reclamosList.Count(r => r.Estado == EstadoReclamo.Resuelto),
                ReclamosCerrados = reclamosList.Count(r => r.Estado == EstadoReclamo.Cerrado),
                ReclamosRechazados = reclamosList.Count(r => r.Estado == EstadoReclamo.Rechazado),
                TotalMontoReclamado = reclamosList.Sum(r => r.MontoReclamado ?? 0),
                TotalMontoAprobado = reclamosList.Sum(r => r.MontoAprobado ?? 0),
                MonedaPrincipal = "CRC",
                ReclamosPrioridadAlta = reclamosList.Count(r => r.Prioridad == PrioridadReclamo.Alta),
                ReclamosPrioridadCritica = reclamosList.Count(r => r.Prioridad == PrioridadReclamo.Critica),
                ReclamosVencidos = reclamosVencidos
            };
        }

        public async Task<ReclamoDto?> AsignarUsuarioAsync(int reclamoId, int usuarioId)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null) return null;

            reclamo.UsuarioAsignadoId = usuarioId;
            if (reclamo.Estado == EstadoReclamo.Abierto)
            {
                reclamo.Estado = EstadoReclamo.EnProceso;
            }

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto?> CambiarEstadoAsync(int reclamoId, EstadoReclamo nuevoEstado)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null) return null;

            reclamo.Estado = nuevoEstado;
            
            if (nuevoEstado == EstadoReclamo.Resuelto || nuevoEstado == EstadoReclamo.Cerrado)
            {
                reclamo.FechaResolucion = DateTime.UtcNow;
            }

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto?> ResolverReclamoAsync(int reclamoId, decimal? montoAprobado, string? observaciones)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null) return null;

            reclamo.Estado = EstadoReclamo.Resuelto;
            reclamo.FechaResolucion = DateTime.UtcNow;
            reclamo.MontoAprobado = montoAprobado;
            if (!string.IsNullOrEmpty(observaciones))
            {
                reclamo.Observaciones = observaciones;
            }

            await _reclamoRepository.UpdateAsync(reclamo);
            return _mapper.Map<ReclamoDto>(reclamo);
        }
    }
}