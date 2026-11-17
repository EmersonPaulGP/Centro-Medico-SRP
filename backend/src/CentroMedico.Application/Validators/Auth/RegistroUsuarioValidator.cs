using CentroMedico.Application.DTOs.Auth;
using CentroMedico.Domain.Constants;
using FluentValidation;

namespace CentroMedico.Application.Validators.Auth;

/// <summary>
/// Validador FluentValidation para el registro de nuevos usuarios del sistema.
/// </summary>
public class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDto>
{
    public RegistroUsuarioValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(100)
            .WithMessage("El correo electrónico no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6)
            .WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El nombre no puede superar los 50 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty()
            .WithMessage("El apellido es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El apellido no puede superar los 50 caracteres.");

        RuleFor(x => x.Rol)
            .NotEmpty()
            .WithMessage("El rol es obligatorio.")
            .Must(Roles.EsRolValido)
            .WithMessage($"El rol no es válido. Roles permitidos: {string.Join(", ", Roles.Todos)}.");
    }
}
