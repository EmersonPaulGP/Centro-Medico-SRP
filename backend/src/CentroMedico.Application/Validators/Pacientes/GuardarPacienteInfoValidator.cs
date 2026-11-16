using CentroMedico.Application.DTOs.Pacientes;
using FluentValidation;

namespace CentroMedico.Application.Validators.Pacientes;

/// <summary>
/// Validador de reglas de entrada y formato para los antecedentes clínicos del paciente.
/// </summary>
public class GuardarPacienteInfoValidator : AbstractValidator<GuardarPacienteInfoDto>
{
    private static readonly HashSet<string> GruposSanguineosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
    };

    public GuardarPacienteInfoValidator()
    {
        RuleFor(x => x.Alergias)
            .MaximumLength(500)
            .WithMessage("Las alergias no pueden exceder los 500 caracteres.");

        RuleFor(x => x.GrupoSanguineo)
            .Must(g => string.IsNullOrWhiteSpace(g) || GruposSanguineosValidos.Contains(g.Trim()))
            .WithMessage("El grupo sanguíneo ingresado no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.GrupoSanguineo));
    }
}
