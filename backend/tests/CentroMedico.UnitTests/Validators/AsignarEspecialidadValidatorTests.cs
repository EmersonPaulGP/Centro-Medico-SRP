using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Validators.Medicos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class AsignarEspecialidadValidatorTests
{
    private readonly AsignarEspecialidadValidator _validator;

    public AsignarEspecialidadValidatorTests()
    {
        _validator = new AsignarEspecialidadValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validador_DebeFallar_CuandoIdEspecialidadEsInvalido(int idInvalido)
    {
        var dto = new AsignarEspecialidadDto(idInvalido, "Descripción");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdEspecialidad)
              .WithErrorMessage("El identificador de la especialidad debe ser mayor a 0.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoDescripcionExcede100Caracteres()
    {
        var descripcionLarga = new string('A', 101);
        var dto = new AsignarEspecialidadDto(1, descripcionLarga);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Descripcion)
              .WithErrorMessage("La descripción de la especialidad asignada no puede superar 100 caracteres.");
    }

    [Fact]
    public void Validador_DebeAceptar_CuandoDtoEsValido()
    {
        var dto = new AsignarEspecialidadDto(2, "Especialista en traumatología deportiva");
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validador_DebeAceptar_ConDescripcionNula()
    {
        var dto = new AsignarEspecialidadDto(2, null);
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
