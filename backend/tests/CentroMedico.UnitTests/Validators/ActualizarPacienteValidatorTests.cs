using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Validators.Pacientes;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class ActualizarPacienteValidatorTests
{
    private readonly ActualizarPacienteValidator _validator;

    public ActualizarPacienteValidatorTests()
    {
        _validator = new ActualizarPacienteValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoNombreEstaVacio(string? nombreInvalido)
    {
        var dto = new ActualizarPacienteDto(
            nombreInvalido!,
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            "ARG",
            "11223344",
            "carlos@paciente.com"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nombre)
              .WithErrorMessage("El nombre es obligatorio.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoApellidoEstaVacio(string? apellidoInvalido)
    {
        var dto = new ActualizarPacienteDto(
            "Carlos",
            apellidoInvalido!,
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            "ARG",
            "11223344",
            "carlos@paciente.com"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Apellido)
              .WithErrorMessage("El apellido es obligatorio.");
    }

    [Fact]
    public void Validador_DebeAceptar_CuandoDtoEsValido()
    {
        var dto = new ActualizarPacienteDto(
            "Carlos",
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Rivadavia 4567",
            "ARG",
            "11223344",
            "carlos.nuevo@paciente.com",
            "Nueva observación"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
