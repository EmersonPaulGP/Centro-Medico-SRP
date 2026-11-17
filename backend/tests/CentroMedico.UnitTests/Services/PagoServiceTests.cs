using CentroMedico.Application.DTOs.Pagos;
using CentroMedico.Application.Services;
using CentroMedico.Application.Validators.Pagos;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class PagoServiceTests
{
    private readonly FakePagoRepository _fakePagoRepo;
    private readonly FakePacienteRepository _fakePacienteRepo;
    private readonly FakeCatalogoRepository _fakeCatalogoRepo;
    private readonly FakeTurnoRepository _fakeTurnoRepo;
    private readonly PagoService _service;

    public PagoServiceTests()
    {
        _fakePagoRepo = new FakePagoRepository();
        _fakePacienteRepo = new FakePacienteRepository();
        _fakeCatalogoRepo = new FakeCatalogoRepository();
        _fakeTurnoRepo = new FakeTurnoRepository();
        var validator = new RegistrarPagoValidator();

        _service = new PagoService(
            _fakePagoRepo,
            _fakePacienteRepo,
            _fakeCatalogoRepo,
            _fakeTurnoRepo,
            validator
        );

        // Seed: Paciente 5 activo
        _fakePacienteRepo.Pacientes.Add(new Paciente
        {
            IdPaciente = 5,
            Dni = "30111222",
            Nombre = "Lucía",
            Apellido = "Gómez",
            FNacimiento = new DateOnly(1995, 3, 10),
            Domicilio = "Av. Santa Fe 1234",
            IdPais = "ARG",
            Email = "lucia.gomez@test.com",
            Activo = true
        });

        // Seed: Paciente 99 inactivo
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

        // Seed: Concepto 1
        _fakeCatalogoRepo.Conceptos.Add(new Concepto
        {
            IdConcepto = 1,
            Descripcion = "Consulta Médica General"
        });

        // Seed: Concepto 2
        _fakeCatalogoRepo.Conceptos.Add(new Concepto
        {
            IdConcepto = 2,
            Descripcion = "Práctica de Laboratorio"
        });

        // Seed: Estados de Pago
        _fakeCatalogoRepo.PagosEstados.Add(new PagoEstado { IdEstado = 0, Descripcion = "Pendiente" });
        _fakeCatalogoRepo.PagosEstados.Add(new PagoEstado { IdEstado = 1, Descripcion = "Aprobado" });
        _fakeCatalogoRepo.PagosEstados.Add(new PagoEstado { IdEstado = 2, Descripcion = "Rechazado" });
        _fakeCatalogoRepo.PagosEstados.Add(new PagoEstado { IdEstado = 3, Descripcion = "Reembolsado" });

        // Seed: Turno 10 para Paciente 5 (Pendiente)
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 10,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddHours(2),
            IdEstado = (short)EstadoTurnoEnum.Pendiente
        });

        // Seed: Turno 11 para Paciente 5 (Cancelado)
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 11,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddDays(-1),
            IdEstado = (short)EstadoTurnoEnum.Cancelado
        });

        // Seed: Turno 12 para Paciente 5 (Anulado)
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 12,
            IdPaciente = 5,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddDays(-2),
            IdEstado = (short)EstadoTurnoEnum.Anulado
        });

        // Seed: Turno 20 para Paciente 1 (Otro paciente)
        _fakeTurnoRepo.Turnos.Add(new Turno
        {
            IdTurno = 20,
            IdPaciente = 1,
            IdMedico = 1,
            FechaTurno = DateTime.UtcNow.AddDays(1),
            IdEstado = (short)EstadoTurnoEnum.Pendiente
        });
    }

    [Fact]
    public async Task CriterioAceptacion_RegistroCobroConsultaExitoso_DebeRetornarEstadoAprobadoYFechaActual()
    {
        // Gherkin:
        // Escenario: Registro de cobro de consulta exitoso
        //   Dado que existe el paciente 5 y el concepto 1 "Consulta Médica General"
        //   Cuando se registra un pago con monto 5000.00 en método "Efectivo"
        //   Entonces la API responde con código 201 Created
        //   Y el pago queda registrado con estado 1 (Aprobado) y fecha actual

        // Arrange
        var dto = new RegistrarPagoDto(
            IdPaciente: 5,
            IdTurno: null,
            IdConcepto: 1,
            Monto: 5000.00m,
            MetodoPago: "Efectivo",
            Observacion: "Cobro en recepción"
        );

        // Act
        var result = await _service.RegistrarPagoAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IdPago > 0);
        Assert.Equal(5, result.IdPaciente);
        Assert.Equal("Gómez, Lucía", result.PacienteNombreCompleto);
        Assert.Equal("30111222", result.PacienteDni);
        Assert.Equal(1, result.IdConcepto);
        Assert.Equal("Consulta Médica General", result.ConceptoDescripcion);
        Assert.Equal(5000.00m, result.Monto);
        Assert.Equal(1, result.IdEstado);
        Assert.Equal("Aprobado", result.EstadoDescripcion);
        Assert.Equal("Efectivo", result.MetodoPago);
        Assert.True((DateTime.UtcNow - result.Fecha).TotalSeconds < 5);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-150.00)]
    public async Task CriterioAceptacion_IntentoDeCobroConMontoNegativoOCero_DebeLanzarBusinessRuleException(decimal montoInvalido)
    {
        // Gherkin:
        // Escenario: Intento de cobro con monto negativo o cero
        //   Dado un paciente válido y un concepto válido
        //   Cuando se intenta registrar un pago con monto 0.00 o -150.00
        //   Entonces la API responde con código 400 Bad Request
        //   Y el error especifica "El monto a cobrar debe ser estrictamente mayor a cero"

        // Arrange
        var dto = new RegistrarPagoDto(
            IdPaciente: 5,
            IdTurno: null,
            IdConcepto: 1,
            Monto: montoInvalido,
            MetodoPago: "Efectivo",
            Observacion: null
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Equal("El monto a cobrar debe ser estrictamente mayor a cero", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_ConTurnoAsociadoValido_DebeGuardarIdTurno()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, 10, 1, 3500.00m, "Tarjeta de Débito", "Pago con débito");

        // Act
        var result = await _service.RegistrarPagoAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.IdTurno);
        Assert.Equal("Tarjeta de Débito", result.MetodoPago);
    }

    [Fact]
    public async Task RegistrarPago_PacienteInexistente_DebeLanzarNotFoundException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(999, null, 1, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_PacienteInactivo_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(99, null, 1, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Equal("El paciente no se encuentra activo", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_ConceptoInexistente_DebeLanzarNotFoundException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, null, 88, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Contains("88", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_TurnoInexistente_DebeLanzarNotFoundException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, 9999, 1, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_TurnoDeOtroPaciente_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, 20, 1, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Equal("El turno especificado no corresponde al paciente indicado", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_TurnoCancelado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, 11, 1, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Equal("No es posible asociar un cobro a un turno cancelado", ex.Message);
    }

    [Fact]
    public async Task RegistrarPago_TurnoAnulado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, 12, 1, 1000m, "Efectivo", null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RegistrarPagoAsync(dto));
        Assert.Equal("No es posible asociar un cobro a un turno anulado", ex.Message);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_PagoExistente_DebeRetornarPagoDetalle()
    {
        // Arrange
        var nuevo = await _service.RegistrarPagoAsync(new RegistrarPagoDto(5, null, 1, 2000m, "Efectivo", "Nota"));

        // Act
        var result = await _service.ObtenerPorIdAsync(nuevo.IdPago);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nuevo.IdPago, result.IdPago);
        Assert.Equal(2000m, result.Monto);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_PagoInexistente_DebeLanzarNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerPorIdAsync(9999));
        Assert.Contains("9999", ex.Message);
    }

    [Fact]
    public async Task ObtenerComprobanteAsync_PagoExistente_DebeRetornarComprobanteDigitalConDatosFiscales()
    {
        // Arrange
        var nuevo = await _service.RegistrarPagoAsync(new RegistrarPagoDto(5, 10, 1, 4500m, "Transferencia Bancaria", "Arancel consulta"));

        // Act
        var comprobante = await _service.ObtenerComprobanteAsync(nuevo.IdPago);

        // Assert
        Assert.NotNull(comprobante);
        Assert.Equal(nuevo.IdPago, comprobante.NumeroComprobante);
        Assert.Equal("Centro Médico SRP", comprobante.CentroMedicoNombre);
        Assert.Equal("30-71234567-9", comprobante.CentroMedicoCuit);
        Assert.Equal("Gómez, Lucía", comprobante.PacienteNombre);
        Assert.Equal("30111222", comprobante.PacienteDni);
        Assert.Equal("Av. Santa Fe 1234", comprobante.PacienteDomicilio);
        Assert.Equal("Consulta Médica General", comprobante.Concepto);
        Assert.Equal(4500m, comprobante.Subtotal);
        Assert.Equal(4500m, comprobante.Total);
        Assert.Equal("Transferencia Bancaria", comprobante.MetodoPago);
    }

    [Fact]
    public async Task ObtenerComprobanteAsync_PagoInexistente_DebeLanzarNotFoundException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerComprobanteAsync(8888));
        Assert.Contains("8888", ex.Message);
    }

    [Fact]
    public async Task ListarPagosAsync_FiltroPorPacienteYRangoDeFechas_DebeRetornarItemsPaginados()
    {
        // Arrange
        await _service.RegistrarPagoAsync(new RegistrarPagoDto(5, null, 1, 1000m, "Efectivo", null));
        await _service.RegistrarPagoAsync(new RegistrarPagoDto(5, null, 2, 2000m, "Tarjeta de Débito", null));

        var filtro = new PagoFiltroDto(
            IdPaciente: 5,
            Desde: DateTime.UtcNow.AddDays(-1),
            Hasta: DateTime.UtcNow.AddDays(1),
            Pagina: 1,
            RegistrosPorPagina: 10
        );

        // Act
        var resultado = await _service.ListarPagosAsync(filtro);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.TotalRegistros);
        Assert.Equal(2, resultado.Items.Count);
        Assert.Equal(1, resultado.PaginaActual);
    }
}

public class FakePagoRepository : IPagoRepository
{
    public List<Pago> Pagos { get; } = new();

    public Task<Pago?> ObtenerPorIdAsync(int idPago, CancellationToken cancellationToken = default)
    {
        var pago = Pagos.FirstOrDefault(p => p.IdPago == idPago);
        return Task.FromResult(pago);
    }

    public Task<Pago?> ObtenerPorIdConDetalleAsync(int idPago, CancellationToken cancellationToken = default)
    {
        var pago = Pagos.FirstOrDefault(p => p.IdPago == idPago);
        return Task.FromResult(pago);
    }

    public Task<(IReadOnlyList<Pago> Items, int Total)> ObtenerFiltradosAsync(
        int? idPaciente,
        byte? idConcepto,
        byte? idEstado,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = Pagos.AsEnumerable();

        if (idPaciente.HasValue)
        {
            query = query.Where(p => p.IdPaciente == idPaciente.Value);
        }

        if (idConcepto.HasValue)
        {
            query = query.Where(p => p.IdConcepto == idConcepto.Value);
        }

        if (idEstado.HasValue)
        {
            query = query.Where(p => p.IdEstado == idEstado.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(p => p.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(p => p.Fecha <= hasta.Value);
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(p => p.Fecha)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Pago>)items, total));
    }

    public Task<Pago> AgregarAsync(Pago pago, CancellationToken cancellationToken = default)
    {
        pago.IdPago = Pagos.Count > 0 ? Pagos.Max(p => p.IdPago) + 1 : 1;
        Pagos.Add(pago);
        return Task.FromResult(pago);
    }
}
