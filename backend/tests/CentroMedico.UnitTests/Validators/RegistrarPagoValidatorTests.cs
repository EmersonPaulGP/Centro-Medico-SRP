using CentroMedico.Application.DTOs.Pagos;
using CentroMedico.Application.Validators.Pagos;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class RegistrarPagoValidatorTests
{
    private readonly RegistrarPagoValidator _validator;

    public RegistrarPagoValidatorTests()
    {
        _validator = new RegistrarPagoValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-150.50)]
    public void Validador_DebeFallar_CuandoMontoEsCeroONegativo(decimal montoInvalido)
    {
        var dto = new RegistrarPagoDto(1, null, 1, montoInvalido, "Efectivo", null);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Monto)
              .WithErrorMessage("El monto a cobrar debe ser estrictamente mayor a cero");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100.0)]
    [InlineData(5000.50)]
    public void Validador_DebePasar_CuandoMontoEsMayorACero(decimal montoValido)
    {
        var dto = new RegistrarPagoDto(1, null, 1, montoValido, "Efectivo", null);
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Monto);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validador_DebeFallar_CuandoIdPacienteEsMenorOIgualACero(int idPacienteInvalido)
    {
        var dto = new RegistrarPagoDto(idPacienteInvalido, null, 1, 1500m, "Efectivo", null);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdPaciente)
              .WithErrorMessage("El identificador del paciente debe ser mayor a cero.");
    }

    [Theory]
    [InlineData((byte)0)]
    public void Validador_DebeFallar_CuandoIdConceptoEsCero(byte idConceptoInvalido)
    {
        var dto = new RegistrarPagoDto(1, null, idConceptoInvalido, 1500m, "Efectivo", null);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdConcepto)
              .WithErrorMessage("El identificador del concepto de pago debe ser mayor a cero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validador_DebeFallar_CuandoIdTurnoTieneValorMenorOIgualACero(int idTurnoInvalido)
    {
        var dto = new RegistrarPagoDto(1, idTurnoInvalido, 1, 1500m, "Efectivo", null);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdTurno)
              .WithErrorMessage("El identificador del turno debe ser mayor a cero.");
    }

    [Fact]
    public void Validador_DebePasar_CuandoIdTurnoEsNulo()
    {
        var dto = new RegistrarPagoDto(1, null, 1, 1500m, "Efectivo", null);
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.IdTurno);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validador_DebeFallar_CuandoMetodoPagoEstaVacio(string? metodoInvalido)
    {
        var dto = new RegistrarPagoDto(1, null, 1, 1500m, metodoInvalido, null);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MetodoPago);
    }

    [Theory]
    [InlineData("Bitcoins")]
    [InlineData("Cheque al portador")]
    [InlineData("Trueque")]
    public void Validador_DebeFallar_CuandoMetodoPagoNoEsPermitido(string metodoInvalido)
    {
        var dto = new RegistrarPagoDto(1, null, 1, 1500m, metodoInvalido, null);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MetodoPago)
              .WithErrorMessage("El método de pago no es válido. Métodos permitidos: Efectivo, Tarjeta de Débito, Tarjeta de Crédito, Transferencia Bancaria, Mercado Pago / Billetera Virtual.");
    }

    [Theory]
    [InlineData("Efectivo")]
    [InlineData("Tarjeta de Débito")]
    [InlineData("Tarjeta de Crédito")]
    [InlineData("Transferencia Bancaria")]
    [InlineData("Mercado Pago / Billetera Virtual")]
    [InlineData("Mercado Pago")]
    [InlineData("transferencia")]
    public void Validador_DebePasar_CuandoMetodoPagoEsPermitido(string metodoValido)
    {
        var dto = new RegistrarPagoDto(1, null, 1, 1500m, metodoValido, null);
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MetodoPago);
    }

    [Fact]
    public void Validador_DebeFallar_CuandoObservacionExcede1000Caracteres()
    {
        var obsLarga = new string('A', 1001);
        var dto = new RegistrarPagoDto(1, null, 1, 1500m, "Efectivo", obsLarga);
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Observacion)
              .WithErrorMessage("La observación no debe superar los 1000 caracteres.");
    }

    [Fact]
    public void Validador_DebePasar_CuandoTodosLosCamposSonValidos()
    {
        var dto = new RegistrarPagoDto(5, 10, 1, 5000.00m, "Efectivo", "Cobro de consulta médica general");
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
