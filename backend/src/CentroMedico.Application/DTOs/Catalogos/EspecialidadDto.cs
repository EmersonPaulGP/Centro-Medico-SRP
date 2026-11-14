namespace CentroMedico.Application.DTOs.Catalogos;

public record EspecialidadDto(
    int IdEspecialidad,
    string Nombre,
    string? Descripcion,
    bool Activo
);
