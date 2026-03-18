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

            RuleFor(x => x.NombreAsegurado)
                .NotEmpty().WithMessage("El nombre del asegurado es requerido");

            RuleFor(x => x.Prima)
                .GreaterThanOrEqualTo(0).WithMessage("La prima no puede ser negativa");

            RuleFor(x => x.Moneda)
                .NotEmpty().WithMessage("La moneda es requerida");

            RuleFor(x => x.FechaVigencia)
                .NotEmpty().WithMessage("La fecha de vigencia es requerida");

            RuleFor(x => x.Frecuencia)
                .NotEmpty().WithMessage("La frecuencia es requerida");

            RuleFor(x => x.Aseguradora)
                .NotEmpty().WithMessage("La aseguradora es requerida");
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

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("La contraseña actual es requerida");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida")
                .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden");
        }
    }

    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email debe tener un formato válido");
        }
    }

    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("El token de reseteo es requerido");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida")
                .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden");
        }
    }
}
