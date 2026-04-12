using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        private readonly IReclamoHistorialRepository _historialRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ReclamoService(
            IReclamoRepository reclamoRepository,
            IReclamoHistorialRepository historialRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _reclamoRepository  = reclamoRepository;
            _historialRepository = historialRepository;
            _unitOfWork          = unitOfWork;
            _mapper              = mapper;
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

        public async Task<PagedResultDto<ReclamoDto>> GetReclamosByFiltroAsync(ReclamoFilterDto filtro)
        {
            // Map DTO → Domain filter (keeps Domain free of Application references)
            var queryFilter = new Domain.Queries.ReclamoQueryFilter
            {
                NumeroPoliza          = filtro.NumeroPoliza,
                NumeroReclamo         = filtro.NumeroReclamo,
                Estado                = filtro.Estado,
                TipoReclamo           = filtro.TipoReclamo,
                Prioridad             = filtro.Prioridad,
                FechaDesde            = filtro.FechaDesde,
                FechaHasta            = filtro.FechaHasta,
                UsuarioAsignadoId     = filtro.UsuarioAsignadoId,
                ClienteNombreCompleto = filtro.ClienteNombreCompleto,
                PageNumber            = filtro.PageNumber,
                PageSize              = filtro.PageSize
            };

            var (items, totalCount) = await _reclamoRepository.GetByFiltroAsync(queryFilter);

            return new PagedResultDto<ReclamoDto>
            {
                Items      = _mapper.Map<IEnumerable<ReclamoDto>>(items),
                TotalCount = totalCount,
                PageNumber = filtro.PageNumber,
                PageSize   = filtro.PageSize
            };
        }

        public async Task<ReclamoStatsDto> GetReclamosStatsAsync()
        {
            // Use dedicated COUNT/SUM queries — no full table scan
            var totalReclamos      = await _reclamoRepository.GetTotalCountAsync();
            var pendientes         = await _reclamoRepository.GetCountByEstadoAsync(EstadoReclamo.Pendiente);
            var enProceso          = await _reclamoRepository.GetCountByEstadoAsync(EstadoReclamo.EnProceso);
            var resueltos          = await _reclamoRepository.GetCountByEstadoAsync(EstadoReclamo.Resuelto);
            var rechazados         = await _reclamoRepository.GetCountByEstadoAsync(EstadoReclamo.Rechazado);
            var montoTotalReclamado = await _reclamoRepository.GetMontoTotalReclamadoAsync();
            var montoTotalAprobado  = await _reclamoRepository.GetMontoTotalAprobadoAsync();

            return new ReclamoStatsDto
            {
                TotalReclamos      = totalReclamos,
                ReclamosPendientes = pendientes,
                ReclamosEnProceso  = enProceso,
                ReclamosResueltos  = resueltos,
                ReclamosRechazados = rechazados,
                MontoTotalReclamado = montoTotalReclamado,
                MontoTotalAprobado  = montoTotalAprobado,
                TasaAprobacion = montoTotalReclamado > 0
                    ? (montoTotalAprobado / montoTotalReclamado) * 100
                    : 0
            };
        }

        public async Task<string> GenerateNumeroReclamoAsync()
        {
            return await _reclamoRepository.GenerateNumeroReclamoAsync();
        }

        public async Task<ReclamoDto> CreateReclamoAsync(CreateReclamoDto request, string usuario = "Sistema")
        {
            Poliza? poliza = null;
            if (!string.IsNullOrEmpty(request.NumeroPoliza))
                poliza = await _unitOfWork.Polizas.GetByNumeroPolizaAsync(request.NumeroPoliza);

            var reclamo = _mapper.Map<Reclamo>(request);

            if (poliza != null)
            {
                reclamo.NombreAsegurado        = poliza.NombreAsegurado ?? string.Empty;
                reclamo.ClienteNombreCompleto  = poliza.NombreAsegurado ?? string.Empty;
                reclamo.NumeroPoliza           = poliza.NumeroPoliza    ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(reclamo.ClienteNombreCompleto))
            {
                reclamo.ClienteNombreCompleto = request.NombreAsegurado;
            }

            if (string.IsNullOrEmpty(reclamo.NumeroReclamo))
                reclamo.NumeroReclamo = await GenerateNumeroReclamoAsync();

            reclamo.FechaReclamo = DateTime.UtcNow;
            reclamo.CreatedAt    = DateTime.UtcNow;
            reclamo.CreatedBy    = usuario;

            await _reclamoRepository.AddAsync(reclamo);
            await _unitOfWork.SaveChangesAsync();

            // Audit: creation event
            await RegistrarEventoHistorialAsync(
                reclamo.Id, "Creacion",
                $"Reclamo {reclamo.NumeroReclamo} creado.",
                valorNuevo: reclamo.Estado.ToString(),
                usuario: usuario);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> UpdateReclamoAsync(int id, UpdateReclamoDto request, string usuario = "Sistema")
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(id);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {id} no encontrado");

            _mapper.Map(request, reclamo);
            reclamo.UpdatedAt = DateTime.UtcNow;
            reclamo.UpdatedBy = usuario;

            await _reclamoRepository.UpdateAsync(reclamo);

            // Audit: generic update
            await RegistrarEventoHistorialAsync(
                id, "Actualizacion",
                "Información del reclamo actualizada.",
                usuario: usuario);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> AsignarUsuarioAsync(int reclamoId, int usuarioId, string usuario = "Sistema")
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            var anteriorId = reclamo.UsuarioAsignadoId?.ToString() ?? "Sin asignar";
            reclamo.UsuarioAsignadoId = usuarioId;
            reclamo.UpdatedAt         = DateTime.UtcNow;
            reclamo.UpdatedBy         = usuario;

            await _reclamoRepository.UpdateAsync(reclamo);

            await RegistrarEventoHistorialAsync(
                reclamoId, "Asignacion",
                $"Reclamo asignado al usuario #{usuarioId}.",
                valorAnterior: anteriorId,
                valorNuevo:    usuarioId.ToString(),
                usuario:       usuario);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> CambiarEstadoAsync(int reclamoId, EstadoReclamo nuevoEstado, string usuario = "Sistema")
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            var estadoAnterior = reclamo.Estado;
            reclamo.Estado     = nuevoEstado;
            reclamo.UpdatedAt  = DateTime.UtcNow;
            reclamo.UpdatedBy  = usuario;

            await _reclamoRepository.UpdateAsync(reclamo);

            await RegistrarEventoHistorialAsync(
                reclamoId, "CambioEstado",
                $"Estado cambiado de {estadoAnterior} a {nuevoEstado}.",
                valorAnterior: estadoAnterior.ToString(),
                valorNuevo:    nuevoEstado.ToString(),
                usuario:       usuario);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task<ReclamoDto> ResolverReclamoAsync(int reclamoId, decimal? montoAprobado, string observaciones, string usuario = "Sistema")
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null)
                throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            var estadoAnterior      = reclamo.Estado;
            reclamo.Estado          = EstadoReclamo.Resuelto;
            reclamo.MontoAprobado   = montoAprobado;
            reclamo.FechaResolucion = DateTime.UtcNow;
            reclamo.Observaciones   = observaciones ?? reclamo.Observaciones;
            reclamo.UpdatedAt       = DateTime.UtcNow;
            reclamo.UpdatedBy       = usuario;

            await _reclamoRepository.UpdateAsync(reclamo);

            await RegistrarEventoHistorialAsync(
                reclamoId, "Resolucion",
                $"Reclamo resuelto. Monto aprobado: {montoAprobado?.ToString("C") ?? "N/A"}.",
                valorAnterior: estadoAnterior.ToString(),
                valorNuevo:    EstadoReclamo.Resuelto.ToString(),
                usuario:       usuario);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReclamoDto>(reclamo);
        }

        public async Task DeleteReclamoAsync(int id)
        {
            await _reclamoRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Historial ─────────────────────────────────────────────────────────

        public async Task<IEnumerable<ReclamoHistorialEntryDto>> GetHistorialAsync(int reclamoId)
        {
            var entries = await _historialRepository.GetByReclamoIdAsync(reclamoId);
            return entries.Select(e => new ReclamoHistorialEntryDto
            {
                Id            = e.Id,
                ReclamoId     = e.ReclamoId,
                TipoEvento    = e.TipoEvento,
                ValorAnterior = e.ValorAnterior,
                ValorNuevo    = e.ValorNuevo,
                Descripcion   = e.Descripcion,
                Usuario       = e.Usuario,
                FechaEvento   = e.CreatedAt
            });
        }

        // ── Documentos ────────────────────────────────────────────────────────

        public async Task<IEnumerable<ReclamoDocumentoDto>> GetDocumentosAsync(int reclamoId)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null || string.IsNullOrWhiteSpace(reclamo.DocumentosAdjuntos))
                return Enumerable.Empty<ReclamoDocumentoDto>();

            var metadata = ParseDocumentosJson(reclamo.DocumentosAdjuntos);
            // Strip physical path from public DTO
            return metadata.Select(m => (ReclamoDocumentoDto)m);
        }

        public async Task<ReclamoDocumentoDto> AddDocumentoMetadataAsync(
            int    reclamoId,
            string docId,
            string nombre,
            long   tamano,
            string tipoContenido,
            string ruta,
            string usuario)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId)
                ?? throw new ArgumentException($"Reclamo con ID {reclamoId} no encontrado");

            var metadata = ParseDocumentosJson(reclamo.DocumentosAdjuntos);

            var nuevo = new ReclamoDocumentoMetadata
            {
                Id            = docId,
                Nombre        = nombre,
                Tamano        = tamano,
                TipoContenido = tipoContenido,
                FechaSubida   = DateTime.UtcNow,
                SubidoPor     = usuario,
                Ruta          = ruta
            };
            metadata.Add(nuevo);

            reclamo.DocumentosAdjuntos = JsonSerializer.Serialize(metadata, _jsonOptions);
            reclamo.UpdatedAt          = DateTime.UtcNow;
            reclamo.UpdatedBy          = usuario;

            await _reclamoRepository.UpdateAsync(reclamo);

            await RegistrarEventoHistorialAsync(
                reclamoId, "DocumentoAgregado",
                $"Documento adjuntado: {nombre}.",
                valorNuevo: nombre,
                usuario:    usuario);

            await _unitOfWork.SaveChangesAsync();
            return nuevo;
        }

        public async Task<string?> RemoveDocumentoAsync(int reclamoId, string docId, string usuario = "Sistema")
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null) return null;

            var metadata = ParseDocumentosJson(reclamo.DocumentosAdjuntos);
            var target   = metadata.FirstOrDefault(d => d.Id == docId);
            if (target == null) return null;

            metadata.Remove(target);
            reclamo.DocumentosAdjuntos = metadata.Count > 0
                ? JsonSerializer.Serialize(metadata, _jsonOptions)
                : string.Empty;
            reclamo.UpdatedAt = DateTime.UtcNow;
            reclamo.UpdatedBy = usuario;

            await _reclamoRepository.UpdateAsync(reclamo);

            await RegistrarEventoHistorialAsync(
                reclamoId, "DocumentoEliminado",
                $"Documento eliminado: {target.Nombre}.",
                valorAnterior: target.Nombre,
                usuario:       usuario);

            await _unitOfWork.SaveChangesAsync();
            return target.Ruta;
        }

        public async Task<ReclamoDocumentoMetadata?> GetDocumentoMetadataAsync(int reclamoId, string docId)
        {
            var reclamo = await _reclamoRepository.GetByIdAsync(reclamoId);
            if (reclamo == null || string.IsNullOrWhiteSpace(reclamo.DocumentosAdjuntos))
                return null;

            return ParseDocumentosJson(reclamo.DocumentosAdjuntos)
                .FirstOrDefault(d => d.Id == docId);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Persists one immutable audit-trail entry.  Caller must call SaveChanges afterwards.
        /// </summary>
        private async Task RegistrarEventoHistorialAsync(
            int     reclamoId,
            string  tipoEvento,
            string  descripcion,
            string? valorAnterior = null,
            string? valorNuevo    = null,
            string  usuario       = "Sistema")
        {
            var entry = new ReclamoHistorial
            {
                ReclamoId     = reclamoId,
                TipoEvento    = tipoEvento,
                Descripcion   = descripcion,
                ValorAnterior = valorAnterior,
                ValorNuevo    = valorNuevo,
                Usuario       = usuario,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = usuario
            };
            await _historialRepository.AddAsync(entry);
        }

        private static List<ReclamoDocumentoMetadata> ParseDocumentosJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<ReclamoDocumentoMetadata>();

            try
            {
                return JsonSerializer.Deserialize<List<ReclamoDocumentoMetadata>>(json, _jsonOptions)
                       ?? new List<ReclamoDocumentoMetadata>();
            }
            catch
            {
                return new List<ReclamoDocumentoMetadata>();
            }
        }
    }
}