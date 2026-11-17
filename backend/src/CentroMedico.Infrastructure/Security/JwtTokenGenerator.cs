using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CentroMedico.Infrastructure.Security;

/// <summary>
/// Generador y firmador de tokens criptográficos JWT para autenticación.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, int ExpiraEnSegundos) GenerarToken(Usuario usuario)
    {
        var secretKey = _configuration["Jwt:Key"]
            ?? "CentroMedicoSecretKeyForJwtAuthenticationTokenGeneration2026!";

        var issuer = _configuration["Jwt:Issuer"] ?? "CentroMedicoApi";
        var audience = _configuration["Jwt:Audience"] ?? "CentroMedicoClient";

        var expiryMinutesStr = _configuration["Jwt:ExpiryMinutes"];
        var expiryMinutes = int.TryParse(expiryMinutesStr, out var parsedMinutes) && parsedMinutes > 0
            ? parsedMinutes
            : 480; // 8 horas por defecto

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("rol", usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var fechaExpiracion = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = fechaExpiracion,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var expiraSegundos = (int)TimeSpan.FromMinutes(expiryMinutes).TotalSeconds;

        return (tokenString, expiraSegundos);
    }
}
