using CentroMedico.Application.DTOs.Turnos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Turnos;

/// <summary>
/// Validador para la solicitud de cambio de estado de un turno médico.
/// </summary>
public class ActualizarEstadoTurnoValidator : AbstractValidator<ActualizarEstadoTurnoDto>
{
    public ActualizarEstadoTurnoValidator()
    {
        RuleFor(x => x.NuevoEstado)
            .InclusiveBetween((short)0, (short)6)
            .WithMessage("El estado de turno ingresado no es válido (debe estar entre 0 y 6).");

        RuleFor(x => x.Observacion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Observacion))
            .WithMessage("La observación no puede exceder los 1000 caracteres.");
    }
}
