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
│   ├── CentroMedico.Application/      # Capa de Aplicación (Casos de uso, DTOs, Validaciones FluentValidation, ApiResponse)
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
* [x] **Pruebas Automatizadas:**
  - Proyecto de pruebas `CentroMedico.UnitTests` con 16 casos de prueba (100% aprobadas).
  - Validación de escenarios Gherkin para especialidades activas y detección de duplicados.

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
* **Catálogos Base:**
  * `http://localhost:5235/api/v1/catalogos/paises`
  * `http://localhost:5235/api/v1/catalogos/especialidades`
  * `http://localhost:5235/api/v1/catalogos/estados-turno`
  * `http://localhost:5235/api/v1/catalogos/conceptos-pago`
  * `http://localhost:5235/api/v1/catalogos/estados-pago`
