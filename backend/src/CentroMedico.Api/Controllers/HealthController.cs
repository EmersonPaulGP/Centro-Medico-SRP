using CentroMedico.Application.Common.Models;
using CentroMedico.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly CentroMedicoDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(CentroMedicoDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealthAsync()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();

            if (!canConnect)
            {
                _logger.LogWarning("La comprobación de salud de la base de datos falló: No se pudo conectar.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Fail("No se pudo establecer conexión con la base de datos."));
            }

            var healthData = new
            {
                status = "Healthy",
                database = "Connected",
                timestamp = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Ok(healthData, "Base de datos conectada correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comprobar la salud del servicio y base de datos.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail($"Error al verificar el estado del servicio: {ex.Message}"));
        }
    }
}
