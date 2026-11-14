using CentroMedico.Application.DTOs.Catalogos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Catalogos;

public class CrearEspecialidadValidator : AbstractValidator<CrearEspecialidadDto>
{
    public CrearEspecialidadValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la especialidad es obligatorio.")
            .MaximumLength(60)
            .WithMessage("El nombre de la especialidad no puede exceder 60 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Descripcion))
            .WithMessage("La descripción no puede exceder 200 caracteres.");
    }
}
