using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para los antecedentes clínicos de los pacientes.
/// </summary>
public interface IPacienteInfoRepository
{
    Task<PacienteInfo?> ObtenerPorPacienteIdAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task GuardarOActualizarAsync(PacienteInfo info, CancellationToken cancellationToken = default);
}
