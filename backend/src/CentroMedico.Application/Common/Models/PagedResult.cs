namespace CentroMedico.Application.Common.Models;

/// <summary>
/// Envoltorio estándar para resultados de consultas paginadas.
/// </summary>
/// <typeparam name="T">Tipo de dato de los elementos paginados.</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; }
    public int RegistrosPorPagina { get; set; }
    public int TotalPaginas => RegistrosPorPagina > 0 ? (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina) : 0;
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

    public PagedResult()
    {
    }

    public PagedResult(IReadOnlyList<T> items, int totalRegistros, int paginaActual, int registrosPorPagina)
    {
        Items = items;
        TotalRegistros = totalRegistros;
        PaginaActual = paginaActual;
        RegistrosPorPagina = registrosPorPagina;
    }
}
