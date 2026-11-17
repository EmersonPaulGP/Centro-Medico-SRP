namespace CentroMedico.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando fallan las credenciales o la autenticación del usuario.
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message = "Credenciales inválidas")
        : base(message)
    {
    }
}
