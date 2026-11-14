using CentroMedico.Application.DTOs.Pacientes;
using FluentValidation;

namespace CentroMedico.Application.Validators.Pacientes;

/// <summary>
/// Validador de entrada para el registro de nuevos pacientes.
/// </summary>
public class CrearPacienteValidator : AbstractValidator<CrearPacienteDto>
{
    public CrearPacienteValidator()
    {
        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El documento de identidad (DNI) es obligatorio.")
            .MaximumLength(20).WithMessage("El DNI no puede exceder 20 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres.");

        RuleFor(x => x.FNacimiento)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de nacimiento debe ser una fecha pasada.")
            .GreaterThan(new DateOnly(1900, 1, 1))
            .WithMessage("La fecha de nacimiento no es válida.");

        RuleFor(x => x.Domicilio)
            .NotEmpty().WithMessage("El domicilio es obligatorio.")
            .MaximumLength(150).WithMessage("El domicilio no puede exceder 150 caracteres.");

        RuleFor(x => x.IdPais)
            .NotEmpty().WithMessage("El código de país es obligatorio.")
            .Length(3).WithMessage("El código de país debe tener exactamente 3 caracteres (norma ISO-3).");

        RuleFor(x => x.Telefono)
            .MaximumLength(25).When(x => !string.IsNullOrEmpty(x.Telefono))
            .WithMessage("El teléfono no puede exceder 25 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(150).WithMessage("El correo electrónico no puede exceder 150 caracteres.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(x => x.Observacion)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Observacion))
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres.");
    }
}
