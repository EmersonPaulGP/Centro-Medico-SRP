using CentroMedico.Application.DTOs.Historia;
using CentroMedico.Application.Validators.Historia;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class CrearHistoriaValidatorTests
{
    private readonly CrearHistoriaValidator _validator;

    public CrearHistoriaValidatorTests()
    {
        _validator = new CrearHistoriaValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validador_DebeFallar_CuandoObservacionEstaVacia(string? observacion)
    {
        var dto = new CrearHistoriaDto(1, 1, null, "Gripe", observacion!);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Observacion);
    }

    [Theory]
    [InlineData("Reposo")]
    [InlineData("123456789")]
    public void Validador_DebeFallar_CuandoObservacionTieneMenosDe10Caracteres(string obsCorta)
    {
        var dto = new CrearHistoriaDto(1, 1, null, "Gripe", obsCorta);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Observacion)
              .WithErrorMessage("La observación clínica debe contener al menos 10 caracteres.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validador_DebeFallar_CuandoIdPacienteEsInvalido(int idPaciente)
    {
        var dto = new CrearHistoriaDto(idPaciente, 1, null, "Gripe", "Reposo y medicación por 48hs");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdPaciente)
              .WithErrorMessage("El identificador del paciente debe ser mayor a 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Validador_DebeFallar_CuandoIdMedicoEsInvalido(int idMedico)
    {
        var dto = new CrearHistoriaDto(1, idMedico, null, "Gripe", "Reposo y medicación por 48hs");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdMedico)
              .WithErrorMessage("El identificador del médico debe ser mayor a 0.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoIdTurnoEsMenorOIgualACero()
    {
        var dto = new CrearHistoriaDto(1, 1, 0, "Gripe", "Reposo y medicación por 48hs");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdTurno!.Value)
              .WithErrorMessage("El identificador del turno debe ser mayor a 0.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoDiagnosticoExcede250Caracteres()
    {
        var dto = new CrearHistoriaDto(1, 1, null, new string('D', 251), "Reposo y medicación por 48hs");
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Diagnostico)
              .WithErrorMessage("El diagnóstico no puede exceder los 250 caracteres.");
    }

    [Fact]
    public void Validador_DebeAceptar_DtoValido()
    {
        var dto = new CrearHistoriaDto(5, 1, 20, "Faringitis bacteriana", "Reposo 48hs y prescripción de Amoxicilina");
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
