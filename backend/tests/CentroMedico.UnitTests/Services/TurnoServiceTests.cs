using CentroMedico.Application.DTOs.Turnos;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Turnos;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class TurnoServiceTests
{
    private readonly FakeTurnoRepository _fakeTurnoRepo;
    private readonly FakePacienteRepository _fakePacienteRepo;
    private readonly FakeMedicoRepository _fakeMedicoRepo;
    private readonly TurnoService _service;

    public TurnoServiceTests()
    {
        _fakeTurnoRepo = new FakeTurnoRepository();
        _fakePacienteRepo = new FakePacienteRepository();
        _fakeMedicoRepo = new FakeMedicoRepository();

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
            Telefono = "11445566",
            Activo = true
        });

        // Paciente 6 activo (para intentar solapar)
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 6,
            Dni = "30333444",
            Nombre = "Marcos",
            Apellido = "Díaz",
            FNacimiento = new DateOnly(1992, 7, 21),
            Domicilio = "Corrientes 5678",
            IdPais = "ARG",
            Email = "marcos@test.com",
            Telefono = "11778899",
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

        var agendarValidator = new AgendarTurnoValidator();
        var cancelarValidator = new CancelarTurnoValidator();
        var actualizarEstadoValidator = new ActualizarEstadoTurnoValidator();

        _service = new TurnoService(
            _fakeTurnoRepo,
            _fakePacienteRepo,
            _fakeMedicoRepo,
            agendarValidator,
            cancelarValidator,
            actualizarEstadoValidator);
    }

    [Fact]
    public async Task AgendarAsync_CriterioAceptacion_AgendamientoExitosoSinConflicto_DebeRetornar201YEstadoPendiente()
    {
        // Gherkin:
        // Dado que el paciente 5 y el médico 1 están activos
        // Y el médico 1 no tiene ninguna cita el "2026-11-20 a las 10:00"
        // Cuando se solicita agendar el turno para esa fecha y hora
        // Entonces el código de respuesta debe ser 201 Created (verificado en el servicio)
        // Y el turno queda registrado con estado 0 (Pendiente)

        // Arrange
        var fecha = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        var dto = new AgendarTurnoDto(5, 1, fecha, "Consulta de rutina");

        // Act
        var resultado = await _service.AgendarAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.IdPaciente);
        Assert.Equal(1, resultado.IdMedico);
        Assert.Equal(fecha, resultado.FechaTurno);
        Assert.Equal((short)EstadoTurnoEnum.Pendiente, resultado.IdEstado);
        Assert.Equal("Pendiente", resultado.EstadoDescripcion);
        Assert.Equal("Gómez, Lucía", resultado.PacienteNombreCompleto);
        Assert.Equal("Quito, Esteban", resultado.MedicoNombreCompleto);

        Assert.Single(_fakeTurnoRepo.Turnos);
        Assert.Equal((short)EstadoTurnoEnum.Pendiente, _fakeTurnoRepo.Turnos[0].IdEstado);
    }

    [Fact]
    public async Task AgendarAsync_CriterioAceptacion_ConflictoHorario_DebeLanzarBusinessRuleException()
    {
        // Gherkin:
        // Dado que el médico 1 ya tiene un turno pendiente el "2026-11-20 a las 10:00"
        // Cuando otro paciente intenta agendar con el médico 1 en esa misma fecha y hora
        // Entonces el código de respuesta debe ser 400 Bad Request
        // Y el error especifica "El médico ya cuenta con una cita activa en ese horario"

        // Arrange: médico 1 ya tiene turno con paciente 5
        var fecha = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 1,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = fecha,
            IdEstado = (short)EstadoTurnoEnum.Pendiente,
            FechaAlta = DateTime.UtcNow
        });

        // Paciente 6 intenta agendar en esa misma fecha y hora con médico 1
        var dto = new AgendarTurnoDto(6, 1, fecha, "Intento superpuesto");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AgendarAsync(dto));
        Assert.Equal("El médico ya cuenta con una cita activa en ese horario", ex.Message);
    }

    [Fact]
    public async Task CancelarTurnoAsync_CriterioAceptacion_CancelacionYliberacionHorario_PermiteNuevaReserva()
    {
        // Gherkin:
        // Dado un turno pendiente asignado
        // Cuando se cancela con motivo "Paciente no puede asistir por viaje"
        // Entonces el turno pasa a estado 2 (Cancelado)
        // Y el horario queda disponible para nuevas reservas

        // Arrange: Turno asignado previamente al paciente 5
        var fecha = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        var turnoInicial = new Turno
        {
            IdTurno = 10,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = fecha,
            IdEstado = (short)EstadoTurnoEnum.Pendiente,
            Observacion = "Cita original",
            FechaAlta = DateTime.UtcNow,
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1),
            TurnoEstado = new TurnoEstado { IdEstado = 0, Descripcion = "Pendiente" }
        };
        _fakeTurnoRepo.Turnos.Add(turnoInicial);

        // Act 1: Cancelar turno
        var cancelarDto = new CancelarTurnoDto("Paciente no puede asistir por viaje");
        var cancelado = await _service.CancelarTurnoAsync(10, cancelarDto);

        // Assert 1: Estado cancelado y observación actualizada
        Assert.Equal((short)EstadoTurnoEnum.Cancelado, cancelado.IdEstado);
        Assert.Contains("Paciente no puede asistir por viaje", cancelado.Observacion);

        // Act 2: Paciente 6 intenta agendar en ese mismo horario liberado
        var nuevoDto = new AgendarTurnoDto(6, 1, fecha, "Nuevo turno en horario liberado");
        var nuevoTurno = await _service.AgendarAsync(nuevoDto);

        // Assert 2: Se agenda exitosamente sin conflicto
        Assert.NotNull(nuevoTurno);
        Assert.Equal(6, nuevoTurno.IdPaciente);
        Assert.Equal(1, nuevoTurno.IdMedico);
        Assert.Equal((short)EstadoTurnoEnum.Pendiente, nuevoTurno.IdEstado);
    }

    [Fact]
    public async Task AgendarAsync_PacienteInexistente_DebeLanzarNotFoundException()
    {
        var dto = new AgendarTurnoDto(9999, 1, DateTime.UtcNow.AddDays(1));
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.AgendarAsync(dto));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task AgendarAsync_PacienteInactivo_DebeLanzarBusinessRuleException()
    {
        var dto = new AgendarTurnoDto(99, 1, DateTime.UtcNow.AddDays(1));
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AgendarAsync(dto));
        Assert.Contains("El paciente no se encuentra activo", ex.Message);
    }

    [Fact]
    public async Task AgendarAsync_MedicoInexistente_DebeLanzarNotFoundException()
    {
        var dto = new AgendarTurnoDto(5, 9999, DateTime.UtcNow.AddDays(1));
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.AgendarAsync(dto));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task AgendarAsync_MedicoInactivo_DebeLanzarBusinessRuleException()
    {
        var dto = new AgendarTurnoDto(5, 88, DateTime.UtcNow.AddDays(1));
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AgendarAsync(dto));
        Assert.Contains("El médico no se encuentra activo", ex.Message);
    }

    [Fact]
    public async Task CancelarTurnoAsync_TurnoYaCancelado_DebeLanzarBusinessRuleException()
    {
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 15,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddDays(2),
            IdEstado = (short)EstadoTurnoEnum.Cancelado,
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1)
        });

        var dto = new CancelarTurnoDto("Cancelar nuevamente");
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CancelarTurnoAsync(15, dto));
        Assert.Equal("El turno ya se encuentra cancelado", ex.Message);
    }

    [Fact]
    public async Task CambiarEstadoAsync_TurnoRealizado_NoPermiteModificar()
    {
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 20,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddDays(-1),
            IdEstado = (short)EstadoTurnoEnum.Realizado,
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1)
        });

        var dto = new ActualizarEstadoTurnoDto((short)EstadoTurnoEnum.Cancelado, "Cambio prohibido");
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CambiarEstadoAsync(20, dto));
        Assert.Contains("No es posible modificar el estado de un turno ya realizado", ex.Message);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_TurnoInexistente_DebeLanzarNotFoundException()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPorIdAsync(9999));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task ObtenerAgendaAsync_FiltroPorMedico_DebeRetornarTurnosFiltrados()
    {
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 1,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddDays(1),
            IdEstado = (short)EstadoTurnoEnum.Pendiente,
            Paciente = _fakePacienteRepo.Pacientes.First(p => p.IdPaciente == 5),
            Medico = _fakeMedicoRepo.Medicos.First(m => m.IdMedico == 1)
        });

        var filtro = new TurnoFiltroAgendaDto(IdMedico: 1);
        var agenda = await _service.ObtenerAgendaAsync(filtro);

        Assert.Equal(1, agenda.TotalRegistros);
        Assert.Single(agenda.Items);
        Assert.Equal(1, agenda.Items[0].IdMedico);
    }
}

public class FakeTurnoRepository : ITurnoRepository
{
    public List<Turno> Turnos { get; } = new();

    public Task<bool> ExisteConflictoHorarioAsync(int idMedico, DateTime fechaTurno, int? ignorarIdTurno = null, CancellationToken cancellationToken = default)
    {
        const short cancelado = (short)EstadoTurnoEnum.Cancelado;
        const short anulado = (short)EstadoTurnoEnum.Anulado;

        var existe = Turnos.Any(t => t.IdMedico == idMedico &&
                                    t.FechaTurno == fechaTurno &&
                                    t.IdEstado != cancelado &&
                                    t.IdEstado != anulado &&
                                    (!ignorarIdTurno.HasValue || t.IdTurno != ignorarIdTurno.Value));
        return Task.FromResult(existe);
    }

    public Task<Turno?> ObtenerPorIdAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        var turno = Turnos.FirstOrDefault(t => t.IdTurno == idTurno);
        return Task.FromResult(turno);
    }

    public Task<Turno?> ObtenerPorIdConDetallesAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        var turno = Turnos.FirstOrDefault(t => t.IdTurno == idTurno);
        return Task.FromResult(turno);
    }

    public Task<(IReadOnlyList<Turno> Items, int Total)> ObtenerAgendaAsync(
        int? idMedico,
        int? idPaciente,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        short? idEstado,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = Turnos.AsEnumerable();

        if (idMedico.HasValue)
        {
            query = query.Where(t => t.IdMedico == idMedico.Value);
        }
        if (idPaciente.HasValue)
        {
            query = query.Where(t => t.IdPaciente == idPaciente.Value);
        }
        if (fechaInicio.HasValue)
        {
            query = query.Where(t => t.FechaTurno >= fechaInicio.Value);
        }
        if (fechaFin.HasValue)
        {
            query = query.Where(t => t.FechaTurno <= fechaFin.Value);
        }
        if (idEstado.HasValue)
        {
            query = query.Where(t => t.IdEstado == idEstado.Value);
        }

        var total = query.Count();
        var items = query
            .OrderBy(t => t.FechaTurno)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Turno>)items, total));
    }

    public Task<Turno> AgregarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        turno.IdTurno = Turnos.Count > 0 ? Turnos.Max(t => t.IdTurno) + 1 : 1;
        Turnos.Add(turno);
        return Task.FromResult(turno);
    }

    public Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        var index = Turnos.FindIndex(t => t.IdTurno == turno.IdTurno);
        if (index >= 0)
        {
            Turnos[index] = turno;
        }
        return Task.CompletedTask;
    }
}
