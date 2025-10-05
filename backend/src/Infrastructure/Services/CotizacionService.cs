using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class CotizacionService : ICotizacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CotizacionService> _logger;

        public CotizacionService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CotizacionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CotizacionDto>> GetAllAsync()
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CotizacionDto>>(cotizaciones);
        }

        public async Task<CotizacionDto?> GetByIdAsync(int id)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            return cotizacion == null ? null : _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<CotizacionDto> CreateAsync(CreateCotizacionDto createDto, int usuarioId)
        {
            var cotizacion = _mapper.Map<Cotizacion>(createDto);
            cotizacion.UsuarioId = usuarioId;
            cotizacion.NumeroCotizacion = await GenerateNumeroCotizacionAsync();

            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {NumeroCotizacion} creada por usuario {UsuarioId}", 
                cotizacion.NumeroCotizacion, usuarioId);

            // Cargar la entidad completa con el usuario
            await _context.Entry(cotizacion)
                .Reference(c => c.Usuario)
                .LoadAsync();

            return _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<CotizacionDto> UpdateAsync(int id, UpdateCotizacionDto updateDto)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                throw new KeyNotFoundException($"Cotización con ID {id} no encontrada");
            }

            _mapper.Map(updateDto, cotizacion);
            cotizacion.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Id} actualizada", id);

            return _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task DeleteAsync(int id)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);
            if (cotizacion == null)
            {
                throw new KeyNotFoundException($"Cotización con ID {id} no encontrada");
            }

            _context.Cotizaciones.Remove(cotizacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Id} eliminada", id);
        }

        public async Task<IEnumerable<CotizacionDto>> SearchAsync(CotizacionSearchDto searchDto)
        {
            var query = _context.Cotizaciones
                .Include(c => c.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.NumeroCotizacion))
            {
                query = query.Where(c => c.NumeroCotizacion.Contains(searchDto.NumeroCotizacion));
            }

            if (!string.IsNullOrEmpty(searchDto.NombreSolicitante))
            {
                query = query.Where(c => c.NombreSolicitante.Contains(searchDto.NombreSolicitante));
            }

            if (!string.IsNullOrEmpty(searchDto.Email))
            {
                query = query.Where(c => c.Email.Contains(searchDto.Email));
            }

            if (!string.IsNullOrEmpty(searchDto.TipoSeguro))
            {
                query = query.Where(c => c.TipoSeguro == searchDto.TipoSeguro);
            }

            if (!string.IsNullOrEmpty(searchDto.Estado))
            {
                query = query.Where(c => c.Estado == searchDto.Estado);
            }

            if (searchDto.FechaDesde.HasValue)
            {
                query = query.Where(c => c.FechaCotizacion >= searchDto.FechaDesde.Value);
            }

            if (searchDto.FechaHasta.HasValue)
            {
                query = query.Where(c => c.FechaCotizacion <= searchDto.FechaHasta.Value);
            }

            // Paginación
            query = query.OrderByDescending(c => c.FechaCreacion)
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize);

            var cotizaciones = await query.ToListAsync();
            return _mapper.Map<IEnumerable<CotizacionDto>>(cotizaciones);
        }

        public async Task<CotizacionDto> UpdateEstadoAsync(int id, string estado)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                throw new KeyNotFoundException($"Cotización con ID {id} no encontrada");
            }

            cotizacion.Estado = estado;
            cotizacion.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Estado de cotización {Id} actualizado a {Estado}", id, estado);

            return _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<IEnumerable<CotizacionDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CotizacionDto>>(cotizaciones);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cotizaciones.AnyAsync(c => c.Id == id);
        }

        public async Task<string> GenerateNumeroCotizacionAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"COT{year}";
            
            var lastCotizacion = await _context.Cotizaciones
                .Where(c => c.NumeroCotizacion.StartsWith(prefix))
                .OrderByDescending(c => c.NumeroCotizacion)
                .FirstOrDefaultAsync();

            if (lastCotizacion == null)
            {
                return $"{prefix}0001";
            }

            var lastNumber = lastCotizacion.NumeroCotizacion.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out int number))
            {
                return $"{prefix}{(number + 1):D4}";
            }

            return $"{prefix}0001";
        }
    }
}