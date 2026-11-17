namespace CentroMedico.Application.DTOs.Pagos;

/// <summary>
/// Parámetros de consulta y filtrado para arqueo y listado de pagos.
/// </summary>
public record PagoFiltroDto(
    int? IdPaciente = null,
    byte? IdConcepto = null,
    byte? IdEstado = null,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    int Pagina = 1,
    int RegistrosPorPagina = 20
);
