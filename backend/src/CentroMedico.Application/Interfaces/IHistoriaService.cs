using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Historia;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso para la historia clínica y notas de evolución médica.
/// </summary>
public interface IHistoriaService
{
    Task<HistoriaDetalleDto> RegistrarEvolucionAsync(CrearHistoriaDto dto, CancellationToken cancellationToken = default);
    Task<HistoriaDetalleDto> ObtenerPorIdAsync(int idHistoria, CancellationToken cancellationToken = default);
    Task<PagedResult<HistoriaDetalleDto>> ObtenerHistoriaPacienteAsync(int idPaciente, HistoriaFiltroDto filtro, CancellationToken cancellationToken = default);
}
