using CentroMedico.Application.DTOs.Turnos;
using CentroMedico.Application.Validators.Turnos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class CancelarTurnoValidatorTests
{
    private readonly CancelarTurnoValidator _validator;

    public CancelarTurnoValidatorTests()
    {
        _validator = new CancelarTurnoValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoMotivoEstaVacio(string? motivo)
    {
        var dto = new CancelarTurnoDto(motivo!);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MotivoCancelacion);
    }

    [Theory]
    [InlineData("Hola")]
    [InlineData("Viaj")]
    [InlineData("1234")]
    public void Validador_DebeFallar_CuandoMotivoTieneMenosDe5Caracteres(string motivoCorto)
    {
        var dto = new CancelarTurnoDto(motivoCorto);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MotivoCancelacion)
              .WithErrorMessage("El motivo de cancelación debe tener al menos 5 caracteres.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoMotivoExcede500Caracteres()
    {
        var dto = new CancelarTurnoDto(new string('M', 501));
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MotivoCancelacion)
              .WithErrorMessage("El motivo de cancelación no puede exceder los 500 caracteres.");
    }

    [Fact]
    public void Validador_DebeAceptar_MotivoValido()
    {
        var dto = new CancelarTurnoDto("Paciente no puede asistir por viaje de urgencia");
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
