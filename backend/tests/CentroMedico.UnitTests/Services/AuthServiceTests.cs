using CentroMedico.Application.DTOs.Auth;
using CentroMedico.Application.Interfaces;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Auth;
using CentroMedico.Domain.Constants;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class AuthServiceTests
{
    private readonly FakeUsuarioRepository _fakeRepo;
    private readonly FakePasswordHasher _fakeHasher;
    private readonly FakeJwtTokenGenerator _fakeJwtGen;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _fakeRepo = new FakeUsuarioRepository();
        _fakeHasher = new FakePasswordHasher();
        _fakeJwtGen = new FakeJwtTokenGenerator();

        var loginValidator = new LoginRequestValidator();
        var registroValidator = new RegistroUsuarioValidator();

        _service = new AuthService(
            _fakeRepo,
            _fakeHasher,
            _fakeJwtGen,
            loginValidator,
            registroValidator
        );

        // Seed: Usuario Recepcionista (Password correcta: "Recep1234!")
        _fakeRepo.Usuarios.Add(new Usuario
        {
            IdUsuario = 10,
            Email = "recepcion@centromedico.com",
            PasswordHash = _fakeHasher.HashPassword("Recep1234!"),
            Nombre = "Laura",
            Apellido = "Sánchez",
            Rol = Roles.Recepcionista,
            Activo = true
        });

        // Seed: Usuario Administrador (Password correcta: "Admin1234!")
        _fakeRepo.Usuarios.Add(new Usuario
        {
            IdUsuario = 1,
            Email = "admin@centromedico.com",
            PasswordHash = _fakeHasher.HashPassword("Admin1234!"),
            Nombre = "Carlos",
            Apellido = "Admin",
            Rol = Roles.Administrador,
            Activo = true
        });

        // Seed: Usuario Inactivo
        _fakeRepo.Usuarios.Add(new Usuario
        {
            IdUsuario = 99,
            Email = "inactivo@centromedico.com",
            PasswordHash = _fakeHasher.HashPassword("Inactivo123!"),
            Nombre = "Usuario",
            Apellido = "Inactivo",
            Rol = Roles.Medico,
            Activo = false
        });
    }

    [Fact]
    public async Task CriterioAceptacion_LoginExitosoConCredencialesCorrectas_DebeDevolverTokenYPerfil()
    {
        // Gherkin:
        // Escenario: Login exitoso con credenciales correctas
        //   Dado un usuario registrado con rol "Recepcionista"
        //   Cuando envía sus credenciales correctas a POST "/api/v1/auth/login"
        //   Entonces la API responde con código 200 OK
        //   Y devuelve el token JWT, tiempo de expiración y los datos básicos del usuario

        // Arrange
        var dto = new LoginRequestDto("recepcion@centromedico.com", "Recep1234!");

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Bearer", result.TokenType);
        Assert.True(result.ExpiraEnSegundos > 0);
        Assert.NotNull(result.Usuario);
        Assert.Equal(10, result.Usuario.IdUsuario);
        Assert.Equal("recepcion@centromedico.com", result.Usuario.Email);
        Assert.Equal("Laura", result.Usuario.Nombre);
        Assert.Equal("Sánchez", result.Usuario.Apellido);
        Assert.Equal(Roles.Recepcionista, result.Usuario.Rol);
    }

    [Fact]
    public async Task CriterioAceptacion_IntentoDeLoginConPasswordIncorrecta_DebeLanzarAuthenticationException()
    {
        // Gherkin:
        // Escenario: Intento de login con credenciales incorrectas
        //   Dado un usuario existente
        //   Cuando envía una contraseña equivocada
        //   Entonces la API responde con código 401 Unauthorized
        //   Y el mensaje de error indica "Credenciales inválidas"

        // Arrange
        var dto = new LoginRequestDto("recepcion@centromedico.com", "PasswordEquivocada123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => _service.LoginAsync(dto));
        Assert.Equal("Credenciales inválidas", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInexistente_DebeLanzarAuthenticationException()
    {
        // Arrange
        var dto = new LoginRequestDto("noexiste@centromedico.com", "Cualquiera123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => _service.LoginAsync(dto));
        Assert.Equal("Credenciales inválidas", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInactivo_DebeLanzarAuthenticationException()
    {
        // Arrange
        var dto = new LoginRequestDto("inactivo@centromedico.com", "Inactivo123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => _service.LoginAsync(dto));
        Assert.Equal("El usuario se encuentra inactivo", ex.Message);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_CuandoEsValido_DebeGuardarYDevolverPerfil()
    {
        // Arrange
        var dto = new RegistroUsuarioDto("dr.martin@centromedico.com", "Doctor123!", "Martín", "Palermo", "Medico");

        // Act
        var result = await _service.RegistrarUsuarioAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IdUsuario > 0);
        Assert.Equal("dr.martin@centromedico.com", result.Email);
        Assert.Equal(Roles.Medico, result.Rol);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_EmailDuplicado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new RegistroUsuarioDto("admin@centromedico.com", "AdminNuevo123!", "Pedro", "Gómez", "Administrador");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarUsuarioAsync(dto));
        Assert.Equal("Ya existe un usuario registrado con ese correo electrónico", ex.Message);
    }

    [Fact]
    public async Task ObtenerPerfilUsuarioAsync_UsuarioExistente_DebeDevolverPerfil()
    {
        // Act
        var perfil = await _service.ObtenerPerfilUsuarioAsync(1);

        // Assert
        Assert.NotNull(perfil);
        Assert.Equal(1, perfil.IdUsuario);
        Assert.Equal("admin@centromedico.com", perfil.Email);
        Assert.Equal(Roles.Administrador, perfil.Rol);
    }

    [Fact]
    public async Task ObtenerPerfilUsuarioAsync_UsuarioInexistente_DebeLanzarNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPerfilUsuarioAsync(9999));
        Assert.Contains("9999", ex.Message);
    }
}

public class FakeUsuarioRepository : IUsuarioRepository
{
    public List<Usuario> Usuarios { get; } = new();

    public Task<Usuario?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        var u = Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
        return Task.FromResult(u);
    }

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailNorm = email.Trim().ToLower();
        var u = Usuarios.FirstOrDefault(x => x.Email.ToLower() == emailNorm);
        return Task.FromResult(u);
    }

    public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailNorm = email.Trim().ToLower();
        var existe = Usuarios.Any(x => x.Email.ToLower() == emailNorm);
        return Task.FromResult(existe);
    }

    public Task<Usuario> AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        usuario.IdUsuario = Usuarios.Count > 0 ? Usuarios.Max(u => u.IdUsuario) + 1 : 1;
        Usuarios.Add(usuario);
        return Task.FromResult(usuario);
    }

    public Task<int> ContarAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Usuarios.Count);
    }
}

public class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string plainPassword) => $"hashed_{plainPassword}";

    public bool VerifyPassword(string plainPassword, string passwordHash)
        => passwordHash == $"hashed_{plainPassword}";
}

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public (string Token, int ExpiraEnSegundos) GenerarToken(Usuario usuario)
    {
        return ($"fake_jwt_token_for_{usuario.Email}_{usuario.Rol}", 28800);
    }
}
