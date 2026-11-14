namespace CentroMedico.Domain.Exceptions;

public class BusinessRuleException : Exception
{
    public string? Code { get; }

    public BusinessRuleException(string message) : base(message)
    {
    }

    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
