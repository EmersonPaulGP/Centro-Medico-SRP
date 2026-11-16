using CentroMedico.Application.DTOs.Historia;
using FluentValidation;

namespace CentroMedico.Application.Validators.Historia;

/// <summary>
/// Validador para el registro de una evolución en la historia clínica.
/// </summary>
public class CrearHistoriaValidator : AbstractValidator<CrearHistoriaDto>
{
    public CrearHistoriaValidator()
    {
        RuleFor(x => x.IdPaciente)
            .GreaterThan(0)
            .WithMessage("El identificador del paciente debe ser mayor a 0.");

        RuleFor(x => x.IdMedico)
            .GreaterThan(0)
            .WithMessage("El identificador del médico debe ser mayor a 0.");

        RuleFor(x => x.IdTurno!.Value)
            .GreaterThan(0)
            .When(x => x.IdTurno.HasValue)
            .WithMessage("El identificador del turno debe ser mayor a 0.");

        RuleFor(x => x.Observacion)
            .NotEmpty()
            .WithMessage("La observación clínica o evolución es obligatoria.")
            .MinimumLength(10)
            .WithMessage("La observación clínica debe contener al menos 10 caracteres.");

        RuleFor(x => x.Diagnostico)
            .MaximumLength(250)
            .When(x => !string.IsNullOrEmpty(x.Diagnostico))
            .WithMessage("El diagnóstico no puede exceder los 250 caracteres.");
    }
}
