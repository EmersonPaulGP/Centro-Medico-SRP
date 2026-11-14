namespace CentroMedico.Application.DTOs.Medicos;

public record CrearMedicoDto(
    string Matricula,
    string Dni,
    string Nombre,
    string Apellido,
    string? Telefono = null,
    string? Email = null,
    List<AsignarEspecialidadDto>? Especialidades = null
);
