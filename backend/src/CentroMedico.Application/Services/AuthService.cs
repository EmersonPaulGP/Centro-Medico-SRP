using CentroMedico.Application.DTOs.Auth;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Constants;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para autenticación, registro y sesiones de usuario.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RegistroUsuarioDto> _registroValidator;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RegistroUsuarioDto> registroValidator)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginValidator = loginValidator;
        _registroValidator = registroValidator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(emailNormalizado, cancellationToken);

        if (usuario == null)
        {
            throw new AuthenticationException("Credenciales inválidas");
        }

        if (!usuario.Activo)
        {
            throw new AuthenticationException("El usuario se encuentra inactivo");
        }

        var passwordValido = _passwordHasher.VerifyPassword(dto.Password, usuario.PasswordHash);
        if (!passwordValido)
        {
            throw new AuthenticationException("Credenciales inválidas");
        }

        var (token, expiraSegundos) = _jwtTokenGenerator.GenerarToken(usuario);
        var perfil = new UsuarioPerfilDto(
            usuario.IdUsuario,
            usuario.Email,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Rol
        );

        return new LoginResponseDto(token, "Bearer", expiraSegundos, perfil);
    }

    public async Task<UsuarioPerfilDto> RegistrarUsuarioAsync(RegistroUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registroValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();
        var existeEmail = await _usuarioRepository.ExisteEmailAsync(emailNormalizado, cancellationToken);
        if (existeEmail)
        {
            throw new BusinessRuleException("Ya existe un usuario registrado con ese correo electrónico");
        }

        var rolNormalizado = Roles.Todos.First(r => r.Equals(dto.Rol.Trim(), StringComparison.OrdinalIgnoreCase));
        var hash = _passwordHasher.HashPassword(dto.Password);

        var usuario = new Usuario
        {
            Email = emailNormalizado,
            PasswordHash = hash,
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Rol = rolNormalizado,
            Activo = true,
            FechaAlta = DateTime.UtcNow
        };

        var creado = await _usuarioRepository.AgregarAsync(usuario, cancellationToken);

        return new UsuarioPerfilDto(
            creado.IdUsuario,
            creado.Email,
            creado.Nombre,
            creado.Apellido,
            creado.Rol
        );
    }

    public async Task<UsuarioPerfilDto> ObtenerPerfilUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario, cancellationToken);
        if (usuario == null)
        {
            throw new NotFoundException($"No se encontró ningún usuario con el ID '{idUsuario}'");
        }

        return new UsuarioPerfilDto(
            usuario.IdUsuario,
            usuario.Email,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Rol
        );
    }
}
