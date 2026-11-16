namespace CentroMedico.Application.DTOs.Turnos;

/// <summary>
/// DTO de entrada para cancelar un turno médico con motivo justificado.
/// </summary>
public record CancelarTurnoDto(
    string MotivoCancelacion
);
