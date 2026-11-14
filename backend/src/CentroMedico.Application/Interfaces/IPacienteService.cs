using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pacientes;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato del servicio de aplicación para la gestión clínica y administrativa de pacientes.
/// </summary>
public interface IPacienteService
{
    Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default);
    Task<PacienteDto> ActualizarAsync(int idPaciente, ActualizarPacienteDto dto, CancellationToken cancellationToken = default);
    Task<PacienteDto> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task<PacienteDto> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<PagedResult<PacienteDto>> ListarAsync(PacienteFiltroDto filtro, CancellationToken cancellationToken = default);
    Task BajaLogicaAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task CambiarEstadoActivoAsync(int idPaciente, bool activo, CancellationToken cancellationToken = default);
}
