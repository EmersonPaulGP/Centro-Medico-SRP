using CentroMedico.Application.DTOs.Auth;
using FluentValidation;

namespace CentroMedico.Application.Validators.Auth;

/// <summary>
/// Validador FluentValidation para el inicio de sesión.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
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
            .WithMessage("La contraseña es obligatoria.");
    }
}
