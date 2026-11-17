using CentroMedico.Domain.Entities;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato para la generación y firma de tokens de autenticación JWT.
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, int ExpiraEnSegundos) GenerarToken(Usuario usuario);
}
