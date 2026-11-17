using CentroMedico.Application.Interfaces;

namespace CentroMedico.Infrastructure.Security;

/// <summary>
/// Implementación de seguridad para hasheo de contraseñas utilizando algoritmo BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 11);
    }

    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
