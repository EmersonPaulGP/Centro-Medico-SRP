using CentroMedico.Application.DTOs.Catalogos;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Catalogos;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class CatalogoServiceTests
{
    private readonly FakeCatalogoRepository _fakeRepo;
    private readonly CatalogoService _service;

    public CatalogoServiceTests()
    {
        _fakeRepo = new FakeCatalogoRepository();
        var validator = new CrearEspecialidadValidator();
        _service = new CatalogoService(_fakeRepo, validator);
    }

    [Fact]
    public async Task ObtenerPaisesAsync_DebeRetornarPaisesOrdenadosSinEspaciosEnBlanco()
    {
        // Arrange
        _fakeRepo.Paises.AddRange(new[]
        {
            new Pais { IdPais = "PER", Nombre = "Perú" },
            new Pais { IdPais = "ARG", Nombre = "Argentina" },
            new Pais { IdPais = "BRA", Nombre = "Brasil" }
        });

        // Act
        var result = (await _service.ObtenerPaisesAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("ARG", result[0].IdPais);
        Assert.Equal("Argentina", result[0].Nombre);
        Assert.Equal("Brasil", result[1].Nombre);
        Assert.Equal("Perú", result[2].Nombre);
    }

    [Fact]
    public async Task ObtenerEspecialidadesAsync_CriterioAceptacion_SoloActivas_DebeRetornar5De6()
    {
        // Gherkin:
        // Dado que existen 5 especialidades activas y 1 inactiva en la base de datos
        // Cuando el usuario consulta GET "/api/v1/catalogos/especialidades"
        // Entonces la lista debe contener exactamente las 5 especialidades activas

        // Arrange
        _fakeRepo.Especialidades.AddRange(new[]
        {
            new Especialidad { IdEspecialidad = 1, Nombre = "Clínica Médica", Activo = true },
            new Especialidad { IdEspecialidad = 2, Nombre = "Traumatología", Activo = true },
            new Especialidad { IdEspecialidad = 3, Nombre = "Pediatría", Activo = true },
            new Especialidad { IdEspecialidad = 4, Nombre = "Cardiología", Activo = true },
            new Especialidad { IdEspecialidad = 5, Nombre = "Gastroenterología", Activo = true },
            new Especialidad { IdEspecialidad = 6, Nombre = "Homeopatía Antigua", Activo = false }
        });

        // Act
        var activas = (await _service.ObtenerEspecialidadesAsync(soloActivas: true)).ToList();
        var todas = (await _service.ObtenerEspecialidadesAsync(soloActivas: false)).ToList();

        // Assert
        Assert.Equal(5, activas.Count);
        Assert.All(activas, e => Assert.True(e.Activo));
        Assert.Equal(6, todas.Count);
    }

    [Fact]
    public async Task CrearEspecialidadAsync_CriterioAceptacion_Duplicada_DebeLanzarBusinessRuleException()
    {
        // Gherkin:
        // Dado que ya existe la especialidad "Cardiología"
        // Cuando se envía POST "/api/v1/catalogos/especialidades" con nombre "Cardiología"
        // Entonces el código de respuesta debe ser 400 Bad Request
        // Y el mensaje de error debe indicar "Ya existe una especialidad con ese nombre"

        // Arrange
        _fakeRepo.Especialidades.Add(new Especialidad
        {
            IdEspecialidad = 1,
            Nombre = "Cardiología",
            Activo = true
        });

        var dto = new CrearEspecialidadDto("Cardiología", "Afecciones del corazón");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.CrearEspecialidadAsync(dto));

        Assert.Equal("Ya existe una especialidad con ese nombre", ex.Message);
    }

    [Fact]
    public async Task CrearEspecialidadAsync_CuandoEsNuevaYValida_DebeGuardarYRetornarDto()
    {
        // Arrange
        var dto = new CrearEspecialidadDto("Neurología", "Enfermedades del sistema nervioso");

        // Act
        var result = await _service.CrearEspecialidadAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IdEspecialidad > 0);
        Assert.Equal("Neurología", result.Nombre);
        Assert.Equal("Enfermedades del sistema nervioso", result.Descripcion);
        Assert.True(result.Activo);
    }

    [Fact]
    public async Task ObtenerEstadosTurno_DebeRetornarCatalogoGenerico()
    {
        // Arrange
        _fakeRepo.TurnosEstados.AddRange(new[]
        {
            new TurnoEstado { IdEstado = 0, Descripcion = "Pendiente" },
            new TurnoEstado { IdEstado = 1, Descripcion = "Realizado" }
        });

        // Act
        var result = (await _service.ObtenerEstadosTurnoAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].Id);
        Assert.Equal("Pendiente", result[0].Descripcion);
    }

    [Fact]
    public async Task ObtenerConceptosPago_DebeRetornarCatalogoGenerico()
    {
        // Arrange
        _fakeRepo.Conceptos.AddRange(new[]
        {
            new Concepto { IdConcepto = 1, Descripcion = "Consulta Médica General" }
        });

        // Act
        var result = (await _service.ObtenerConceptosPagoAsync()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Consulta Médica General", result[0].Descripcion);
    }

    [Fact]
    public async Task ObtenerEstadosPago_DebeRetornarCatalogoGenerico()
    {
        // Arrange
        _fakeRepo.PagosEstados.AddRange(new[]
        {
            new PagoEstado { IdEstado = 0, Descripcion = "Pendiente" },
            new PagoEstado { IdEstado = 1, Descripcion = "Aprobado" }
        });

        // Act
        var result = (await _service.ObtenerEstadosPagoAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[1].Id);
        Assert.Equal("Aprobado", result[1].Descripcion);
    }
}

public class FakeCatalogoRepository : ICatalogoRepository
{
    public List<Pais> Paises { get; } = new();
    public List<Especialidad> Especialidades { get; } = new();
    public List<TurnoEstado> TurnosEstados { get; } = new();
    public List<Concepto> Conceptos { get; } = new();
    public List<PagoEstado> PagosEstados { get; } = new();

    public Task<IReadOnlyList<Pais>> ObtenerPaisesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Pais> list = Paises.OrderBy(p => p.Nombre).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Especialidad>> ObtenerEspecialidadesAsync(bool? soloActivas = null, CancellationToken cancellationToken = default)
    {
        var query = Especialidades.AsEnumerable();
        if (soloActivas.HasValue)
        {
            query = query.Where(e => e.Activo == soloActivas.Value);
        }
        IReadOnlyList<Especialidad> list = query.OrderBy(e => e.Nombre).ToList();
        return Task.FromResult(list);
    }

    public Task<Especialidad?> ObtenerEspecialidadPorIdAsync(int idEspecialidad, CancellationToken cancellationToken = default)
    {
        var esp = Especialidades.FirstOrDefault(e => e.IdEspecialidad == idEspecialidad);
        return Task.FromResult(esp);
    }

    public Task<bool> ExisteEspecialidadConNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        var existe = Especialidades.Any(e => e.Nombre.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(existe);
    }

    public Task<Especialidad> CrearEspecialidadAsync(Especialidad especialidad, CancellationToken cancellationToken = default)
    {
        especialidad.IdEspecialidad = Especialidades.Count > 0 ? Especialidades.Max(e => e.IdEspecialidad) + 1 : 1;
        Especialidades.Add(especialidad);
        return Task.FromResult(especialidad);
    }

    public Task<IReadOnlyList<TurnoEstado>> ObtenerEstadosTurnoAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TurnoEstado> list = TurnosEstados.OrderBy(t => t.IdEstado).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Concepto>> ObtenerConceptosPagoAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Concepto> list = Conceptos.OrderBy(c => c.IdConcepto).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<PagoEstado>> ObtenerEstadosPagoAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PagoEstado> list = PagosEstados.OrderBy(p => p.IdEstado).ToList();
        return Task.FromResult(list);
    }
}
