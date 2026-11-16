using CentroMedico.Application.DTOs.Turnos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Turnos;

/// <summary>
/// Validador para la solicitud de cancelación de un turno médico.
/// </summary>
public class CancelarTurnoValidator : AbstractValidator<CancelarTurnoDto>
{
    public CancelarTurnoValidator()
    {
        RuleFor(x => x.MotivoCancelacion)
            .NotEmpty()
            .WithMessage("El motivo de cancelación es obligatorio.")
            .MinimumLength(5)
            .WithMessage("El motivo de cancelación debe tener al menos 5 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo de cancelación no puede exceder los 500 caracteres.");
    }
}
