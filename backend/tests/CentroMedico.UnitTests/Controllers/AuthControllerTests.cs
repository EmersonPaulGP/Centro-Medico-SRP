using System.Security.Claims;
using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Auth;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly FakeAuthService _fakeService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _fakeService = new FakeAuthService();
        _controller = new AuthController(_fakeService);
    }

    [Fact]
    public async Task Login_DebeRetornar200Ok_ConTokenYPerfil()
    {
        // Arrange
        var dto = new LoginRequestDto("recepcion@centromedico.com", "Password123!");

        // Act
        var actionResult = await _controller.Login(dto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("jwt_token_test", apiResponse.Data!.Token);
        Assert.Equal(Roles.Recepcionista, apiResponse.Data.Usuario.Rol);
    }

    [Fact]
    public async Task Registro_DebeRetornar201Created_ConUsuarioPerfil()
    {
        // Arrange
        var dto = new RegistroUsuarioDto("nuevo@test.com", "Password123!", "Juan", "Pérez", Roles.Medico);

        // Act
        var actionResult = await _controller.Registro(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<UsuarioPerfilDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("nuevo@test.com", apiResponse.Data!.Email);
    }

    [Fact]
    public async Task ObtenerPerfilActual_UsuarioAutenticado_DebeRetornar200Ok()
    {
        // Arrange
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim(ClaimTypes.Email, "recepcion@centromedico.com"),
            new Claim(ClaimTypes.Role, Roles.Recepcionista)
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // Act
        var actionResult = await _controller.ObtenerPerfilActual(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<UsuarioPerfilDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(10, apiResponse.Data!.IdUsuario);
        Assert.Equal("recepcion@centromedico.com", apiResponse.Data.Email);
    }

    [Fact]
    public async Task ObtenerPerfilActual_SinClaimDeIdentidad_DebeRetornar401Unauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act
        var actionResult = await _controller.ObtenerPerfilActual(CancellationToken.None);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }
}

public class FakeAuthService : IAuthService
{
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var perfil = new UsuarioPerfilDto(10, dto.Email, "Laura", "Sánchez", Roles.Recepcionista);
        var res = new LoginResponseDto("jwt_token_test", "Bearer", 28800, perfil);
        return Task.FromResult(res);
    }

    public Task<UsuarioPerfilDto> RegistrarUsuarioAsync(RegistroUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var perfil = new UsuarioPerfilDto(5, dto.Email, dto.Nombre, dto.Apellido, dto.Rol);
        return Task.FromResult(perfil);
    }

    public Task<UsuarioPerfilDto> ObtenerPerfilUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        var perfil = new UsuarioPerfilDto(idUsuario, "recepcion@centromedico.com", "Laura", "Sánchez", Roles.Recepcionista);
        return Task.FromResult(perfil);
    }
}
