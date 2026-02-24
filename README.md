# 🚀 Gateway.Api -- API Gateway con YARP + JWT + Identity

## 📌 Descripción General

`Gateway.Api` es un API Gateway desarrollado en **.NET 8** utilizando:

-   ✅ YARP (Yet Another Reverse Proxy)
-   ✅ ASP.NET Core Identity
-   ✅ JWT para autenticación
-   ✅ EF Core con soporte PostgreSQL / SQLite
-   ✅ Unit of Work + Repository (Ardalis.Specification)
-   ✅ Auditoría completa de transacciones

------------------------------------------------------------------------

# 🏗 Arquitectura del Proyecto

## Proyectos

### Gateway.Api

-   Endpoints
-   Middleware
-   JWT
-   YARP
-   Swagger

### Gateway.Infrastructure

-   DbContext
-   Switch Postgres / SQLite
-   Identity configuration
-   Repository + UnitOfWork

### Gateway.Domain

-   Entidades del dominio
-   GatewayTransactionLog

------------------------------------------------------------------------

# 🔐 Autenticación

## Flujo

1.  POST /auth/token
2.  Validación con Identity
3.  Generación de JWT con roles
4.  Uso de Authorization: Bearer `<token>`{=html}

------------------------------------------------------------------------

## 🖥️ Frontend – Admin Console (SecureGate)

El proyecto incluye una consola administrativa web desarrollada en **ASP.NET Core MVC** para la gestión de identidad y autorización del gateway.

### 👥 Gestión de Usuarios
- Creación de usuarios con email y contraseña.
- Eliminación de usuarios.
- Visualización de identificador único (User ID).
- Interfaz profesional basada en modales y validaciones integradas.

### 🛡️ Gestión de Roles
- Creación y administración de roles del sistema.
- Visualización centralizada de roles disponibles.
- Asociación dinámica entre usuarios y roles.

### 🔐 Asignación de Roles
- Administración de roles desde un modal dedicado por usuario.
- Visualización clara de roles actuales mediante *badges*.
- Asignación de nuevos roles desde lista filtrada (solo roles no asignados).
- Eliminación de roles mediante acción directa sobre cada badge.
- Regla de negocio aplicada en UI: **todo usuario debe tener al menos un rol asignado** (no se permite eliminar el último rol).

### 🎨 Enfoque UX/UI
- Diseño consistente con tema oscuro profesional.
- Uso de modales para evitar navegación innecesaria.
- Prevención de estados inválidos mediante validaciones visuales.
- Aplicación del principio de menor privilegio como recomendación contextual.

------------------------------------------------------------------------

# 👤 Usuarios y Roles

Se utiliza ASP.NET Core Identity:

Tablas creadas automáticamente: - AspNetUsers - AspNetRoles -
AspNetUserRoles - etc.

------------------------------------------------------------------------

# 🌐 Reverse Proxy (YARP)

Configuración vía reverseproxy.json.

Ejemplo:

``` json
{
  "ReverseProxy": {
    "Routes": {
      "apicertificado_all": {
        "ClusterId": "apicertificado_cluster",
        "Match": {
          "Path": "/internal/apicertificado/{**catch-all}"
        },
        "AuthorizationPolicy": "ProxyPolicy",
        "Transforms": [
          { "PathPattern": "/{**catch-all}" }
        ]
      }
    },
    "Clusters": {
      "apicertificado_cluster": {
        "Destinations": {
          "d1": {
            "Address": "http://172.20.1.21:9534/"
          }
        }
      }
    }
  }
}
```

------------------------------------------------------------------------

# 📊 Auditoría

Se registra:

-   UserId
-   Username
-   RequestId
-   CorrelationId
-   HTTP Method
-   Path + Query
-   RouteId
-   ClusterId
-   Destination
-   StatusCode
-   DurationMs
-   ErrorCode

Incluye llamadas a /auth/token.

------------------------------------------------------------------------

# 🗄 Base de Datos

Switch dinámico en appsettings:

``` json
"Database": {
  "Provider": "Postgres",
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=gatewaydb;Username=gateway;Password=secret"
  },
  "Sqlite": {
    "ConnectionString": "Data Source=./data/gateway.db"
  }
}
```

------------------------------------------------------------------------

# 🧪 Migraciones

Crear migración:

dotnet ef migrations add InitialIdentityAndLogs --project
./Gateway.Infrastructure/Gateway.Infrastructure.csproj --startup-project
./Gateway.Api/Gateway.Api.csproj --output-dir Database/Migrations

Actualizar DB:

dotnet ef database update --project
./Gateway.Infrastructure/Gateway.Infrastructure.csproj --startup-project
./Gateway.Api/Gateway.Api.csproj

------------------------------------------------------------------------

# 📈 Funcionalidades MVP

-   Autenticación JWT
-   Identity
-   Roles
-   Proxy configurable
-   Switch DB
-   UnitOfWork
-   Auditoría

------------------------------------------------------------------------

# 🔮 Futuro

-   Refresh tokens
-   OAuth externo
-   Permisos por rol
-   Rate limiting
-   Circuit breaker
-   Métricas
-   Health checks avanzados

------------------------------------------------------------------------

# 🧭 Flujo

Cliente → Gateway → YARP → API Interna → Log → Respuesta

------------------------------------------------------------------------

Proyecto listo como base sólida para evolucionar a entorno productivo.
