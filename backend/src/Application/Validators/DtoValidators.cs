using FluentValidation;
using Application.DTOs;

namespace Application.Validators
{
    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }

    public class CreateClienteDtoValidator : AbstractValidator<CreateClienteDto>
    {
        public CreateClienteDtoValidator()
        {
            RuleFor(x => x.NumeroIdentificacion)
                .NotEmpty().WithMessage("El número de identificación es requerido");

            RuleFor(x => x.TipoIdentificacion)
                .NotEmpty().WithMessage("El tipo de identificación es requerido");

            RuleFor(x => x.PrimerNombre)
                .NotEmpty().WithMessage("El primer nombre es requerido");

            RuleFor(x => x.PrimerApellido)
                .NotEmpty().WithMessage("El primer apellido es requerido");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El email debe tener un formato válido")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.FechaNacimiento)
                .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser anterior a hoy");
        }
    }

    public class CreatePolizaDtoValidator : AbstractValidator<CreatePolizaDto>
    {
        public CreatePolizaDtoValidator()
        {
            RuleFor(x => x.NumeroPoliza)
                .NotEmpty().WithMessage("El número de póliza es requerido");

            RuleFor(x => x.ClienteId)
                .GreaterThan(0).WithMessage("Debe seleccionar un cliente válido");

            RuleFor(x => x.TipoSeguro)
                .NotEmpty().WithMessage("El tipo de seguro es requerido");

            RuleFor(x => x.FechaInicio)
                .NotEmpty().WithMessage("La fecha de inicio es requerida");

            RuleFor(x => x.FechaVencimiento)
                .GreaterThan(x => x.FechaInicio).WithMessage("La fecha de vencimiento debe ser posterior a la fecha de inicio");

            RuleFor(x => x.MontoAsegurado)
                .GreaterThan(0).WithMessage("El monto asegurado debe ser mayor a 0");

            RuleFor(x => x.PrimaNeta)
                .GreaterThan(0).WithMessage("La prima neta debe ser mayor a 0");
        }
    }

    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email debe tener un formato válido");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El primer nombre es requerido");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }

    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email debe tener un formato válido");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El primer nombre es requerido");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido");
        }
    }
}
