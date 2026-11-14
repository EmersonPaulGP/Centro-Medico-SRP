namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO con filtros de búsqueda y parámetros de paginación para pacientes.
/// </summary>
public record PacienteFiltroDto(
    string? Busqueda = null,
    string? IdPais = null,
    bool? SoloActivos = null,
    int Pagina = 1,
    int RegistrosPorPagina = 10
);
