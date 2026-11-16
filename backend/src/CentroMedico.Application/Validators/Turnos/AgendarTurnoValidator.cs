using CentroMedico.Application.DTOs.Turnos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Turnos;

/// <summary>
/// Validador para la solicitud de agendamiento de turnos.
/// </summary>
public class AgendarTurnoValidator : AbstractValidator<AgendarTurnoDto>
{
    public AgendarTurnoValidator()
    {
        RuleFor(x => x.IdPaciente)
            .GreaterThan(0)
            .WithMessage("El identificador del paciente debe ser mayor a 0.");

        RuleFor(x => x.IdMedico)
            .GreaterThan(0)
            .WithMessage("El identificador del médico debe ser mayor a 0.");

        RuleFor(x => x.FechaTurno)
            .NotEmpty()
            .WithMessage("La fecha del turno es obligatoria.")
            .GreaterThan(new DateTime(2000, 1, 1))
            .WithMessage("La fecha del turno no es válida.");

        RuleFor(x => x.Observacion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Observacion))
            .WithMessage("La observación no puede exceder los 1000 caracteres.");
    }
}
