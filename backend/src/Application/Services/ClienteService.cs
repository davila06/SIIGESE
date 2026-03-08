using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClienteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClienteDto>> GetAllAsync()
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<IEnumerable<ClienteDto>> GetByPerfilIdAsync(int perfilId)
        {
            var clientes = await _unitOfWork.Clientes.GetByPerfilIdAsync(perfilId);
            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<ClienteDto?> GetByIdAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            return cliente == null ? null : _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
        {
            var cliente = new Cliente();
            ApplyDto(dto, cliente);
            cliente.FechaRegistro = DateTime.UtcNow;
            await _unitOfWork.Clientes.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDto> UpdateAsync(int id, CreateClienteDto dto)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");
            ApplyDto(dto, cliente);
            await _unitOfWork.Clientes.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");
            await _unitOfWork.Clientes.DeleteAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<DataUploadResultDto> ProcesarExcelAsync(int perfilId, IFormFile file, int userId)
        {
            var result = new DataUploadResultDto
            {
                Success = false,
                Message = "La carga masiva de clientes mediante Excel aún no está configurada.",
                TotalRecords = 0,
                ProcessedRecords = 0,
                ErrorRecords = 0,
                Status = "NotImplemented"
            };
            result.Errors.Add(result.Message);
            return Task.FromResult(result);
        }

        private static void ApplyDto(CreateClienteDto dto, Cliente cliente)
        {
            var partes = new[]
            {
                dto.PrimerNombre?.Trim(),
                dto.SegundoNombre?.Trim(),
                dto.PrimerApellido?.Trim(),
                dto.SegundoApellido?.Trim()
            }.Where(p => !string.IsNullOrWhiteSpace(p));

            cliente.RazonSocial = string.Join(" ", partes);
            cliente.Codigo = dto.Codigo ?? string.Empty;
            cliente.NIT = dto.NumeroIdentificacion ?? string.Empty;
            cliente.Telefono = dto.Telefono ?? string.Empty;
            cliente.Email = dto.Email ?? string.Empty;
            cliente.Direccion = dto.Direccion ?? string.Empty;
            cliente.Ciudad = dto.Canton ?? string.Empty;
            cliente.Departamento = dto.Provincia ?? string.Empty;
            cliente.EsActivo = true;
        }
    }
}
