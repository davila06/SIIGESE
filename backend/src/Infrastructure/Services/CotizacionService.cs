using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class CotizacionService : ICotizacionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CotizacionService> _logger;

        public CotizacionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CotizacionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CotizacionDto>> GetAllAsync()
        {
            var cotizaciones = await _unitOfWork.Cotizaciones.GetAllAsync();
            return _mapper.Map<IEnumerable<CotizacionDto>>(cotizaciones);
        }

        public async Task<CotizacionDto?> GetByIdAsync(int id)
        {
            var cotizacion = await _unitOfWork.Cotizaciones.GetByIdAsync(id);
            return cotizacion == null ? null : _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<CotizacionDto> CreateAsync(CreateCotizacionDto createDto, int usuarioId)
        {
            var cotizacion = _mapper.Map<Cotizacion>(createDto);
            cotizacion.UsuarioId = usuarioId;
            cotizacion.NumeroCotizacion = await GenerateNumeroCotizacionAsync();
            cotizacion.FechaCotizacion = DateTime.UtcNow;
            cotizacion.FechaCreacion = DateTime.UtcNow;
            cotizacion.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Cotizaciones.AddAsync(cotizacion);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<CotizacionDto> UpdateAsync(int id, UpdateCotizacionDto updateDto)
        {
            var cotizacion = await _unitOfWork.Cotizaciones.GetByIdAsync(id);
            if (cotizacion == null)
            {
                throw new KeyNotFoundException($"Cotización con ID {id} no encontrada");
            }

            _mapper.Map(updateDto, cotizacion);
            cotizacion.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Cotizaciones.UpdateAsync(cotizacion);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cotizacion = await _unitOfWork.Cotizaciones.GetByIdAsync(id);
            if (cotizacion == null)
            {
                return false;
            }

            await _unitOfWork.Cotizaciones.DeleteAsync(cotizacion);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CotizacionDto>> SearchAsync(CotizacionSearchDto searchDto)
        {
            IEnumerable<Cotizacion> cotizaciones;

            if (!string.IsNullOrEmpty(searchDto.NombreSolicitante))
            {
                cotizaciones = await _unitOfWork.Cotizaciones.GetByClienteAsync(searchDto.NombreSolicitante);
            }
            else if (searchDto.FechaDesde.HasValue && searchDto.FechaHasta.HasValue)
            {
                cotizaciones = await _unitOfWork.Cotizaciones.GetByFechaRangeAsync(searchDto.FechaDesde.Value, searchDto.FechaHasta.Value);
            }
            else
            {
                cotizaciones = await _unitOfWork.Cotizaciones.GetAllAsync();
            }

            // Aplicar filtros adicionales
            if (!string.IsNullOrEmpty(searchDto.Estado))
            {
                cotizaciones = cotizaciones.Where(c => c.Estado == searchDto.Estado);
            }

            if (!string.IsNullOrEmpty(searchDto.NumeroCotizacion))
            {
                cotizaciones = cotizaciones.Where(c => c.NumeroCotizacion.Contains(searchDto.NumeroCotizacion));
            }

            if (!string.IsNullOrEmpty(searchDto.Email))
            {
                cotizaciones = cotizaciones.Where(c => c.Email.Contains(searchDto.Email));
            }

            if (!string.IsNullOrEmpty(searchDto.TipoSeguro))
            {
                cotizaciones = cotizaciones.Where(c => c.TipoSeguro == searchDto.TipoSeguro);
            }

            return _mapper.Map<IEnumerable<CotizacionDto>>(cotizaciones);
        }

        public async Task<CotizacionDto> UpdateEstadoAsync(int id, string estado)
        {
            var cotizacion = await _unitOfWork.Cotizaciones.GetByIdAsync(id);
            if (cotizacion == null)
            {
                throw new KeyNotFoundException($"Cotización con ID {id} no encontrada");
            }

            cotizacion.Estado = estado;
            cotizacion.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Cotizaciones.UpdateAsync(cotizacion);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CotizacionDto>(cotizacion);
        }

        public async Task<IEnumerable<CotizacionDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var cotizaciones = await _unitOfWork.Cotizaciones.GetAllAsync();
            var cotizacionesUsuario = cotizaciones.Where(c => c.UsuarioId == usuarioId);
            return _mapper.Map<IEnumerable<CotizacionDto>>(cotizacionesUsuario);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var cotizacion = await _unitOfWork.Cotizaciones.GetByIdAsync(id);
            return cotizacion != null;
        }

        public async Task<string> GenerateNumeroCotizacionAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"COT-{year}-";
            
            var cotizaciones = await _unitOfWork.Cotizaciones.GetAllAsync();
            var cotizacionesActuales = cotizaciones
                .Where(c => c.NumeroCotizacion.StartsWith(prefix))
                .ToList();

            if (!cotizacionesActuales.Any())
            {
                return $"{prefix}001";
            }

            var ultimoNumero = cotizacionesActuales
                .Select(c => int.Parse(c.NumeroCotizacion.Substring(prefix.Length)))
                .Max();

            return $"{prefix}{(ultimoNumero + 1):D3}";
        }
    }
}