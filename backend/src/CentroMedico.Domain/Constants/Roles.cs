namespace CentroMedico.Domain.Constants;

/// <summary>
/// Roles de usuario predefinidos para el control de acceso basado en roles (RBAC).
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Medico = "Medico";
    public const string Recepcionista = "Recepcionista";

    public static readonly IReadOnlyList<string> Todos = new[]
    {
        Administrador,
        Medico,
        Recepcionista
    };

    public static bool EsRolValido(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
            return false;

        return Todos.Contains(rol.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
