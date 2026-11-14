using CentroMedico.Application.DTOs.Medicos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Medicos;

public class ActualizarMedicoValidator : AbstractValidator<ActualizarMedicoDto>
{
    public ActualizarMedicoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres.");

        RuleFor(x => x.Telefono)
            .MaximumLength(25).When(x => !string.IsNullOrEmpty(x.Telefono))
            .WithMessage("El teléfono no puede exceder 25 caracteres.");

        RuleFor(x => x.Email)
            .MaximumLength(150).When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("El correo electrónico no puede exceder 150 caracteres.")
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("El correo electrónico no tiene un formato válido.");
    }
}
