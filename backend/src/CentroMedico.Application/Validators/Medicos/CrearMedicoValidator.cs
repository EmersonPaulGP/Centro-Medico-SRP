using CentroMedico.Application.DTOs.Medicos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Medicos;

public class CrearMedicoValidator : AbstractValidator<CrearMedicoDto>
{
    public CrearMedicoValidator()
    {
        RuleFor(x => x.Matricula)
            .NotEmpty().WithMessage("La matrícula profesional es obligatoria.")
            .MaximumLength(30).WithMessage("La matrícula no puede exceder 30 caracteres.");

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El documento de identidad (DNI) es obligatorio.")
            .MaximumLength(20).WithMessage("El DNI no puede exceder 20 caracteres.");

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

        RuleFor(x => x.Especialidades)
            .NotNull().WithMessage("Debe asignar al menos una especialidad al médico.")
            .Must(e => e != null && e.Count > 0).WithMessage("Debe asignar al menos una especialidad al médico.");

        RuleForEach(x => x.Especialidades)
            .ChildRules(esp =>
            {
                esp.RuleFor(e => e.IdEspecialidad)
                    .GreaterThan(0).WithMessage("El identificador de la especialidad debe ser mayor a 0.");
                esp.RuleFor(e => e.Descripcion)
                    .MaximumLength(100).When(e => !string.IsNullOrEmpty(e.Descripcion))
                    .WithMessage("La descripción de la especialidad asignada no puede superar 100 caracteres.");
            });
    }
}
