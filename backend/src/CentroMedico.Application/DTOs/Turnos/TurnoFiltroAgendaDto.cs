namespace CentroMedico.Application.DTOs.Turnos;

/// <summary>
/// Parámetros de consulta y paginación para la agenda de turnos médicos.
/// </summary>
public record TurnoFiltroAgendaDto(
    int? IdMedico = null,
    int? IdPaciente = null,
    DateTime? FechaInicio = null,
    DateTime? FechaFin = null,
    short? IdEstado = null,
    int Pagina = 1,
    int RegistrosPorPagina = 20
);
