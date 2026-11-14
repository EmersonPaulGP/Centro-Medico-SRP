namespace CentroMedico.Application.DTOs.Medicos;

public record MedicoDetalleDto(
    int IdMedico,
    string Matricula,
    string Dni,
    string Nombre,
    string Apellido,
    string? Telefono,
    string? Email,
    bool Activo,
    DateTime FechaAlta,
    DateTime? FechaModificacion,
    IReadOnlyList<EspecialidadAsignadaDto> Especialidades
);
