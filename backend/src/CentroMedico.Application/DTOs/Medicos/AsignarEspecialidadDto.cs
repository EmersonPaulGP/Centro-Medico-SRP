namespace CentroMedico.Application.DTOs.Medicos;

public record AsignarEspecialidadDto(
    int IdEspecialidad,
    string? Descripcion = null
);
