namespace CentroMedico.Domain.Enums;

/// <summary>
/// Catálogo de estados posibles para una cita o turno médico.
/// </summary>
public enum EstadoTurnoEnum : short
{
    Pendiente = 0,
    Realizado = 1,
    Cancelado = 2,
    Rechazado = 3,
    Postergado = 4,
    Anulado = 5,
    Derivado = 6
}
