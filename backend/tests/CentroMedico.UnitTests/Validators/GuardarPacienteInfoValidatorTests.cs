using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Validators.Pacientes;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class GuardarPacienteInfoValidatorTests
{
    private readonly GuardarPacienteInfoValidator _validator;

    public GuardarPacienteInfoValidatorTests()
    {
        _validator = new GuardarPacienteInfoValidator();
    }

    [Theory]
    [InlineData("A+")]
    [InlineData("A-")]
    [InlineData("B+")]
    [InlineData("B-")]
    [InlineData("AB+")]
    [InlineData("AB-")]
    [InlineData("O+")]
    [InlineData("O-")]
    [InlineData("a+")]
    [InlineData("o-")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validador_DebeAceptar_GrupoSanguineoValidoONulo(string? grupo)
    {
        var dto = new GuardarPacienteInfoDto(
            Diabetico: true,
            Implantes: false,
            Alergias: "Polen",
            GrupoSanguineo: grupo
        );

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.GrupoSanguineo);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("C+")]
    [InlineData("RH+")]
    [InlineData("123")]
    public void Validador_DebeFallar_CuandoGrupoSanguineoEsInvalido(string grupoInvalido)
    {
        var dto = new GuardarPacienteInfoDto(
            Diabetico: false,
            Implantes: false,
            Alergias: null,
            GrupoSanguineo: grupoInvalido
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.GrupoSanguineo)
              .WithErrorMessage("El grupo sanguíneo ingresado no es válido");
    }

    [Fact]
    public void Validador_DebeFallar_CuandoAlergiasExcede500Caracteres()
    {
        var dto = new GuardarPacienteInfoDto(
            Diabetico: false,
            Implantes: false,
            Alergias: new string('A', 501),
            GrupoSanguineo: "O+"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Alergias)
              .WithErrorMessage("Las alergias no pueden exceder los 500 caracteres.");
    }

    [Fact]
    public void Validador_DebeAceptar_DatosCompletosYValidos()
    {
        var dto = new GuardarPacienteInfoDto(
            Diabetico: true,
            Implantes: true,
            Alergias: "Penicilina, Sulfas",
            GrupoSanguineo: "O+"
        );

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
