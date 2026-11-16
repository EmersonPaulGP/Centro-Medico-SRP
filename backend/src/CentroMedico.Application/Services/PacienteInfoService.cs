using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para la consulta y registro/actualización de antecedentes clínicos.
/// </summary>
public class PacienteInfoService : IPacienteInfoService
{
    private readonly IPacienteInfoRepository _pacienteInfoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IValidator<GuardarPacienteInfoDto> _validator;

    public PacienteInfoService(
        IPacienteInfoRepository pacienteInfoRepository,
        IPacienteRepository pacienteRepository,
        IValidator<GuardarPacienteInfoDto> validator)
    {
        _pacienteInfoRepository = pacienteInfoRepository;
        _pacienteRepository = pacienteRepository;
        _validator = validator;
    }

    public async Task<PacienteInfoDto> ObtenerPorPacienteIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        // 1. Verificar existencia del paciente
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente, cancellationToken);
        if (paciente == null)
        {
            throw new NotFoundException($"No se encontró ningún paciente con el ID '{idPaciente}'");
        }

        // 2. Consultar antecedentes clínicos cargados
        var info = await _pacienteInfoRepository.ObtenerPorPacienteIdAsync(idPaciente, cancellationToken);
        if (info == null)
        {
            // RF-02: Valores por defecto si aún no han sido registrados
            return new PacienteInfoDto(
                idPaciente,
                Diabetico: false,
                Implantes: false,
                Alergias: null,
                GrupoSanguineo: null,
                FechaModificacion: null
            );
        }

        return new PacienteInfoDto(
            info.IdPaciente,
            info.Diabetico,
            info.Implantes,
            info.Alergias,
            info.GrupoSanguineo,
            info.FechaModificacion
        );
    }

    public async Task<PacienteInfoDto> GuardarOActualizarAsync(
        int idPaciente,
        GuardarPacienteInfoDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Validación de reglas de entrada y formato
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorGrupo = validationResult.Errors.FirstOrDefault(e => e.PropertyName == nameof(dto.GrupoSanguineo));
            if (errorGrupo != null)
            {
                throw new BusinessRuleException(errorGrupo.ErrorMessage);
            }

            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Regla de negocio: verificar existencia previa del paciente
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente, cancellationToken);
        if (paciente == null)
        {
            throw new NotFoundException($"No se encontró ningún paciente con el ID '{idPaciente}'");
        }

        // 3. Normalización y asignación de marca temporal (RF-05)
        var fechaActual = DateTime.UtcNow;
        var grupoNormalizado = string.IsNullOrWhiteSpace(dto.GrupoSanguineo)
            ? null
            : dto.GrupoSanguineo.Trim().ToUpperInvariant();
        var alergiasNormalizadas = string.IsNullOrWhiteSpace(dto.Alergias)
            ? null
            : dto.Alergias.Trim();

        var entidad = new PacienteInfo
        {
            IdPaciente = idPaciente,
            Diabetico = dto.Diabetico,
            Implantes = dto.Implantes,
            Alergias = alergiasNormalizadas,
            GrupoSanguineo = grupoNormalizado,
            FechaModificacion = fechaActual
        };

        // 4. Persistencia mediante lógica de Upsert
        await _pacienteInfoRepository.GuardarOActualizarAsync(entidad, cancellationToken);

        return new PacienteInfoDto(
            idPaciente,
            dto.Diabetico,
            dto.Implantes,
            alergiasNormalizadas,
            grupoNormalizado,
            fechaActual
        );
    }
}
