namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa a un usuario del sistema para autenticación y autorización.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Nombre completo del usuario concatenando apellido y nombre.
    /// </summary>
    public string NombreCompleto => $"{Apellido}, {Nombre}".Trim(',', ' ');
}
