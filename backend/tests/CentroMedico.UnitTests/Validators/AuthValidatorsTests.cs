using CentroMedico.Application.DTOs.Auth;
using CentroMedico.Application.Validators.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace CentroMedico.UnitTests.Validators;

public class AuthValidatorsTests
{
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RegistroUsuarioValidator _registroValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void LoginValidator_DebeFallar_CuandoEmailEstaVacio(string? email)
    {
        var dto = new LoginRequestDto(email!, "Password123!");
        var result = _loginValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("correo-no-valido")]
    [InlineData("usuario@")]
    [InlineData("@dominio.com")]
    public void LoginValidator_DebeFallar_CuandoEmailTieneFormatoInvalido(string emailInvalido)
    {
        var dto = new LoginRequestDto(emailInvalido, "Password123!");
        var result = _loginValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("El correo electrónico no tiene un formato válido.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void LoginValidator_DebeFallar_CuandoPasswordEstaVacia(string? password)
    {
        var dto = new LoginRequestDto("admin@test.com", password!);
        var result = _loginValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("La contraseña es obligatoria.");
    }

    [Fact]
    public void LoginValidator_DebePasar_CuandoEmailYPasswordSonValidos()
    {
        var dto = new LoginRequestDto("admin@centromedico.com", "Password123!");
        var result = _loginValidator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc")]
    public void RegistroValidator_DebeFallar_CuandoPasswordTieneMenosDe6Caracteres(string passCorta)
    {
        var dto = new RegistroUsuarioDto("nuevo@test.com", passCorta, "Juan", "Pérez", "Recepcionista");
        var result = _registroValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("La contraseña debe tener al menos 6 caracteres.");
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Invitado")]
    [InlineData("Paciente")]
    public void RegistroValidator_DebeFallar_CuandoRolNoEsPermitido(string rolInvalido)
    {
        var dto = new RegistroUsuarioDto("nuevo@test.com", "Password123!", "Juan", "Pérez", rolInvalido);
        var result = _registroValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Rol);
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("Medico")]
    [InlineData("Recepcionista")]
    public void RegistroValidator_DebePasar_CuandoRolEsPermitido(string rolValido)
    {
        var dto = new RegistroUsuarioDto("nuevo@test.com", "Password123!", "Juan", "Pérez", rolValido);
        var result = _registroValidator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Rol);
    }
}
