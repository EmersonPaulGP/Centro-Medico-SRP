using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Medicos;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using CentroMedico.UnitTests.Services;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class MedicoServiceTests
{
    private readonly FakeMedicoRepository _fakeMedicoRepo;
    private readonly FakeCatalogoRepository _fakeCatalogoRepo;
    private readonly MedicoService _service;

    public MedicoServiceTests()
    {
        _fakeMedicoRepo = new FakeMedicoRepository();
        _fakeCatalogoRepo = new FakeCatalogoRepository();

        // Especialidad semilla
        _fakeCatalogoRepo.Especialidades.Add(new Especialidad
        {
            IdEspecialidad = 2,
            Nombre = "Traumatología",
            Activo = true
        });

        _fakeCatalogoRepo.Especialidades.Add(new Especialidad
        {
            IdEspecialidad = 3,
            Nombre = "Especialidad Inactiva",
            Activo = false
        });

        var crearValidator = new CrearMedicoValidator();
        var actualizarValidator = new ActualizarMedicoValidator();
        var asignarValidator = new AsignarEspecialidadValidator();

        _service = new MedicoService(_fakeMedicoRepo, _fakeCatalogoRepo, crearValidator, actualizarValidator, asignarValidator);
    }

    [Fact]
    public async Task CrearAsync_CriterioAceptacion_RegistroExitosoConEspecialidad_DebeRetornar201_Y_QuedarActivo()
    {
        // Gherkin:
        // Dado que el DNI "12345678" y la matrícula "MN-98765" no existen en el sistema
        // Y la especialidad "Traumatología" con ID 2 existe y está activa
        // Cuando el usuario envía los datos del médico asociándolo a la especialidad 2
        // Entonces el código de respuesta debe ser 201 Created (simulado a nivel servicio)
        // Y el médico queda persistido con estado activo = true y sus especialidades vinculadas

        // Arrange
        var dto = new CrearMedicoDto(
            "MN-98765",
            "12345678",
            "Alejandro",
            "Sanz",
            "+54911223344",
            "alejandro.sanz@hospital.com",
            new List<AsignarEspecialidadDto> { new(2, "Jefe de Servicio") }
        );

        // Act
        var result = await _service.CrearAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IdMedico > 0);
        Assert.Equal("MN-98765", result.Matricula);
        Assert.Equal("12345678", result.Dni);
        Assert.True(result.Activo);
        Assert.Single(result.Especialidades);
        Assert.Equal(2, result.Especialidades[0].IdEspecialidad);
        Assert.Equal("Traumatología", result.Especialidades[0].Nombre);
    }

    [Fact]
    public async Task CrearAsync_CriterioAceptacion_MatriculaDuplicada_DebeLanzarBusinessRuleException()
    {
        // Gherkin:
        // Dado que ya existe un médico registrado con matrícula "MN-98765"
        // Cuando se intenta registrar otro médico con la misma matrícula
        // Entonces la API responde con código 400 Bad Request
        // Y el error especifica "Ya existe un médico registrado con esa matrícula profesional"

        // Arrange
        _fakeMedicoRepo.Medicos.Add(new Medico
        {
            IdMedico = 1,
            Matricula = "MN-98765",
            Dni = "87654321",
            Nombre = "Juan",
            Apellido = "Pérez",
            Activo = true
        });

        var dto = new CrearMedicoDto(
            "MN-98765",
            "99887766",
            "Roberto",
            "Gómez",
            null,
            null,
            new List<AsignarEspecialidadDto> { new(2) }
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(dto));
        Assert.Equal("Ya existe un médico registrado con esa matrícula profesional", ex.Message);
    }

    [Fact]
    public async Task CrearAsync_DniDuplicado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        _fakeMedicoRepo.Medicos.Add(new Medico
        {
            IdMedico = 1,
            Matricula = "MN-11111",
            Dni = "12345678",
            Nombre = "Juan",
            Apellido = "Pérez",
            Activo = true
        });

        var dto = new CrearMedicoDto(
            "MN-22222",
            "12345678",
            "Roberto",
            "Gómez",
            null,
            null,
            new List<AsignarEspecialidadDto> { new(2) }
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(dto));
        Assert.Equal("Ya existe un médico registrado con ese DNI", ex.Message);
    }

    [Fact]
    public async Task CrearAsync_EspecialidadInactiva_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new CrearMedicoDto(
            "MN-33333",
            "33333333",
            "Clara",
            "López",
            null,
            null,
            new List<AsignarEspecialidadDto> { new(3) } // Id 3 está inactiva
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(dto));
        Assert.Contains("no existe o no se encuentra activa", ex.Message);
    }

    [Fact]
    public async Task CambiarEstadoActivoAsync_BajaLogica_DebeDesactivarMedicoSinBorradoFisico()
    {
        // Arrange
        var medico = new Medico
        {
            IdMedico = 10,
            Matricula = "MN-10101",
            Dni = "10101010",
            Nombre = "Mario",
            Apellido = "Bross",
            Activo = true
        };
        _fakeMedicoRepo.Medicos.Add(medico);

        // Act
        await _service.CambiarEstadoActivoAsync(10, false);

        // Assert
        var medicoActualizado = await _service.ObtenerPorIdAsync(10);
        Assert.False(medicoActualizado.Activo);
        Assert.Single(_fakeMedicoRepo.Medicos); // Sigue existiendo en la base de datos
    }

    [Fact]
    public async Task ListarAsync_ConFiltroSoloActivos_DebeExcluirMedicosDadosDeBaja()
    {
        // Arrange
        _fakeMedicoRepo.Medicos.AddRange(new[]
        {
            new Medico { IdMedico = 1, Nombre = "Ana", Apellido = "García", Matricula = "M1", Dni = "D1", Activo = true },
            new Medico { IdMedico = 2, Nombre = "Beatriz", Apellido = "López", Matricula = "M2", Dni = "D2", Activo = false },
            new Medico { IdMedico = 3, Nombre = "Carlos", Apellido = "Martínez", Matricula = "M3", Dni = "D3", Activo = true }
        });

        var filtro = new MedicoFiltroDto(SoloActivos: true);

        // Act
        var result = await _service.ListarAsync(filtro);

        // Assert
        Assert.Equal(2, result.TotalRegistros);
        Assert.All(result.Items, m => Assert.True(m.Activo));
    }

    [Fact]
    public async Task ActualizarAsync_Exitoso_DebeActualizarDatosPersonalesYFechaModificacion()
    {
        // Arrange
        var medico = new Medico
        {
            IdMedico = 1,
            Matricula = "MN-1234",
            Dni = "12345678",
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = "111",
            Email = "juan@test.com",
            Activo = true
        };
        _fakeMedicoRepo.Medicos.Add(medico);

        var dto = new ActualizarMedicoDto("Juan Carlos", "Pérez Gómez", "222", "jc.perez@test.com");

        // Act
        var result = await _service.ActualizarAsync(1, dto);

        // Assert
        Assert.Equal("Juan Carlos", result.Nombre);
        Assert.Equal("Pérez Gómez", result.Apellido);
        Assert.Equal("222", result.Telefono);
        Assert.Equal("jc.perez@test.com", result.Email);
        Assert.NotNull(result.FechaModificacion);
    }

    [Fact]
    public async Task AsignarEspecialidadAsync_Exitoso_DebeAgregarNuevaEspecialidad()
    {
        // Arrange
        _fakeCatalogoRepo.Especialidades.Add(new Especialidad { IdEspecialidad = 4, Nombre = "Cardiología", Activo = true });

        var medico = new Medico
        {
            IdMedico = 20,
            Matricula = "MN-20202",
            Dni = "20202020",
            Nombre = "Laura",
            Apellido = "Fernández",
            Activo = true,
            Especialidades = new List<MedicoEspecialidad>
            {
                new() { IdMedico = 20, IdEspecialidad = 2, Especialidad = _fakeCatalogoRepo.Especialidades.First(e => e.IdEspecialidad == 2) }
            }
        };
        _fakeMedicoRepo.Medicos.Add(medico);

        var dto = new AsignarEspecialidadDto(4, "Cardióloga clínica");

        // Act
        var result = await _service.AsignarEspecialidadAsync(20, dto);

        // Assert
        Assert.Equal(2, result.Especialidades.Count);
        Assert.Contains(result.Especialidades, e => e.IdEspecialidad == 4 && e.Nombre == "Cardiología");
    }

    [Fact]
    public async Task AsignarEspecialidadAsync_EspecialidadYaAsignada_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var medico = new Medico
        {
            IdMedico = 21,
            Matricula = "MN-21212",
            Dni = "21212121",
            Nombre = "Esteban",
            Apellido = "Quito",
            Activo = true,
            Especialidades = new List<MedicoEspecialidad>
            {
                new() { IdMedico = 21, IdEspecialidad = 2, Especialidad = _fakeCatalogoRepo.Especialidades.First(e => e.IdEspecialidad == 2) }
            }
        };
        _fakeMedicoRepo.Medicos.Add(medico);

        var dto = new AsignarEspecialidadDto(2, "Repetida");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AsignarEspecialidadAsync(21, dto));
        Assert.Contains("ya tiene asignada la especialidad", ex.Message);
    }

    [Fact]
    public async Task RemoverEspecialidadAsync_Exitoso_DebeEliminarEspecialidad()
    {
        // Arrange
        var esp2 = _fakeCatalogoRepo.Especialidades.First(e => e.IdEspecialidad == 2);
        _fakeCatalogoRepo.Especialidades.Add(new Especialidad { IdEspecialidad = 5, Nombre = "Pediatría", Activo = true });
        var esp5 = _fakeCatalogoRepo.Especialidades.First(e => e.IdEspecialidad == 5);

        var medico = new Medico
        {
            IdMedico = 30,
            Matricula = "MN-30303",
            Dni = "30303030",
            Nombre = "Marina",
            Apellido = "Silva",
            Activo = true,
            Especialidades = new List<MedicoEspecialidad>
            {
                new() { IdMedico = 30, IdEspecialidad = 2, Especialidad = esp2 },
                new() { IdMedico = 30, IdEspecialidad = 5, Especialidad = esp5 }
            }
        };
        _fakeMedicoRepo.Medicos.Add(medico);

        // Act
        var result = await _service.RemoverEspecialidadAsync(30, 5);

        // Assert
        Assert.Single(result.Especialidades);
        Assert.Equal(2, result.Especialidades[0].IdEspecialidad);
    }

    [Fact]
    public async Task RemoverEspecialidadAsync_NoPuedeRemoverUnicaEspecialidad_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var esp2 = _fakeCatalogoRepo.Especialidades.First(e => e.IdEspecialidad == 2);
        var medico = new Medico
        {
            IdMedico = 31,
            Matricula = "MN-31313",
            Dni = "31313131",
            Nombre = "Rodrigo",
            Apellido = "Paz",
            Activo = true,
            Especialidades = new List<MedicoEspecialidad>
            {
                new() { IdMedico = 31, IdEspecialidad = 2, Especialidad = esp2 }
            }
        };
        _fakeMedicoRepo.Medicos.Add(medico);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RemoverEspecialidadAsync(31, 2));
        Assert.Contains("El médico debe conservar al menos una especialidad médica asignada", ex.Message);
    }
}

public class FakeMedicoRepository : IMedicoRepository
{
    public List<Medico> Medicos { get; } = new();

    public Task<bool> ExisteMatriculaAsync(string matricula, int? ignorarIdMedico = null, CancellationToken cancellationToken = default)
    {
        var existe = Medicos.Any(m => m.Matricula.Equals(matricula.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                      (!ignorarIdMedico.HasValue || m.IdMedico != ignorarIdMedico.Value));
        return Task.FromResult(existe);
    }

    public Task<bool> ExisteDniAsync(string dni, int? ignorarIdMedico = null, CancellationToken cancellationToken = default)
    {
        var existe = Medicos.Any(m => m.Dni.Trim() == dni.Trim() &&
                                      (!ignorarIdMedico.HasValue || m.IdMedico != ignorarIdMedico.Value));
        return Task.FromResult(existe);
    }

    public Task<(IReadOnlyList<Medico> Items, int Total)> ObtenerFiltradosAsync(
        string? busqueda,
        int? idEspecialidad,
        bool? soloActivos,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = Medicos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            query = query.Where(m =>
                m.Nombre.ToLower().Contains(texto) ||
                m.Apellido.ToLower().Contains(texto) ||
                m.Matricula.ToLower().Contains(texto) ||
                m.Dni.Contains(texto));
        }

        if (idEspecialidad.HasValue)
        {
            query = query.Where(m => m.Especialidades.Any(me => me.IdEspecialidad == idEspecialidad.Value));
        }

        if (soloActivos.HasValue)
        {
            query = query.Where(m => m.Activo == soloActivos.Value);
        }

        var total = query.Count();
        var items = query
            .OrderBy(m => m.Apellido)
            .ThenBy(m => m.Nombre)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Medico>)items, total));
    }

    public Task<Medico?> ObtenerConEspecialidadesPorIdAsync(int idMedico, CancellationToken cancellationToken = default)
    {
        var medico = Medicos.FirstOrDefault(m => m.IdMedico == idMedico);
        return Task.FromResult(medico);
    }

    public Task<Medico> CrearAsync(Medico medico, CancellationToken cancellationToken = default)
    {
        medico.IdMedico = Medicos.Count > 0 ? Medicos.Max(m => m.IdMedico) + 1 : 1;
        Medicos.Add(medico);
        return Task.FromResult(medico);
    }

    public Task ActualizarAsync(Medico medico, CancellationToken cancellationToken = default)
    {
        var index = Medicos.FindIndex(m => m.IdMedico == medico.IdMedico);
        if (index >= 0)
        {
            Medicos[index] = medico;
        }
        return Task.CompletedTask;
    }

    public Task ActualizarEspecialidadesAsync(
        int idMedico,
        IEnumerable<MedicoEspecialidad> nuevasEspecialidades,
        CancellationToken cancellationToken = default)
    {
        var medico = Medicos.FirstOrDefault(m => m.IdMedico == idMedico);
        if (medico != null)
        {
            medico.Especialidades = nuevasEspecialidades.ToList();
        }
        return Task.CompletedTask;
    }

    public Task AgregarEspecialidadAsync(MedicoEspecialidad especialidad, CancellationToken cancellationToken = default)
    {
        var medico = Medicos.FirstOrDefault(m => m.IdMedico == especialidad.IdMedico);
        if (medico != null)
        {
            medico.Especialidades.Add(especialidad);
        }
        return Task.CompletedTask;
    }

    public Task EliminarEspecialidadAsync(int idMedico, int idEspecialidad, CancellationToken cancellationToken = default)
    {
        var medico = Medicos.FirstOrDefault(m => m.IdMedico == idMedico);
        if (medico != null)
        {
            var esp = medico.Especialidades.FirstOrDefault(e => e.IdEspecialidad == idEspecialidad);
            if (esp != null)
            {
                medico.Especialidades.Remove(esp);
            }
        }
        return Task.CompletedTask;
    }
}
