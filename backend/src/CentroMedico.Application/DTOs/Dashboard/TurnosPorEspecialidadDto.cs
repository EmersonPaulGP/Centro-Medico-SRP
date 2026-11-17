namespace CentroMedico.Application.DTOs.Dashboard;

/// <summary>
/// Distribución y porcentaje de turnos demandados por cada especialidad médica.
/// </summary>
public record TurnosPorEspecialidadDto(
    string Especialidad,
    int TotalTurnos,
    decimal Porcentaje
);
