using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Pacientes;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class PacienteServiceTests
{
    private readonly FakePacienteRepository _fakePacienteRepo;
    private readonly FakeCatalogoRepository _fakeCatalogoRepo;
    private readonly PacienteService _service;

    public PacienteServiceTests()
    {
        _fakePacienteRepo = new FakePacienteRepository();
        _fakeCatalogoRepo = new FakeCatalogoRepository();

        _fakeCatalogoRepo.Paises.Add(new Pais
        {
            IdPais = "ARG",
            Nombre = "Argentina"
        });

        _fakeCatalogoRepo.Paises.Add(new Pais
        {
            IdPais = "CHL",
            Nombre = "Chile"
        });

        var crearValidator = new CrearPacienteValidator();
        var actualizarValidator = new ActualizarPacienteValidator();

        _service = new PacienteService(_fakePacienteRepo, _fakeCatalogoRepo, crearValidator, actualizarValidator);
    }

    [Fact]
    public async Task CrearAsync_CriterioAceptacion_RegistroExitoso_DebeCrearPacienteActivo()
    {
        // Gherkin:
        // Dado que el DNI "45678912" no está registrado
        // Y el país "ARG" existe en la base de datos
        // Cuando se envía la solicitud de registro con datos válidos
        // Entonces el código de respuesta debe ser 201 Created (simulado a nivel servicio)
        // Y el paciente queda creado con estado activo = true

        // Arrange
        var dto = new CrearPacienteDto(
            "45678912",
            "Martín",
            "Palermo",
            new DateOnly(1980, 11, 7),
            "Brandsen 805",
            "ARG",
            "+541199887766",
            "martin.palermo@boca.com",
            "Paciente derivado"
        );

        // Act
        var result = await _service.CrearAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IdPaciente > 0);
        Assert.Equal("45678912", result.Dni);
        Assert.Equal("Martín", result.Nombre);
        Assert.Equal("Palermo", result.Apellido);
        Assert.Equal("ARG", result.IdPais);
        Assert.Equal("Argentina", result.PaisNombre);
        Assert.True(result.Activo);
        Assert.True(result.Edad > 40);
    }

    [Fact]
    public async Task CrearAsync_CriterioAceptacion_DniDuplicado_DebeLanzarBusinessRuleException()
    {
        // Gherkin:
        // Dado que ya existe un paciente con DNI "45678912"
        // Cuando se intenta registrar otro paciente con el mismo DNI
        // Entonces el código de respuesta debe ser 400 Bad Request
        // Y el mensaje de error debe indicar "Ya existe un paciente registrado con el DNI proporcionado"

        // Arrange
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 1,
            Dni = "45678912",
            Nombre = "Juan",
            Apellido = "Román",
            FNacimiento = new DateOnly(1978, 6, 24),
            Domicilio = "Brandsen 805",
            IdPais = "ARG",
            Email = "roman@boca.com",
            Activo = true,
            Pais = _fakeCatalogoRepo.Paises.First(p => p.IdPais == "ARG")
        });

        var dto = new CrearPacienteDto(
            "45678912",
            "Otro",
            "Paciente",
            new DateOnly(1995, 1, 1),
            "Calle Falsa 123",
            "ARG",
            null,
            "otro@paciente.com"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(dto));
        Assert.Equal("Ya existe un paciente registrado con el DNI proporcionado", ex.Message);
    }

    [Fact]
    public async Task ObtenerPorDniAsync_CriterioAceptacion_ConsultaRapidaPorDni_DebeRetornarPacienteCoincidente()
    {
        // Gherkin:
        // Dado que existe un paciente activo con DNI "45678912"
        // Cuando se consulta GET "/api/v1/pacientes/dni/45678912"
        // Entonces el código de respuesta debe ser 200 OK
        // Y los datos devueltos coinciden con dicho paciente

        // Arrange
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 2,
            Dni = "45678912",
            Nombre = "Guillermo",
            Apellido = "Barros",
            FNacimiento = new DateOnly(1973, 5, 4),
            Domicilio = "La Plata 100",
            IdPais = "ARG",
            Email = "guillermo@boca.com",
            Activo = true,
            Pais = _fakeCatalogoRepo.Paises.First(p => p.IdPais == "ARG")
        });

        // Act
        var result = await _service.ObtenerPorDniAsync("45678912");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.IdPaciente);
        Assert.Equal("45678912", result.Dni);
        Assert.Equal("Guillermo", result.Nombre);
        Assert.Equal("Barros", result.Apellido);
    }

    [Fact]
    public async Task CrearAsync_PaisInexistente_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new CrearPacienteDto(
            "88888888",
            "Pedro",
            "Pascal",
            new DateOnly(1985, 2, 2),
            "Santiago 10",
            "ZZZ", // País no existe
            null,
            "pedro@pascal.com"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(dto));
        Assert.Contains("no existe en el catálogo", ex.Message);
    }

    [Fact]
    public async Task ActualizarAsync_Exitoso_DebeActualizarCamposPermitidosYFechaModificacion()
    {
        // Arrange
        var paciente = new Paciente
        {
            IdPaciente = 5,
            Dni = "55555555",
            Nombre = "Lionel",
            Apellido = "Messi",
            FNacimiento = new DateOnly(1987, 6, 24),
            Domicilio = "Rosario 10",
            IdPais = "ARG",
            Email = "leo@messi.com",
            Activo = true,
            Pais = _fakeCatalogoRepo.Paises.First(p => p.IdPais == "ARG")
        };
        _fakePacienteRepo.Pacientes.Add(paciente);

        var dto = new ActualizarPacienteDto(
            "Lionel Andrés",
            "Messi Cuccittini",
            new DateOnly(1987, 6, 24),
            "Miami 10",
            "ARG",
            "+1305000000",
            "leo.andres@messi.com",
            "Capitán"
        );

        // Act
        var result = await _service.ActualizarAsync(5, dto);

        // Assert
        Assert.Equal("Lionel Andrés", result.Nombre);
        Assert.Equal("Messi Cuccittini", result.Apellido);
        Assert.Equal("Miami 10", result.Domicilio);
        Assert.Equal("+1305000000", result.Telefono);
        Assert.Equal("leo.andres@messi.com", result.Email);
        Assert.Equal("Capitán", result.Observacion);
        Assert.NotNull(result.FechaModificacion);
    }

    [Fact]
    public async Task BajaLogicaAsync_DebeMarcarActivoFalseSinBorradoFisico()
    {
        // Arrange
        var paciente = new Paciente
        {
            IdPaciente = 10,
            Dni = "10101010",
            Nombre = "Diego",
            Apellido = "Maradona",
            FNacimiento = new DateOnly(1960, 10, 30),
            Domicilio = "Villa Fiorito",
            IdPais = "ARG",
            Email = "diego@diego.com",
            Activo = true,
            Pais = _fakeCatalogoRepo.Paises.First(p => p.IdPais == "ARG")
        };
        _fakePacienteRepo.Pacientes.Add(paciente);

        // Act
        await _service.BajaLogicaAsync(10);

        // Assert
        var pacienteActualizado = await _service.ObtenerPorIdAsync(10);
        Assert.False(pacienteActualizado.Activo);
        Assert.NotNull(pacienteActualizado.FechaModificacion);
        Assert.Single(_fakePacienteRepo.Pacientes); // Permanece persistido en la base de datos
    }

    [Fact]
    public async Task ListarAsync_ConFiltroSoloActivos_DebeExcluirPacientesInactivos()
    {
        // Arrange
        var pais = _fakeCatalogoRepo.Paises.First(p => p.IdPais == "ARG");
        _fakePacienteRepo.Pacientes.AddRange(new[]
        {
            new Paciente { IdPaciente = 1, Nombre = "Ana", Apellido = "García", Dni = "D1", IdPais = "ARG", Pais = pais, Activo = true, Email = "a@a.com" },
            new Paciente { IdPaciente = 2, Nombre = "Beatriz", Apellido = "López", Dni = "D2", IdPais = "ARG", Pais = pais, Activo = false, Email = "b@b.com" },
            new Paciente { IdPaciente = 3, Nombre = "Carlos", Apellido = "Martínez", Dni = "D3", IdPais = "ARG", Pais = pais, Activo = true, Email = "c@c.com" }
        });

        var filtro = new PacienteFiltroDto(SoloActivos: true);

        // Act
        var result = await _service.ListarAsync(filtro);

        // Assert
        Assert.Equal(2, result.TotalRegistros);
        Assert.All(result.Items, p => Assert.True(p.Activo));
    }

    [Fact]
    public async Task ObtenerPorIdAsync_PacienteInexistente_DebeLanzarNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPorIdAsync(999));
    }

    [Fact]
    public async Task ObtenerPorDniAsync_PacienteInexistente_DebeLanzarNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPorDniAsync("99999999"));
    }
}

public class FakePacienteRepository : IPacienteRepository
{
    public List<Paciente> Pacientes { get; } = new();

    public Task<bool> ExisteDniAsync(string dni, int? ignorarIdPaciente = null, CancellationToken cancellationToken = default)
    {
        var existe = Pacientes.Any(p => p.Dni.Trim() == dni.Trim() &&
                                       (!ignorarIdPaciente.HasValue || p.IdPaciente != ignorarIdPaciente.Value));
        return Task.FromResult(existe);
    }

    public Task<Paciente?> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        var paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == idPaciente);
        return Task.FromResult(paciente);
    }

    public Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var paciente = Pacientes.FirstOrDefault(p => p.Dni.Trim() == dni.Trim());
        return Task.FromResult(paciente);
    }

    public Task<(IReadOnlyList<Paciente> Items, int Total)> ObtenerFiltradosAsync(
        string? busqueda,
        string? idPais,
        bool? soloActivos,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = Pacientes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(texto) ||
                p.Apellido.ToLower().Contains(texto) ||
                p.Dni.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(idPais))
        {
            var codigo = idPais.Trim().ToUpper();
            query = query.Where(p => p.IdPais == codigo);
        }

        if (soloActivos.HasValue)
        {
            query = query.Where(p => p.Activo == soloActivos.Value);
        }

        var total = query.Count();
        var items = query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Paciente>)items, total));
    }

    public Task<Paciente> AgregarAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        paciente.IdPaciente = Pacientes.Count > 0 ? Pacientes.Max(p => p.IdPaciente) + 1 : 1;
        Pacientes.Add(paciente);
        return Task.FromResult(paciente);
    }

    public Task ActualizarAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        var index = Pacientes.FindIndex(p => p.IdPaciente == paciente.IdPaciente);
        if (index >= 0)
        {
            Pacientes[index] = paciente;
        }
        return Task.CompletedTask;
    }
}
