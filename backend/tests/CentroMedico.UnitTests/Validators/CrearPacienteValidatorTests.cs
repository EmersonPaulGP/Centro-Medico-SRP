using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Validators.Pacientes;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class CrearPacienteValidatorTests
{
    private readonly CrearPacienteValidator _validator;

    public CrearPacienteValidatorTests()
    {
        _validator = new CrearPacienteValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoDniEstaVacio(string? dniInvalido)
    {
        var dto = new CrearPacienteDto(
            dniInvalido!,
            "Carlos",
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            "ARG",
            "11223344",
            "carlos@paciente.com"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Dni)
              .WithErrorMessage("El documento de identidad (DNI) es obligatorio.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoDniExcede20Caracteres()
    {
        var dto = new CrearPacienteDto(
            new string('1', 21),
            "Carlos",
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            "ARG",
            "11223344",
            "carlos@paciente.com"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Dni)
              .WithErrorMessage("El DNI no puede exceder 20 caracteres.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoNombreEstaVacio(string? nombreInvalido)
    {
        var dto = new CrearPacienteDto(
            "12345678",
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
        var dto = new CrearPacienteDto(
            "12345678",
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
    public void Validador_DebeFallar_CuandoFechaNacimientoEsFutura()
    {
        var fechaFutura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var dto = new CrearPacienteDto(
            "12345678",
            "Carlos",
            "Gómez",
            fechaFutura,
            "Av. Corrientes 1234",
            "ARG",
            "11223344",
            "carlos@paciente.com"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FNacimiento)
              .WithErrorMessage("La fecha de nacimiento debe ser una fecha pasada.");
    }

    [Theory]
    [InlineData("AR")]
    [InlineData("ARGT")]
    [InlineData("")]
    public void Validador_DebeFallar_CuandoIdPaisNoTiene3Caracteres(string idPaisInvalido)
    {
        var dto = new CrearPacienteDto(
            "12345678",
            "Carlos",
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            idPaisInvalido,
            "11223344",
            "carlos@paciente.com"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdPais);
    }

    [Theory]
    [InlineData("correo-invalido")]
    [InlineData("correo@")]
    [InlineData("@dominio.com")]
    public void Validador_DebeFallar_CuandoEmailEsInvalido(string emailInvalido)
    {
        var dto = new CrearPacienteDto(
            "12345678",
            "Carlos",
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            "ARG",
            "11223344",
            emailInvalido
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("El correo electrónico no tiene un formato válido.");
    }

    [Fact]
    public void Validador_DebeAceptar_CuandoDtoEsValido()
    {
        var dto = new CrearPacienteDto(
            "12345678",
            "Carlos",
            "Gómez",
            new DateOnly(1990, 5, 15),
            "Av. Corrientes 1234",
            "ARG",
            "+54911223344",
            "carlos.gomez@gmail.com",
            "Observación opcional"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
