namespace CentroMedico.Application.DTOs.Catalogos;

public record CrearEspecialidadDto(
    string Nombre,
    string? Descripcion = null
);
