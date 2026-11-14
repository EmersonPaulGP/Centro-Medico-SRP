namespace CentroMedico.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"No se encontró el registro de tipo '{entityName}' con identificador '{key}'.")
    {
    }
}
