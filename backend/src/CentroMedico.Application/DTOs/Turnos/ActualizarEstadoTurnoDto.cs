namespace CentroMedico.Application.DTOs.Turnos;

/// <summary>
/// DTO de entrada para actualizar el estado de un turno médico.
/// </summary>
public record ActualizarEstadoTurnoDto(
    short NuevoEstado,
    string? Observacion = null
);
