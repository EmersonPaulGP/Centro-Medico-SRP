using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Turnos;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso para la gestión y agendamiento de turnos médicos.
/// </summary>
public interface ITurnoService
{
    Task<TurnoDetalleDto> AgendarAsync(AgendarTurnoDto dto, CancellationToken cancellationToken = default);
    Task<TurnoDetalleDto> ObtenerPorIdAsync(int idTurno, CancellationToken cancellationToken = default);
    Task<PagedResult<TurnoDetalleDto>> ObtenerAgendaAsync(TurnoFiltroAgendaDto filtro, CancellationToken cancellationToken = default);
    Task<TurnoDetalleDto> CambiarEstadoAsync(int idTurno, ActualizarEstadoTurnoDto dto, CancellationToken cancellationToken = default);
    Task<TurnoDetalleDto> CancelarTurnoAsync(int idTurno, CancelarTurnoDto dto, CancellationToken cancellationToken = default);
}
