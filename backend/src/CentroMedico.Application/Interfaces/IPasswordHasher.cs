namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato para el hasheo criptográfico y verificación de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string passwordHash);
}
