using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Pacientes;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class PacienteInfoServiceTests
{
    private readonly FakePacienteRepository _fakePacienteRepo;
    private readonly FakePacienteInfoRepository _fakePacienteInfoRepo;
    private readonly PacienteInfoService _service;

    public PacienteInfoServiceTests()
    {
        _fakePacienteRepo = new FakePacienteRepository();
        _fakePacienteInfoRepo = new FakePacienteInfoRepository();

        // Paciente base para pruebas
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 10,
            Dni = "12345678",
            Nombre = "Juan",
            Apellido = "Pérez",
            FNacimiento = new DateOnly(1985, 5, 20),
            Domicilio = "Av. Corrientes 500",
            IdPais = "ARG",
            Email = "juan.perez@test.com",
            Activo = true,
            FechaAlta = DateTime.UtcNow
        });

        var validator = new GuardarPacienteInfoValidator();
        _service = new PacienteInfoService(_fakePacienteInfoRepo, _fakePacienteRepo, validator);
    }

    [Fact]
    public async Task ObtenerPorPacienteIdAsync_CuandoPacienteNoExiste_DebeLanzarNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPorPacienteIdAsync(999));
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public async Task ObtenerPorPacienteIdAsync_RF02_CuandoPacienteExisteSinAntecedentes_DebeRetornarValoresPorDefecto()
    {
        // Arrange: Paciente 10 no posee PacienteInfo cargado previamente

        // Act
        var resultado = await _service.ObtenerPorPacienteIdAsync(10);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.IdPaciente);
        Assert.False(resultado.Diabetico);
        Assert.False(resultado.Implantes);
        Assert.Null(resultado.Alergias);
        Assert.Null(resultado.GrupoSanguineo);
        Assert.Null(resultado.FechaModificacion);
    }

    [Fact]
    public async Task ObtenerPorPacienteIdAsync_RF01_CuandoTieneAntecedentes_DebeRetornarDatosCompletos()
    {
        // Arrange
        var fecha = DateTime.UtcNow.AddDays(-1);
        _fakePacienteInfoRepo.Items.Add(new PacienteInfo
        {
            IdPaciente = 10,
            Diabetico = true,
            Implantes = false,
            Alergias = "Ibuprofeno",
            GrupoSanguineo = "A+",
            FechaModificacion = fecha
        });

        // Act
        var resultado = await _service.ObtenerPorPacienteIdAsync(10);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.IdPaciente);
        Assert.True(resultado.Diabetico);
        Assert.False(resultado.Implantes);
        Assert.Equal("Ibuprofeno", resultado.Alergias);
        Assert.Equal("A+", resultado.GrupoSanguineo);
        Assert.Equal(fecha, resultado.FechaModificacion);
    }

    [Fact]
    public async Task GuardarOActualizarAsync_CriterioAceptacion_RegistroExitoso_DebeVincularDatosAlPaciente()
    {
        // Gherkin:
        // Dado que existe el paciente con ID 10
        // Cuando se envía PUT "/api/v1/pacientes/10/antecedentes" con:
        //   | diabetico | implantes | alergias    | grupoSanguineo |
        //   | true      | false     | Penicilina  | O+             |
        // Entonces el código de respuesta debe ser 200 OK
        // Y los datos quedan vinculados al paciente 10

        // Arrange
        var dto = new GuardarPacienteInfoDto(
            Diabetico: true,
            Implantes: false,
            Alergias: "Penicilina",
            GrupoSanguineo: "O+"
        );

        // Act
        var resultado = await _service.GuardarOActualizarAsync(10, dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.IdPaciente);
        Assert.True(resultado.Diabetico);
        Assert.False(resultado.Implantes);
        Assert.Equal("Penicilina", resultado.Alergias);
        Assert.Equal("O+", resultado.GrupoSanguineo);
        Assert.NotNull(resultado.FechaModificacion);

        // Verificar persistencia en repositorio
        var guardado = _fakePacienteInfoRepo.Items.FirstOrDefault(i => i.IdPaciente == 10);
        Assert.NotNull(guardado);
        Assert.True(guardado!.Diabetico);
        Assert.False(guardado.Implantes);
        Assert.Equal("Penicilina", guardado.Alergias);
        Assert.Equal("O+", guardado.GrupoSanguineo);
    }

    [Fact]
    public async Task GuardarOActualizarAsync_CriterioAceptacion_GrupoSanguineoInvalido_DebeLanzarBusinessRuleException()
    {
        // Gherkin:
        // Dado que existe el paciente con ID 10
        // Cuando se envía grupoSanguineo "XYZ"
        // Entonces el código de respuesta debe ser 400 Bad Request
        // Y el error especifica "El grupo sanguíneo ingresado no es válido"

        // Arrange
        var dto = new GuardarPacienteInfoDto(
            Diabetico: true,
            Implantes: false,
            Alergias: "Ninguna",
            GrupoSanguineo: "XYZ"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.GuardarOActualizarAsync(10, dto));
        Assert.Equal("El grupo sanguíneo ingresado no es válido", ex.Message);
    }

    [Fact]
    public async Task GuardarOActualizarAsync_CuandoPacienteNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        var dto = new GuardarPacienteInfoDto(
            Diabetico: false,
            Implantes: false,
            Alergias: null,
            GrupoSanguineo: "B+"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.GuardarOActualizarAsync(999, dto));
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public async Task GuardarOActualizarAsync_Upsert_ActualizarAntecedentesExistentes_DebeModificarDatos()
    {
        // Arrange: antecedentes preexistentes
        _fakePacienteInfoRepo.Items.Add(new PacienteInfo
        {
            IdPaciente = 10,
            Diabetico = false,
            Implantes = false,
            Alergias = null,
            GrupoSanguineo = "A+",
            FechaModificacion = DateTime.UtcNow.AddMonths(-1)
        });

        var dto = new GuardarPacienteInfoDto(
            Diabetico: true,
            Implantes: true,
            Alergias: "Latex",
            GrupoSanguineo: "A-"
        );

        // Act
        var resultado = await _service.GuardarOActualizarAsync(10, dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.IdPaciente);
        Assert.True(resultado.Diabetico);
        Assert.True(resultado.Implantes);
        Assert.Equal("Latex", resultado.Alergias);
        Assert.Equal("A-", resultado.GrupoSanguineo);
        Assert.NotNull(resultado.FechaModificacion);

        Assert.Single(_fakePacienteInfoRepo.Items);
        var persistido = _fakePacienteInfoRepo.Items[0];
        Assert.True(persistido.Diabetico);
        Assert.True(persistido.Implantes);
        Assert.Equal("Latex", persistido.Alergias);
        Assert.Equal("A-", persistido.GrupoSanguineo);
    }
}

public class FakePacienteInfoRepository : IPacienteInfoRepository
{
    public List<PacienteInfo> Items { get; } = new();

    public Task<PacienteInfo?> ObtenerPorPacienteIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(i => i.IdPaciente == idPaciente);
        return Task.FromResult(item);
    }

    public Task GuardarOActualizarAsync(PacienteInfo info, CancellationToken cancellationToken = default)
    {
        var index = Items.FindIndex(i => i.IdPaciente == info.IdPaciente);
        if (index >= 0)
        {
            Items[index] = info;
        }
        else
        {
            Items.Add(info);
        }

        return Task.CompletedTask;
    }
}
