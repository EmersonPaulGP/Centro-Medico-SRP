namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO para la modificación del estado activo (baja lógica / reactivación).
/// </summary>
public record CambiarEstadoPacienteDto(bool Activo);
