# Centro Médico SRP — Sistema de Gestión Clínica y Administrativa

Sistema integral backend diseñado bajo principios de **Clean Architecture** y **Single Responsibility Principle (SRP)** para la administración hospitalaria, gestión de citas médicas, historiales clínicos, médicos, pacientes y facturación.

---

## 🏗️ 1. Arquitectura del Backend

La solución está construida sobre **.NET 8 (C# 12)** y desacoplada en capas independientes con estricta inversión de dependencias:

```
backend/
├── CentroMedico.sln
├── src/
│   ├── CentroMedico.Domain/           # Capa de Dominio (Entidades puras, Enums, Contratos de repositorios, Excepciones de negocio)
│   ├── CentroMedico.Application/      # Capa de Aplicación (Casos de uso, DTOs, Validaciones FluentValidation, ApiResponse, Paginación)
│   ├── CentroMedico.Infrastructure/   # Capa de Infraestructura (EF Core 8, DbContext, Mapeos Fluent API, Repositorios)
│   └── CentroMedico.Api/              # Capa de Presentación / Entrada (Controllers RESTful, Middlewares globales, Swagger)
└── tests/
    └── CentroMedico.UnitTests/        # Pruebas unitarias y de integración (xUnit, FluentValidation.TestHelper)
```

### Reglas de Dependencia:
* **Domain:** No depende de ningún proyecto ni framework externo.
* **Application:** Depende únicamente de `Domain`.
* **Infrastructure:** Implementa los accesos a datos y depende de `Application` y `Domain`.
* **Api:** Entry point HTTP que referencia a `Infrastructure` y `Application` para el pipeline y la inyección de dependencias.

---

## 🛠️ 2. Stack Tecnológico

* **Runtime:** .NET 8 SDK
* **Lenguaje:** C# 12
* **Web Framework:** ASP.NET Core Web API
* **ORM:** Entity Framework Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`)
* **Validación:** FluentValidation (`FluentValidation.AspNetCore`)
* **Documentación Interactiva:** Swagger / OpenAPI v3 (`Swashbuckle.AspNetCore`)
* **Testing:** xUnit v2, Microsoft.NET.Test.Sdk
* **Base de Datos:** Microsoft SQL Server 2022 (Base de datos: `CentroMedico`)

---

## 🗄️ 3. Base de Datos (SQL Server)

El modelo de datos se inicializa a partir del script [`CentroMedico.sql`](./CentroMedico.sql), que implementa:
* **Catálogos Normalizados:** `Pais`, `Especialidad`, `TurnoEstado`, `Concepto`, `PagoEstado`.
* **Entidades Principales:** `Medico`, `MedicoEspecialidad` (N:N), `Paciente`, `PacienteInfo` (1:1 médica), `Turno`, `Historia`, `Pago`.
* **Políticas de Integridad:** Claves foráneas indexadas, constraints únicas (DNI, Matrícula, Nombre de especialidad), auditoría temporal (`fechaAlta`, `fechaModificacion`) y banderas de Soft Delete (`activo`).

---

## 📡 4. Convenciones de API y Estándar de Respuesta

Todas las respuestas de la API están estandarizadas bajo el patrón de envoltorio unificado (**API Envelope Pattern**):

### Respuesta Exitosa (`2xx`):
```json
{
  "success": true,
  "message": "Operación exitosa.",
  "data": { ... },
  "errors": null
}
```

### Respuesta de Error (`4xx` / `5xx`):
Manejadas automáticamente por el `GlobalExceptionHandlerMiddleware` sin exponer trazas internas del servidor ni de la base de datos:
```json
{
  "success": false,
  "message": "Mensaje descriptivo del error o validación.",
  "data": null,
  "errors": [
    {
      "field": "campo",
      "message": "Descripción del error en el campo."
    }
  ]
}
```

---

## 🚀 5. Features Implementadas

### Feature 001: Setup y Arquitectura Base
* [x] Creación de solución `.sln` y los 4 proyectos de capas (.NET 8).
* [x] Referenciamiento estricto entre capas bajo Clean Architecture.
* [x] Instalación y configuración de paquetes NuGet: EF Core SqlServer, FluentValidation, Swashbuckle.
* [x] Creación del modelo unificado de respuesta `ApiResponse<T>` y `ApiErrorItem`.
* [x] Definición de excepciones de dominio: `NotFoundException`, `BusinessRuleException`, `ValidationException`.
* [x] Implementación de `CentroMedicoDbContext` y método de extensión `AddInfrastructureServices`.
* [x] Implementación de `GlobalExceptionHandlerMiddleware` con mapeo de códigos HTTP y logging estructurado.
* [x] Configuración de CORS permitiendo orígenes habituales de frontend (React / Vite: `http://localhost:5173`, `http://localhost:3000`).
* [x] Implementación del endpoint de salud `GET /api/v1/health` con verificación activa de conexión a SQL Server (`CanConnectAsync`).
* [x] Configuración y habilitación de Swagger UI en `/swagger`.

### Feature 002: Catálogos del Sistema
* [x] **Dominio:**
  - Entidades maestras: `Pais`, `Especialidad`, `TurnoEstado`, `Concepto`, `PagoEstado`.
  - Contrato de persistencia desacoplado `ICatalogoRepository`.
* [x] **Infraestructura:**
  - Mapeos Fluent API con tipos de columna nativos (`char(3)`, `varchar`, `identity`, `constraints`).
  - Registro de `DbSet<...>` en `CentroMedicoDbContext`.
  - Implementación optimizada de repositorio con consultas de solo lectura (`AsNoTracking`) en `CatalogoRepository`.
* [x] **Aplicación:**
  - DTOs inmutables: `PaisDto`, `EspecialidadDto`, `CrearEspecialidadDto`, `CatalogoGenericoDto`.
  - Contrato de servicio `ICatalogoService` e implementación `CatalogoService`.
  - Validador `CrearEspecialidadValidator` con FluentValidation (nombre obligatorio, máx 60; descripción opcional máx 200).
  - Regla de negocio de no duplicidad de especialidades médicas (lanza `BusinessRuleException` -> HTTP 400).
* [x] **API y Presentación:**
  - Endpoints RESTful en `CatalogosController` (`/api/v1/catalogos`):
    - `GET /api/v1/catalogos/paises`: Países ordenados alfabéticamente.
    - `GET /api/v1/catalogos/especialidades`: Especialidades médicas (filtro `soloActivas = true` por defecto).
    - `GET /api/v1/catalogos/estados-turno`: Estados para el motor de citas (`Pendiente`, `Realizado`, etc.).
    - `GET /api/v1/catalogos/conceptos-pago`: Conceptos de cobro clínico y laboratorio.
    - `GET /api/v1/catalogos/estados-pago`: Estados de recaudación y facturación.
    - `POST /api/v1/catalogos/especialidades`: Registro de nueva especialidad con respuesta HTTP 201 Created.

### Feature 003: Gestión de Médicos y Especialidades
* [x] **Dominio:**
  - Entidades `Medico` y `MedicoEspecialidad` (relación N:N) con políticas de auditoría y soft delete (`activo`).
  - Contrato `IMedicoRepository` con operaciones de búsqueda con filtros, paginación, unicidad y asignación de especialidades.
* [x] **Infraestructura:**
  - Mapeos Fluent API `MedicoConfiguration` (índices únicos en matrícula y DNI) y `MedicoEspecialidadConfiguration` (clave primaria compuesta `[idMedico, idEspecialidad]`).
  - Registro de `DbSet<Medico>` y `DbSet<MedicoEspecialidad>` en `CentroMedicoDbContext`.
  - Implementación de repositorio `MedicoRepository` con `Include` de especialidades y consultas paginadas optimizadas.
* [x] **Aplicación:**
  - Modelo de paginación reutilizable `PagedResult<T>`.
  - DTOs especializados: `CrearMedicoDto`, `ActualizarMedicoDto`, `MedicoDetalleDto`, `MedicoFiltroDto`, `AsignarEspecialidadDto`, `EspecialidadAsignadaDto`, `CambiarEstadoMedicoDto`.
  - Validadores FluentValidation `CrearMedicoValidator` y `ActualizarMedicoValidator` (obligatoriedad de matrícula, DNI, nombre, apellido y al menos 1 especialidad).
  - Servicio `MedicoService` con aplicación de reglas de negocio duras:
    - Validación de unicidad de matrícula profesional (HTTP 400 `BusinessRuleException`).
    - Validación de unicidad de DNI (HTTP 400 `BusinessRuleException`).
    - Validación de existencia y estado activo de las especialidades médicas asignadas.
    - Política estricta de baja lógica (`activo = false`) sin borrado físico de la historia clínica.
* [x] **API y Presentación:**
  - Endpoints RESTful en `MedicosController` (`/api/v1/medicos`):
    - `GET /api/v1/medicos`: Listado paginado con filtros por texto, especialidad y estado activo.
    - `GET /api/v1/medicos/{id}`: Detalle completo del médico y sus especialidades.
    - `POST /api/v1/medicos`: Registro de médico con especialidades iniciales (HTTP 201 Created).
    - `PUT /api/v1/medicos/{id}`: Actualización de datos personales y de contacto.
    - `PATCH /api/v1/medicos/{id}/estado`: Toggle de estado activo / baja lógica.
    - `DELETE /api/v1/medicos/{id}`: Baja lógica conforme a convención RESTful (HTTP 204 No Content).
    - `PUT /api/v1/medicos/{id}/especialidades`: Reemplazo masivo de especialidades asignadas.
    - `POST /api/v1/medicos/{id}/especialidades`: Asignación de especialidad adicional con validaciones de existencia y estado activo.
    - `DELETE /api/v1/medicos/{id}/especialidades/{idEspecialidad}`: Remoción de especialidad con regla de retención de al menos una especialidad.
* [x] **Pruebas Automatizadas:**
  - Cobertura total de pruebas automáticas (100% aprobadas en xUnit).
  - Verificación de criterios Gherkin para registro exitoso, control de duplicados, asignación/remoción de especialidades y filtros de baja lógica.

### Feature 004: Gestión de Pacientes
* [x] **Dominio:**
  - Entidad `Paciente` con cálculo de propiedad `Edad` a partir de `FNacimiento`, relación con catálogo `Pais` y campos de auditoría.
  - Contrato `IPacienteRepository` con soporte para verificación de DNI, búsqueda por ID, DNI y paginación con filtros.
* [x] **Infraestructura:**
  - Mapeo Fluent API `PacienteConfiguration` con índice único en `dni` (`UQ_Paciente_Dni`), clave foránea restrictiva hacia `Pais` (`FK_Paciente_Pais`) e ignorado de propiedad calculada `Edad`.
  - Registro de `DbSet<Paciente>` en `CentroMedicoDbContext`.
  - Implementación `PacienteRepository` con consultas asíncronas paginadas optimizadas con `AsNoTracking` e `Include(p => p.Pais)`.
* [x] **Aplicación:**
  - DTOs dedicados: `CrearPacienteDto`, `ActualizarPacienteDto`, `PacienteDto`, `PacienteFiltroDto`, `CambiarEstadoPacienteDto`.
  - Validadores FluentValidation:
    - `CrearPacienteValidator`: DNI obligatorio (máx 20), fecha de nacimiento en el pasado, código ISO-3 de país de 3 caracteres, email válido obligatorio (máx 150) y domicilio obligatorio (máx 150).
    - `ActualizarPacienteValidator`: Formato y obligatoriedad en modificaciones.
  - Servicio `PacienteService` con validación de existencia de país en catálogo y unicidad de DNI (HTTP 400 `BusinessRuleException`).
* [x] **API y Presentación:**
  - Endpoints RESTful en `PacientesController` (`/api/v1/pacientes`):
    - `GET /api/v1/pacientes`: Consulta paginada con filtros por texto (`nombre`, `apellido`, `dni`), `idPais` y `soloActivos`.
    - `GET /api/v1/pacientes/{id}`: Detalle del paciente por ID.
    - `GET /api/v1/pacientes/dni/{dni}`: Búsqueda rápida por DNI.
    - `POST /api/v1/pacientes`: Alta de paciente con respuesta HTTP 201 Created y encabezado `Location`.
    - `PUT /api/v1/pacientes/{id}`: Actualización de datos personales y de contacto.
    - `PATCH /api/v1/pacientes/{id}/estado`: Toggle de estado activo / reactivación.
    - `DELETE /api/v1/pacientes/{id}`: Baja lógica conforme al estándar RESTful (HTTP 204 No Content).
* [x] **Pruebas Automatizadas:**
  - Cobertura total de 102 pruebas automáticas en la solución (100% aprobadas en xUnit).
  - Cobertura de criterios Gherkin para registro exitoso, control de DNI duplicado, búsqueda por DNI, validación de país, cálculo de edad y baja lógica.

### Feature 005: Antecedentes Clínicos del Paciente (PacienteInfo)
* [x] **Dominio:**
  - Entidad `PacienteInfo` que modela la ficha clínica base (condición diabética, implantes, alergias, grupo sanguíneo y auditoría `fechaModificacion`) con relación 1 a 1 hacia `Paciente`.
  - Contrato de repositorio desacoplado `IPacienteInfoRepository` con métodos para consulta y persistencia atómica (Upsert).
* [x] **Infraestructura:**
  - Mapeo Fluent API en `PacienteInfoConfiguration` con `idPaciente` como clave primaria y foránea referenciando a `Paciente` con eliminación en cascada (`ON DELETE CASCADE`).
  - Registro de `DbSet<PacienteInfo>` en `CentroMedicoDbContext`.
  - Implementación `PacienteInfoRepository` con lógica de consulta de solo lectura (`AsNoTracking`) y Upsert atómico para inserción o modificación.
* [x] **Aplicación:**
  - DTOs dedicados: `GuardarPacienteInfoDto` y `PacienteInfoDto`.
  - Validador FluentValidation `GuardarPacienteInfoValidator`:
    - Alergias con longitud máxima de 500 caracteres.
    - Grupo sanguíneo validado contra el listado clínico permitido (`A+`, `A-`, `B+`, `B-`, `AB+`, `AB-`, `O+`, `O-` o nulo).
  - Servicio `PacienteInfoService` con validación previa de existencia del paciente, valores por defecto en ficha no inicializada (RF-02) y emisión de `BusinessRuleException` (HTTP 400) ante grupos sanguíneos inválidos.
* [x] **API y Presentación:**
  - Endpoints RESTful integrados en `PacientesController` (`/api/v1/pacientes`):
    - `GET /api/v1/pacientes/{idPaciente}/antecedentes`: Consulta de antecedentes clínicos (retorna valores por defecto si no existen).
    - `PUT /api/v1/pacientes/{idPaciente}/antecedentes`: Creación o actualización (Upsert) de antecedentes con marca temporal UTC.
* [x] **Pruebas Automatizadas:**
  - Cobertura total ampliada a 130 pruebas automáticas en la solución (100% aprobadas en xUnit).
  - Verificación de criterios Gherkin para registro exitoso, control de formato de grupo sanguíneo, manejo de paciente inexistente, valores por defecto y actualización de antecedentes.

### Feature 006: Gestión y Agendamiento de Turnos Médicos
* [x] **Dominio:**
  - Entidad `Turno` que modela citas médicas (`idPaciente`, `idMedico`, `fechaTurno`, `idEstado`, `observacion`, auditoría) con relaciones hacia `Paciente`, `Medico` y `TurnoEstado`.
  - Enum `EstadoTurnoEnum` con estados tipados: `Pendiente (0)`, `Realizado (1)`, `Cancelado (2)`, `Rechazado (3)`, `Postergado (4)`, `Anulado (5)`, `Derivado (6)`.
  - Contrato `ITurnoRepository` con soporte para verificación de solapamiento en agenda médica (`ExisteConflictoHorarioAsync`), detalle con entidades vinculadas, agenda paginada y persistencia.
* [x] **Infraestructura:**
  - Mapeo Fluent API `TurnoConfiguration` con índice compuesto optimizado en `(idMedico, fechaTurno)`, claves foráneas restringidas y valores por defecto SQL (`DF_Turno_Estado`, `SYSDATETIME()`).
  - Registro de `DbSet<Turno>` en `CentroMedicoDbContext`.
  - Implementación `TurnoRepository` con lógica de exclusión de estados cancelados/anulados para conflictos horarios y filtros dinámicos por médico, paciente, fechas y estado.
* [x] **Aplicación:**
  - DTOs dedicados: `AgendarTurnoDto`, `ActualizarEstadoTurnoDto`, `CancelarTurnoDto`, `TurnoDetalleDto` y `TurnoFiltroAgendaDto`.
  - Validadores FluentValidation:
    - `AgendarTurnoValidator`: validación de IDs positivos, fecha requerida y no pasada remota, longitud de observación.
    - `CancelarTurnoValidator`: motivo de cancelación obligatorio (mínimo 5 y máximo 500 caracteres).
    - `ActualizarEstadoTurnoValidator`: estados válidos (rango 0 a 6).
  - Servicio `TurnoService` coordinando reglas de negocio estrictas:
    - Validación de existencia y estado activo obligatorio tanto para el paciente como para el médico (HTTP 400 `BusinessRuleException` o HTTP 404 `NotFoundException`).
    - Regla de conflicto de agenda dura: prevención de citas activas simultáneas para el mismo profesional (HTTP 400).
    - Cancelación de turno con motivo justificado y liberación automática del horario para nuevas reservas.
    - Control de transiciones de estado e imposibilidad de cancelar o modificar citas ya realizadas.
* [x] **API y Presentación:**
  - Endpoints RESTful en `TurnosController` (`/api/v1/turnos`):
    - `GET /api/v1/turnos`: Consulta paginada de agenda con filtros por médico, paciente, rango de fechas y estado.
    - `GET /api/v1/turnos/{id}`: Detalle completo del turno con datos del paciente, médico y estado.
    - `POST /api/v1/turnos`: Agendamiento de nueva cita médica con respuesta HTTP 201 Created y encabezado `Location`.
    - `PATCH /api/v1/turnos/{id}/estado`: Transición controlada del estado del turno.
    - `POST /api/v1/turnos/{id}/cancelar`: Cancelación formal con motivo y liberación horaria.
* [x] **Pruebas Automatizadas:**
  - Cobertura total de 161 pruebas automáticas en la solución (100% aprobadas en xUnit).
  - Verificación completa de criterios Gherkin: agendamiento exitoso en estado pendiente, detección de conflicto de horario en la agenda médica, y cancelación con liberación de horario para nuevas reservas.

### Feature 007: Historia Clínica y Registro de Consultas Médicas
* [x] **Dominio:**
  - Entidad `Historia` que documenta evoluciones médicas (`idPaciente`, `idMedico`, `idTurno`, `fechaHistoria`, `diagnostico`, `observacion`, `fechaAlta`) con relaciones hacia `Paciente`, `Medico` y `Turno`.
  - Contrato `IHistoriaRepository` con métodos para registrar evoluciones, consultar detalle e indexar la línea de tiempo cronológica por paciente.
  - Regla de inmutabilidad clínica: conservación histórica estricta sin borrado físico.
* [x] **Infraestructura:**
  - Mapeo Fluent API `HistoriaConfiguration` con clave primaria identity, restricciones de FK restrictivas, tipos `datetime2(0)`, `varchar(250)`, `varchar(max)` e índice compuesto `(idPaciente, fechaHistoria)` para optimización de consultas temporales.
  - Registro de `DbSet<Historia>` en `CentroMedicoDbContext`.
  - Implementación `HistoriaRepository` con consultas paginadas ordenadas por `FechaHistoria DESC` y filtros por médico, fechas y término de búsqueda.
* [x] **Aplicación:**
  - DTOs dedicados: `CrearHistoriaDto`, `HistoriaDetalleDto` e `HistoriaFiltroDto`.
  - Validador FluentValidation `CrearHistoriaValidator`: obligatoriedad de observación clínica (mínimo 10 caracteres), IDs mayores a cero y longitud máxima de diagnóstico.
  - Servicio `HistoriaService` con lógica transaccional:
    - Validación de existencia y estado activo de paciente y médico.
    - Validación de pertenencia del turno al paciente y médico indicados, impidiendo asociar turnos cancelados o anulados.
    - Transición atómica del estado del turno asociado a `1 (Realizado)`.
    - Consulta de la línea de tiempo completa del paciente con ordenamiento cronológico descendente.
* [x] **API y Presentación:**
  - Endpoints RESTful en `HistoriasClinicasController` (`/api/v1/historias`):
    - `POST /api/v1/historias`: Asienta una nueva evolución médica (HTTP 201 Created con encabezado `Location`) y actualiza el turno a realizado de forma automática.
    - `GET /api/v1/historias/{id}`: Obtiene el detalle de una evolución clínica por su ID.
    - `GET /api/v1/pacientes/{idPaciente}/historia`: Consulta paginada de la historia clínica cronológica de un paciente.
* [x] **Pruebas Automatizadas:**
  - Cobertura total ampliada a 188 pruebas automáticas en la solución (100% aprobadas en xUnit).
  - Verificación completa de criterios Gherkin: registro exitoso de evolución con cambio automático de turno a Realizado (1), y consulta de historial clínico ordenado cronológicamente de la más reciente a la más antigua.

### Feature 008: Gestión de Pagos y Facturación
* [x] **Dominio:**
  - Entidad `Pago` que documenta cobros y transacciones (`idPaciente`, `idTurno`, `idConcepto`, `fecha`, `monto`, `idEstado`, `metodoPago`, `observacion`, `fechaAlta`) con relaciones hacia `Paciente`, `Turno`, `Concepto` y `PagoEstado`.
  - Enum `EstadoPagoEnum` (Pendiente=0, Aprobado=1, Rechazado=2, Reembolsado=3).
  - Contrato `IPagoRepository` con métodos para agregar pagos, consultar por identificador con detalle legible y filtrado avanzado para arqueo de caja.
  - Métodos adicionales en `ICatalogoRepository` para validación y consulta de conceptos de pago (`ObtenerConceptoPorIdAsync`, `ExisteConceptoAsync`).
* [x] **Infraestructura:**
  - Mapeo Fluent API `PagoConfiguration` con clave primaria identity, precisión decimal `decimal(18,2)`, restricciones FK restrictivas, tipos `datetime2(0)`, `varchar(50)`, `varchar(1000)` e índice compuesto `(idPaciente, fecha)` para optimización de consultas.
  - Registro de `DbSet<Pago>` en `CentroMedicoDbContext`.
  - Implementación `PagoRepository` con soporte para consultas de arqueo por rango de fechas, paciente, concepto y estado.
* [x] **Aplicación:**
  - DTOs dedicados: `RegistrarPagoDto`, `PagoDetalleDto`, `ComprobantePagoDto` y `PagoFiltroDto`.
  - Validador FluentValidation `RegistrarPagoValidator`: validación estricta de monto mayor a cero, métodos de pago permitidos (`Efectivo`, `Tarjeta de Débito`, `Tarjeta de Crédito`, `Transferencia Bancaria`, `Mercado Pago / Billetera Virtual`), IDs mayores a cero y longitud máxima de observación.
  - Servicio `PagoService`:
    - Validación de existencia y estado activo del paciente.
    - Validación de existencia del concepto de pago en el catálogo.
    - Validación de turno asociado (pertenencia al paciente, turnos no cancelados ni anulados).
    - Registro de cobro con estado inicial `1 (Aprobado)` y fecha actual.
    - Generación y emisión de comprobantes digitales de pago con desglose fiscal y datos del centro médico.
* [x] **API y Presentación:**
  - Endpoints RESTful en `PagosController` (`/api/v1/pagos`):
    - `POST /api/v1/pagos`: Registra un cobro exitoso (HTTP 201 Created con encabezado `Location`).
    - `GET /api/v1/pagos/{id}`: Obtiene el detalle legible del cobro por su ID.
    - `GET /api/v1/pagos/{id}/comprobante`: Emite el comprobante digital fiscal del pago.
    - `GET /api/v1/pagos`: Consulta paginada con filtros por fecha, concepto, paciente y estado (arqueo de caja).
    - `GET /api/v1/pacientes/{idPaciente}/pagos`: Consulta el historial de pagos de un paciente específico.
* [x] **Pruebas Automatizadas:**
  - Cobertura total ampliada a 236 pruebas automáticas en la solución (100% aprobadas en xUnit).
  - Verificación completa de criterios Gherkin: registro exitoso de cobro en estado Aprobado (1), rechazo con código 400 Bad Request ante montos menores o iguales a cero, validación de métodos de pago y emisión de comprobantes.

### Feature 009: Métricas Operativas y Dashboard Administrativo
* [x] **Aplicación:**
  - DTOs dedicados para analítica en tiempo real: `DashboardResumenDto`, `TurnosPorEspecialidadDto`, `RecaudacionMensualDto` y `TurnoResumenDto`.
  - Contrato de repositorio analítico `IDashboardRepository` y servicio de aplicación `IDashboardService`.
  - Servicio `DashboardService` que gestiona el cálculo de KPIs, agregaciones por mes/año y listado de citas prioritarias para recepción.
* [x] **Infraestructura:**
  - Implementación de alto rendimiento `DashboardRepository` con EF Core ejecutando consultas directas en modo `AsNoTracking()` sin sobrecarga de memoria.
  - Consolidación de KPIs del día (turnos totales, pendientes y realizados) y del mes (facturación acumulada sumando cobros aprobados con estado 1, pacientes activos totales y tasa porcentual de cancelación de citas).
  - Distribución de citas por especialidad médica calculando volumen total y porcentaje de demanda para gráficos en React.
  - Agrupación histórica de recaudación mensual para los 12 meses del año calendario.
  - Consulta de próximas citas del día para la vista rápida del personal de recepción.
* [x] **API y Presentación:**
  - Endpoints RESTful en `DashboardController` (`/api/v1/dashboard`):
    - `GET /api/v1/dashboard/resumen`: Retorna el conjunto consolidado de KPIs ejecutivos y operativos (HTTP 200 OK).
    - `GET /api/v1/dashboard/turnos-por-especialidad`: Retorna la distribución de citas por especialidad para gráficos de barras o pastel (HTTP 200 OK).
    - `GET /api/v1/dashboard/recaudacion-mensual`: Retorna la serie histórica de ingresos mensuales del año (HTTP 200 OK).
    - `GET /api/v1/dashboard/proximos-turnos`: Retorna las próximas citas para el monitor de recepción (HTTP 200 OK).
* [x] **Pruebas Automatizadas:**
  - Cobertura total ampliada a 244 pruebas automáticas en la solución (100% aprobadas en xUnit).
  - Verificación completa de criterios Gherkin: obtención exitosa de métricas ejecutivas calculadas y distribución porcentual de turnos por especialidad.

---

## 💻 6. Cómo Ejecutar el Proyecto y Pruebas

### Prerrequisitos:
1. .NET 8 SDK instalado.
2. Servidor SQL Server corriendo con la base de datos `CentroMedico` creada.
   * Cadena de conexión por defecto configurada en `backend/src/CentroMedico.Api/appsettings.json`:
     ```
     Server=localhost,1433;Database=CentroMedico;User Id=sa;Password=TuPasswordFuerte123!;TrustServerCertificate=True;MultipleActiveResultSets=true
     ```

### Pasos de compilación y ejecución:
```bash
# 1. Posicionarse en la carpeta backend
cd backend

# 2. Restaurar dependencias y compilar
dotnet build

# 3. Ejecutar la suite de pruebas unitarias
dotnet test

# 4. Iniciar la Web API
cd src/CentroMedico.Api
dotnet run --launch-profile http
```

### Endpoints de Verificación:
* **Health Check:** `http://localhost:5235/api/v1/health`
* **Swagger UI:** `http://localhost:5235/swagger`
* **Catálogos:** `http://localhost:5235/api/v1/catalogos`
* **Médicos:** `http://localhost:5235/api/v1/medicos`
* **Pacientes y Antecedentes:** `http://localhost:5235/api/v1/pacientes` (y `/api/v1/pacientes/{id}/antecedentes`)
* **Turnos y Agenda:** `http://localhost:5235/api/v1/turnos`
* **Historia Clínica:** `http://localhost:5235/api/v1/historias` (y `/api/v1/pacientes/{id}/historia`)
* **Pagos y Facturación:** `http://localhost:5235/api/v1/pagos` (y `/api/v1/pagos/{id}/comprobante`, `/api/v1/pacientes/{id}/pagos`)
* **Dashboard y Analítica:** `http://localhost:5235/api/v1/dashboard/resumen` (y `/turnos-por-especialidad`, `/recaudacion-mensual`, `/proximos-turnos`)
