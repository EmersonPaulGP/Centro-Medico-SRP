namespace CentroMedico.Application.DTOs.Historia;

/// <summary>
/// DTO con filtros de búsqueda y paginación para la historia clínica del paciente.
/// </summary>
public record HistoriaFiltroDto(
    int? IdMedico = null,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    string? TerminoBusqueda = null,
    int Pagina = 1,
    int RegistrosPorPagina = 15
);
