using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Validators.Medicos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class ActualizarMedicoValidatorTests
{
    private readonly ActualizarMedicoValidator _validator;

    public ActualizarMedicoValidatorTests()
    {
        _validator = new ActualizarMedicoValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoNombreEstaVacio(string? nombreInvalido)
    {
        var dto = new ActualizarMedicoDto(nombreInvalido!, "Pérez", "12345678", "medico@centromedico.com");
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
        var dto = new ActualizarMedicoDto("Juan", apellidoInvalido!, "12345678", "medico@centromedico.com");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Apellido)
              .WithErrorMessage("El apellido es obligatorio.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoEmailTieneFormatoInvalido()
    {
        var dto = new ActualizarMedicoDto("Juan", "Pérez", "12345678", "correo-invalido");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("El correo electrónico no tiene un formato válido.");
    }

    [Fact]
    public void Validador_DebeAceptar_CuandoDtoEsValido()
    {
        var dto = new ActualizarMedicoDto("Juan", "Pérez", "+54911223344", "juan.perez@centromedico.com");
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validador_DebeAceptar_ConTelefonoYEmailNulos()
    {
        var dto = new ActualizarMedicoDto("Juan", "Pérez", null, null);
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
