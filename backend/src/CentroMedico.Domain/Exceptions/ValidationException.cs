namespace CentroMedico.Domain.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors, string message = "Uno o más errores de validación ocurrieron.")
        : base(message)
    {
        Errors = errors;
    }
}
