namespace CentroMedico.Application.DTOs.Dashboard;

/// <summary>
/// Total de ingresos recaudados en un mes del año calendario.
/// </summary>
public record RecaudacionMensualDto(
    int Anio,
    int Mes,
    string MesNombre,
    decimal TotalRecaudado
);
