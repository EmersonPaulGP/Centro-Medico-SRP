using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Medicos;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato del servicio de aplicación para la gestión clínica del personal médico.
/// </summary>
public interface IMedicoService
{
    Task<MedicoDetalleDto> CrearAsync(CrearMedicoDto dto, CancellationToken cancellationToken = default);
    Task<MedicoDetalleDto> ActualizarAsync(int idMedico, ActualizarMedicoDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<MedicoDetalleDto>> ListarAsync(MedicoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<MedicoDetalleDto> ObtenerPorIdAsync(int idMedico, CancellationToken cancellationToken = default);
    Task CambiarEstadoActivoAsync(int idMedico, bool activo, CancellationToken cancellationToken = default);
    Task<MedicoDetalleDto> ActualizarEspecialidadesAsync(int idMedico, List<AsignarEspecialidadDto> especialidades, CancellationToken cancellationToken = default);
}
