using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CentroMedico.Domain.Constants;
using CentroMedico.Domain.Entities;
using CentroMedico.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CentroMedico.UnitTests.Security;

public class SecurityTests
{
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenGenerator _jwtGen;
    private readonly IConfiguration _config;

    public SecurityTests()
    {
        _hasher = new PasswordHasher();

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Key", "CentroMedicoSecretKeyForJwtAuthenticationTokenGeneration2026!"},
            {"Jwt:Issuer", "CentroMedicoApi"},
            {"Jwt:Audience", "CentroMedicoClient"},
            {"Jwt:ExpiryMinutes", "480"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtGen = new JwtTokenGenerator(_config);
    }

    [Fact]
    public void PasswordHasher_DebeHashearYVerificarPasswordCorrectamente()
    {
        // Arrange
        const string password = "PasswordFuerte123!";

        // Act
        var hash = _hasher.HashPassword(password);
        var valido = _hasher.VerifyPassword(password, hash);
        var invalido = _hasher.VerifyPassword("OtraPassword123!", hash);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
        Assert.True(valido);
        Assert.False(invalido);
    }

    [Fact]
    public void PasswordHasher_HashInvalidoOInexistente_DebeDevolverFalse()
    {
        // Act
        var resultado = _hasher.VerifyPassword("Cualquiera", "hash_invalido_no_bcrypt");

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void JwtTokenGenerator_DebeGenerarTokenValidoConClaimsCorrectos()
    {
        // Arrange
        var usuario = new Usuario
        {
            IdUsuario = 42,
            Email = "admin@centromedico.com",
            Nombre = "Administrador",
            Apellido = "General",
            Rol = Roles.Administrador,
            Activo = true
        };

        // Act
        var (token, expiraSegundos) = _jwtGen.GenerarToken(usuario);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(480 * 60, expiraSegundos);

        // Validar token criptográficamente
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "CentroMedicoApi",
            ValidAudience = "CentroMedicoClient",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("CentroMedicoSecretKeyForJwtAuthenticationTokenGeneration2026!"))
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        Assert.NotNull(validatedToken);

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

        Assert.Equal("42", idClaim);
        Assert.Equal("admin@centromedico.com", emailClaim);
        Assert.Equal(Roles.Administrador, roleClaim);
    }

    [Theory]
    [InlineData("Admin1234!", "$2a$11$f/yRaI40SdteiU50zk4BROl7z6/MW5elR08zhKf1mD1TrcJ4N2QGa")]
    [InlineData("Recep1234!", "$2a$11$eDToPbU9nxHxOSsuPXlq/O/zkKLixD8DIr9IjkWjRTbZCUPp2jYCO")]
    [InlineData("Medico1234!", "$2a$11$38RugPLN8efWJhgK1kbWxOTMi9AnZC.SGC3.TbZLPyN3PlR6xkBui")]
    public void PasswordHasher_SeedHashes_DebenSerValidos(string password, string hash)
    {
        var valido = _hasher.VerifyPassword(password, hash);
        Assert.True(valido);
    }
}
