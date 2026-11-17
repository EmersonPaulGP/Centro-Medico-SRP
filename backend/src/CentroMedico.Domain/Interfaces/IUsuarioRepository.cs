using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la gestión de usuarios del sistema.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario> AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<int> ContarAsync(CancellationToken cancellationToken = default);
}
