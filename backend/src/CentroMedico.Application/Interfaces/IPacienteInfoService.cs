using CentroMedico.Application.DTOs.Pacientes;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de servicio de aplicación para la gestión de antecedentes clínicos del paciente.
/// </summary>
public interface IPacienteInfoService
{
    Task<PacienteInfoDto> ObtenerPorPacienteIdAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task<PacienteInfoDto> GuardarOActualizarAsync(int idPaciente, GuardarPacienteInfoDto dto, CancellationToken cancellationToken = default);
}
