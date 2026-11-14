using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Validators.Medicos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class CrearMedicoValidatorTests
{
    private readonly CrearMedicoValidator _validator;

    public CrearMedicoValidatorTests()
    {
        _validator = new CrearMedicoValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoMatriculaEstaVacia(string? matriculaInvalida)
    {
        var dto = new CrearMedicoDto(
            matriculaInvalida!,
            "12345678",
            "Carlos",
            "Gómez",
            "11223344",
            "carlos@medico.com",
            new List<AsignarEspecialidadDto> { new(1) }
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Matricula)
              .WithErrorMessage("La matrícula profesional es obligatoria.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoDniEstaVacio(string? dniInvalido)
    {
        var dto = new CrearMedicoDto(
            "MN-12345",
            dniInvalido!,
            "Carlos",
            "Gómez",
            "11223344",
            "carlos@medico.com",
            new List<AsignarEspecialidadDto> { new(1) }
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Dni)
              .WithErrorMessage("El documento de identidad (DNI) es obligatorio.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoNoTieneEspecialidades()
    {
        var dto = new CrearMedicoDto(
            "MN-12345",
            "12345678",
            "Carlos",
            "Gómez",
            "11223344",
            "carlos@medico.com",
            new List<AsignarEspecialidadDto>()
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Especialidades)
              .WithErrorMessage("Debe asignar al menos una especialidad al médico.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoEspecialidadTieneIdCeroONegativo()
    {
        var dto = new CrearMedicoDto(
            "MN-12345",
            "12345678",
            "Carlos",
            "Gómez",
            "11223344",
            "carlos@medico.com",
            new List<AsignarEspecialidadDto> { new(0) }
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Especialidades[0].IdEspecialidad")
              .WithErrorMessage("El identificador de la especialidad debe ser mayor a 0.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoEmailTieneFormatoInvalido()
    {
        var dto = new CrearMedicoDto(
            "MN-12345",
            "12345678",
            "Carlos",
            "Gómez",
            "11223344",
            "correo-invalido-sin-arroba",
            new List<AsignarEspecialidadDto> { new(1) }
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("El correo electrónico no tiene un formato válido.");
    }

    [Fact]
    public void Validador_DebeAceptar_CuandoDtoEsValido()
    {
        var dto = new CrearMedicoDto(
            "MN-12345",
            "12345678",
            "Carlos",
            "Gómez",
            "11223344",
            "carlos.gomez@centromedico.com",
            new List<AsignarEspecialidadDto> { new(1, "Especialista Principal") }
        );

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
