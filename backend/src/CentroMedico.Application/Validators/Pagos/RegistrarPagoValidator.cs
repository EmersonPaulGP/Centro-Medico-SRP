using CentroMedico.Application.DTOs.Pagos;
using FluentValidation;

namespace CentroMedico.Application.Validators.Pagos;

/// <summary>
/// Validador FluentValidation para el registro de pagos y cobros.
/// </summary>
public class RegistrarPagoValidator : AbstractValidator<RegistrarPagoDto>
{
    public static readonly HashSet<string> MetodosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Efectivo",
        "Tarjeta de Débito",
        "Tarjeta de Debito",
        "Tarjeta de Crédito",
        "Tarjeta de Credito",
        "Transferencia Bancaria",
        "Transferencia",
        "Mercado Pago / Billetera Virtual",
        "Mercado Pago",
        "Billetera Virtual"
    };

    public RegistrarPagoValidator()
    {
        RuleFor(x => x.IdPaciente)
            .GreaterThan(0)
            .WithMessage("El identificador del paciente debe ser mayor a cero.");

        RuleFor(x => x.IdConcepto)
            .GreaterThan((byte)0)
            .WithMessage("El identificador del concepto de pago debe ser mayor a cero.");

        RuleFor(x => x.Monto)
            .GreaterThan(0)
            .WithMessage("El monto a cobrar debe ser estrictamente mayor a cero");

        RuleFor(x => x.IdTurno)
            .GreaterThan(0)
            .When(x => x.IdTurno.HasValue)
            .WithMessage("El identificador del turno debe ser mayor a cero.");

        RuleFor(x => x.MetodoPago)
            .NotEmpty()
            .WithMessage("El método de pago es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El método de pago no debe superar los 50 caracteres.")
            .Must(EsMetodoPagoPermitido)
            .WithMessage("El método de pago no es válido. Métodos permitidos: Efectivo, Tarjeta de Débito, Tarjeta de Crédito, Transferencia Bancaria, Mercado Pago / Billetera Virtual.");

        RuleFor(x => x.Observacion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Observacion))
            .WithMessage("La observación no debe superar los 1000 caracteres.");
    }

    private static bool EsMetodoPagoPermitido(string? metodo)
    {
        if (string.IsNullOrWhiteSpace(metodo))
            return false;

        return MetodosValidos.Contains(metodo.Trim());
    }
}
