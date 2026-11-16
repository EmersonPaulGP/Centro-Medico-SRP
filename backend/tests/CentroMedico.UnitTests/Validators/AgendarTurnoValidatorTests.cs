using CentroMedico.Application.DTOs.Turnos;
using CentroMedico.Application.Validators.Turnos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class AgendarTurnoValidatorTests
{
    private readonly AgendarTurnoValidator _validator;

    public AgendarTurnoValidatorTests()
    {
        _validator = new AgendarTurnoValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validador_DebeFallar_CuandoIdPacienteEsInvalido(int idPaciente)
    {
        var dto = new AgendarTurnoDto(idPaciente, 1, DateTime.UtcNow.AddDays(1));
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdPaciente)
              .WithErrorMessage("El identificador del paciente debe ser mayor a 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validador_DebeFallar_CuandoIdMedicoEsInvalido(int idMedico)
    {
        var dto = new AgendarTurnoDto(1, idMedico, DateTime.UtcNow.AddDays(1));
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdMedico)
              .WithErrorMessage("El identificador del médico debe ser mayor a 0.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoFechaTurnoEsPasadaRemota()
    {
        var dto = new AgendarTurnoDto(1, 1, new DateTime(1990, 1, 1));
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FechaTurno)
              .WithErrorMessage("La fecha del turno no es válida.");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoObservacionExcede1000Caracteres()
    {
        var dto = new AgendarTurnoDto(1, 1, DateTime.UtcNow.AddDays(1), new string('X', 1001));
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Observacion)
              .WithErrorMessage("La observación no puede exceder los 1000 caracteres.");
    }

    [Fact]
    public void Validador_DebeAceptar_DtoValido()
    {
        var dto = new AgendarTurnoDto(5, 1, DateTime.UtcNow.AddDays(2), "Primera consulta");
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
