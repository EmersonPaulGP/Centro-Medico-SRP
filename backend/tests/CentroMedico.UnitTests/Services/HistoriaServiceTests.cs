using CentroMedico.Application.DTOs.Historia;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Historia;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class HistoriaServiceTests
{
    private readonly FakeHistoriaRepository _fakeHistoriaRepo;
    private readonly FakePacienteRepository _fakePacienteRepo;
    private readonly FakeMedicoRepository _fakeMedicoRepo;
    private readonly FakeTurnoRepository _fakeTurnoRepo;
    private readonly HistoriaService _service;

    public HistoriaServiceTests()
    {
        _fakeHistoriaRepo = new FakeHistoriaRepository();
        _fakePacienteRepo = new FakePacienteRepository();
        _fakeMedicoRepo = new FakeMedicoRepository();
        _fakeTurnoRepo = new FakeTurnoRepository();

        // Paciente 5 activo
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 5,
            Dni = "30111222",
            Nombre = "Lucía",
            Apellido = "Gómez",
            FNacimiento = new DateOnly(1995, 3, 10),
            Domicilio = "Av. Santa Fe 1234",
            IdPais = "ARG",
            Email = "lucia@test.com",
            Activo = true
        });

        // Paciente 99 inactivo
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 99,
            Dni = "99999999",
            Nombre = "Inactivo",
            Apellido = "Paciente",
            FNacimiento = new DateOnly(1980, 1, 1),
            Domicilio = "Sin domicilio",
            IdPais = "ARG",
            Email = "inactivo@test.com",
            Activo = false
        });

        // Médico 1 activo
        _fakeMedicoRepo.Medicos.Add(new Medico
        {
            IdMedico = 1,
            Matricula = "MN-12345",
            Dni = "20111222",
            Nombre = "Esteban",
            Apellido = "Quito",
            Activo = true
        });

        // Médico 88 inactivo
        _fakeMedicoRepo.Medicos.Add(new Medico
        {
            IdMedico = 88,
            Matricula = "MN-88888",
            Dni = "20888888",
            Nombre = "Médico",
            Apellido = "Inactivo",
            Activo = false
        });

        var validator = new CrearHistoriaValidator();
        _service = new HistoriaService(
            _fakeHistoriaRepo,
            _fakePacienteRepo,
            _fakeMedicoRepo,
            _fakeTurnoRepo,
            validator);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_CriterioAceptacion_ConTurnoPendiente_DebeCrearHistoriaYMarcarTurnoComoRealizado()
    {
        // Gherkin:
        // Dado que existe un turno con ID 20 en estado 0 (Pendiente) para el paciente 5 con el médico 1
        // Cuando el médico 1 registra la historia clínica con:
        //   | diagnostico          | observacion                              |
        //   | Faringitis bacteriana| Reposo 48hs y prescripción de Amoxicilina|
        // Entonces la API responde con código 201 Created (simulado a nivel servicio)
        // Y el turno 20 cambia automáticamente su estado a 1 (Realizado)
        // Y la historia queda indexada para el paciente 5

        // Arrange
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 20,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddHours(-1),
            IdEstado = (short)EstadoTurnoEnum.Pendiente,
            FechaAlta = DateTime.UtcNow.AddDays(-1)
        });

        var dto = new CrearHistoriaDto(
            IdPaciente: 5,
            IdMedico: 1,
            IdTurno: 20,
            Diagnostico: "Faringitis bacteriana",
            Observacion: "Reposo 48hs y prescripción de Amoxicilina"
        );

        // Act
        var resultado = await _service.RegistrarEvolucionAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.IdPaciente);
        Assert.Equal(1, resultado.IdMedico);
        Assert.Equal(20, resultado.IdTurno);
        Assert.Equal("Faringitis bacteriana", resultado.Diagnostico);
        Assert.Equal("Reposo 48hs y prescripción de Amoxicilina", resultado.Observacion);

        // Verificar que el turno 20 cambió a Realizado (1)
        var turno = _fakeTurnoRepo.Turnos.First(t => t.IdTurno == 20);
        Assert.Equal((short)EstadoTurnoEnum.Realizado, turno.IdEstado);
        Assert.NotNull(turno.FechaModificacion);

        // Verificar indexado para el paciente 5
        Assert.Single(_fakeHistoriaRepo.Historias);
        Assert.Equal(5, _fakeHistoriaRepo.Historias[0].IdPaciente);
    }

    [Fact]
    public async Task ObtenerHistoriaPacienteAsync_CriterioAceptacion_DebeRetornarEvolucionesOrdenadasDescendente()
    {
        // Gherkin:
        // Dado que el paciente 5 tiene 3 evoluciones registradas en distintas fechas
        // Cuando se consulta GET "/api/v1/pacientes/5/historia"
        // Entonces la API responde con código 200 OK
        // Y devuelve las 3 evoluciones ordenadas de la más reciente a la más antigua

        // Arrange
        var fecha1 = DateTime.UtcNow.AddDays(-10);
        var fecha2 = DateTime.UtcNow.AddDays(-5);
        var fecha3 = DateTime.UtcNow.AddDays(-1);

        _fakeHistoriaRepo.Historias.Add(new Historia
        {
            IdHistoria = 1,
            IdPaciente = 5,
            IdMedico = 1,
            FechaHistoria = fecha1,
            Diagnostico = "Consulta inicial",
            Observacion = "Paciente con dolor de cabeza persistente",
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1)
        });

        _fakeHistoriaRepo.Historias.Add(new Historia
        {
            IdHistoria = 2,
            IdPaciente = 5,
            IdMedico = 1,
            FechaHistoria = fecha3,
            Diagnostico = "Alta médica",
            Observacion = "Evolución favorable, se otorga alta médica completa",
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1)
        });

        _fakeHistoriaRepo.Historias.Add(new Historia
        {
            IdHistoria = 3,
            IdPaciente = 5,
            IdMedico = 1,
            FechaHistoria = fecha2,
            Diagnostico = "Seguimiento",
            Observacion = "Control de medicación y estudios de laboratorio normales",
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1)
        });

        var filtro = new HistoriaFiltroDto();

        // Act
        var resultado = await _service.ObtenerHistoriaPacienteAsync(5, filtro);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.TotalRegistros);
        Assert.Equal(3, resultado.Items.Count);

        // Orden de más reciente a más antigua: fecha3 (id 2) -> fecha2 (id 3) -> fecha1 (id 1)
        Assert.Equal(2, resultado.Items[0].IdHistoria);
        Assert.Equal(3, resultado.Items[1].IdHistoria);
        Assert.Equal(1, resultado.Items[2].IdHistoria);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_SinTurnoAsociado_DebeCrearHistoriaExitosamente()
    {
        // Arrange
        var dto = new CrearHistoriaDto(
            IdPaciente: 5,
            IdMedico: 1,
            IdTurno: null,
            Diagnostico: "Chequeo preventivo",
            Observacion: "Control anual de signos vitales dentro de parámetros"
        );

        // Act
        var resultado = await _service.RegistrarEvolucionAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Null(resultado.IdTurno);
        Assert.Single(_fakeHistoriaRepo.Historias);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_PacienteInexistente_DebeLanzarNotFoundException()
    {
        var dto = new CrearHistoriaDto(9999, 1, null, "Test", "Observación de prueba válida");
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_PacienteInactivo_DebeLanzarBusinessRuleException()
    {
        var dto = new CrearHistoriaDto(99, 1, null, "Test", "Observación de prueba válida");
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("El paciente no se encuentra activo", ex.Message);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_MedicoInexistente_DebeLanzarNotFoundException()
    {
        var dto = new CrearHistoriaDto(5, 9999, null, "Test", "Observación de prueba válida");
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_MedicoInactivo_DebeLanzarBusinessRuleException()
    {
        var dto = new CrearHistoriaDto(5, 88, null, "Test", "Observación de prueba válida");
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("El médico no se encuentra activo", ex.Message);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_TurnoInexistente_DebeLanzarNotFoundException()
    {
        var dto = new CrearHistoriaDto(5, 1, 9999, "Test", "Observación de prueba válida");
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_TurnoNoCorrespondeAPaciente_DebeLanzarBusinessRuleException()
    {
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 30,
            IdPaciente = 999, // Otro paciente
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow,
            IdEstado = (short)EstadoTurnoEnum.Pendiente
        });

        var dto = new CrearHistoriaDto(5, 1, 30, "Test", "Observación clínica válida");
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("no corresponde al paciente", ex.Message);
    }

    [Fact]
    public async Task RegistrarEvolucionAsync_TurnoCancelado_DebeLanzarBusinessRuleException()
    {
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 35,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow,
            IdEstado = (short)EstadoTurnoEnum.Cancelado
        });

        var dto = new CrearHistoriaDto(5, 1, 35, "Test", "Observación clínica válida");
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarEvolucionAsync(dto));
        Assert.Contains("turno cancelado", ex.Message);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_HistoriaInexistente_DebeLanzarNotFoundException()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPorIdAsync(9999));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task ObtenerHistoriaPacienteAsync_PacienteInexistente_DebeLanzarNotFoundException()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.ObtenerHistoriaPacienteAsync(9999, new HistoriaFiltroDto()));
        Assert.Contains("9999", ex.Message);
    }
}

public class FakeHistoriaRepository : IHistoriaRepository
{
    public List<Historia> Historias { get; } = new();

    public Task<Historia?> ObtenerPorIdConDetalleAsync(int idHistoria, CancellationToken cancellationToken = default)
    {
        var historia = Historias.FirstOrDefault(h => h.IdHistoria == idHistoria);
        return Task.FromResult(historia);
    }

    public Task<(IReadOnlyList<Historia> Items, int Total)> ObtenerPorPacienteIdAsync(
        int idPaciente,
        int? idMedico,
        DateTime? desde,
        DateTime? hasta,
        string? terminoBusqueda,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = Historias.Where(h => h.IdPaciente == idPaciente);

        if (idMedico.HasValue)
        {
            query = query.Where(h => h.IdMedico == idMedico.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(h => h.FechaHistoria >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(h => h.FechaHistoria <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(terminoBusqueda))
        {
            var termino = terminoBusqueda.Trim().ToLower();
            query = query.Where(h =>
                (h.Diagnostico != null && h.Diagnostico.ToLower().Contains(termino)) ||
                h.Observacion.ToLower().Contains(termino));
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(h => h.FechaHistoria)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Historia>)items, total));
    }

    public Task<Historia> AgregarAsync(Historia historia, CancellationToken cancellationToken = default)
    {
        historia.IdHistoria = Historias.Count > 0 ? Historias.Max(h => h.IdHistoria) + 1 : 1;
        Historias.Add(historia);
        return Task.FromResult(historia);
    }
}
