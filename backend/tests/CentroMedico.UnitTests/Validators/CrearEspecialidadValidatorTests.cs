using CentroMedico.Application.DTOs.Catalogos;
using CentroMedico.Application.Validators.Catalogos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class CrearEspecialidadValidatorTests
{
    private readonly CrearEspecialidadValidator _validator;

    public CrearEspecialidadValidatorTests()
    {
        _validator = new CrearEspecialidadValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoNombreEstaVacio(string? nombreInvalido)
    {
        // Arrange
        var dto = new CrearEspecialidadDto(nombreInvalido!, "Descripción de prueba");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre)
              .WithErrorMessage("El nombre de la especialidad es obligatorio.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoNombreExcede60Caracteres()
    {
        // Arrange
        var nombreLargo = new string('A', 61);
        var dto = new CrearEspecialidadDto(nombreLargo, null);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre)
              .WithErrorMessage("El nombre de la especialidad no puede exceder 60 caracteres.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoDescripcionExcede200Caracteres()
    {
        // Arrange
        var descripcionLarga = new string('D', 201);
        var dto = new CrearEspecialidadDto("Cardiología", descripcionLarga);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Descripcion)
              .WithErrorMessage("La descripción no puede exceder 200 caracteres.");
    }

    [Fact]
    public void Validador_DebeAceptar_CuandoDtoEsValido()
    {
        // Arrange
        var dto = new CrearEspecialidadDto("Cardiología", "Especialidad dedicada a las enfermedades cardiovasculares.");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
