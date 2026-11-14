namespace CentroMedico.Application.DTOs.Medicos;

public record MedicoFiltroDto(
    string? Busqueda = null,
    int? IdEspecialidad = null,
    bool? SoloActivos = null,
    int Pagina = 1,
    int RegistrosPorPagina = 10
);
