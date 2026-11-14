namespace CentroMedico.Application.DTOs.Medicos;

public record EspecialidadAsignadaDto(
    int IdEspecialidad,
    string Nombre,
    string? Descripcion = null
);
