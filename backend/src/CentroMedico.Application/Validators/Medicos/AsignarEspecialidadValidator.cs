using CentroMedico.Application.DTOs.Medicos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Medicos;

/// <summary>
/// Validador para la asignación de especialidades médicas a un profesional.
/// </summary>
public class AsignarEspecialidadValidator : AbstractValidator<AsignarEspecialidadDto>
{
    public AsignarEspecialidadValidator()
    {
        RuleFor(x => x.IdEspecialidad)
            .GreaterThan(0).WithMessage("El identificador de la especialidad debe ser mayor a 0.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Descripcion))
            .WithMessage("La descripción de la especialidad asignada no puede superar 100 caracteres.");
    }
}
