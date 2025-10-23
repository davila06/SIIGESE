using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }

    public class CreateClienteValidator : AbstractValidator<CreateClienteDto>
    {
        public CreateClienteValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El código es requerido")
                .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres");

            RuleFor(x => x.RazonSocial)
                .NotEmpty().WithMessage("La razón social es requerida")
                .MaximumLength(200).WithMessage("La razón social no puede exceder 200 caracteres");

            RuleFor(x => x.NIT)
                .NotEmpty().WithMessage("El NIT es requerido")
                .MaximumLength(20).WithMessage("El NIT no puede exceder 20 caracteres");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del email no es válido")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PerfilId)
                .GreaterThan(0).WithMessage("Debe seleccionar un perfil válido");
        }
    }

    public class CreatePolizaValidator : AbstractValidator<CreatePolizaDto>
    {
        public CreatePolizaValidator()
        {
            RuleFor(x => x.NumeroPoliza)
                .NotEmpty().WithMessage("El número de póliza es requerido")
                .MaximumLength(50).WithMessage("El número de póliza no puede exceder 50 caracteres");

            RuleFor(x => x.NombreAsegurado)
                .NotEmpty().WithMessage("El nombre del asegurado es requerido")
                .MaximumLength(200).WithMessage("El nombre del asegurado no puede exceder 200 caracteres");

            RuleFor(x => x.Prima)
                .GreaterThan(0).WithMessage("La prima debe ser mayor a 0");

            RuleFor(x => x.Moneda)
                .NotEmpty().WithMessage("La moneda es requerida")
                .Must(x => x == "CRC" || x == "USD" || x == "EUR")
                .WithMessage("La moneda debe ser CRC, USD o EUR");

            RuleFor(x => x.FechaVigencia)
                .GreaterThan(DateTime.Today.AddDays(-30))
                .WithMessage("La fecha de vigencia no puede ser muy antigua");

            RuleFor(x => x.Frecuencia)
                .NotEmpty().WithMessage("La frecuencia es requerida")
                .Must(x => x == "Mensual" || x == "Trimestral" || x == "Semestral" || x == "Anual")
                .WithMessage("La frecuencia debe ser Mensual, Trimestral, Semestral o Anual");

            RuleFor(x => x.Aseguradora)
                .NotEmpty().WithMessage("La aseguradora es requerida")
                .Must(x => x == "Instituto Nacional de Seguros (INS)" || 
                          x == "ASSA Compañía de Seguros S.A." || 
                          x == "Pan-American Life Insurance de Costa Rica, S.A. (PALIG)" || 
                          x == "Davivienda Seguros (Costa Rica)" || 
                          x == "MNK Seguros Compañía Aseguradora" || 
                          x == "Aseguradora del Istmo (ADISA)")
                .WithMessage("La aseguradora debe ser una de las opciones válidas del sistema");

            RuleFor(x => x.Placa)
                .MaximumLength(10).WithMessage("La placa no puede exceder 10 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Placa));

            RuleFor(x => x.PerfilId)
                .GreaterThan(0).WithMessage("Debe seleccionar un perfil válido");
        }
    }

    public class CreateCotizacionValidator : AbstractValidator<CreateCotizacionDto>
    {
        public CreateCotizacionValidator()
        {
            RuleFor(x => x.NombreSolicitante)
                .NotEmpty().WithMessage("El nombre del solicitante es requerido")
                .MaximumLength(200).WithMessage("El nombre del solicitante no puede exceder 200 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido");

            RuleFor(x => x.TipoSeguro)
                .NotEmpty().WithMessage("El tipo de seguro es requerido")
                .Must(x => x == "AUTO" || x == "VIDA" || x == "HOGAR" || x == "EMPRESARIAL")
                .WithMessage("El tipo de seguro debe ser AUTO, VIDA, HOGAR o EMPRESARIAL");

            RuleFor(x => x.Aseguradora)
                .NotEmpty().WithMessage("La aseguradora es requerida")
                .Must(x => x == "Instituto Nacional de Seguros (INS)" || 
                          x == "ASSA Compañía de Seguros S.A." || 
                          x == "Pan-American Life Insurance de Costa Rica, S.A. (PALIG)" || 
                          x == "Davivienda Seguros (Costa Rica)" || 
                          x == "MNK Seguros Compañía Aseguradora" || 
                          x == "Aseguradora del Istmo (ADISA)")
                .WithMessage("La aseguradora debe ser una de las opciones válidas del sistema");

            RuleFor(x => x.MontoAsegurado)
                .GreaterThan(0).WithMessage("El monto asegurado debe ser mayor a 0");

            RuleFor(x => x.Moneda)
                .NotEmpty().WithMessage("La moneda es requerida")
                .Must(x => x == "CRC" || x == "USD" || x == "EUR")
                .WithMessage("La moneda debe ser CRC, USD o EUR");
        }
    }
}