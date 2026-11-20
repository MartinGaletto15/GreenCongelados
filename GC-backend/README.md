# GC-backend — API de GreenCongelados

API REST en **.NET 8** para la gestión de productos congelados: catálogo, categorías, carrito, pedidos, promociones, costos de envío, usuarios y logística de cadetes.

Este README corresponde al módulo `GC-backend` del repositorio [GreenCongelados](https://github.com/MartinGaletto15/GreenCongelados). El [README principal](../README.md) contiene la descripción general del proyecto.

## 📌 Estado actual

El backend está en **desarrollo activo (MVP)**. Ya cubre:

- Registro con confirmación por email y login con JWT.
- Restablecimiento de contraseña con token de un solo uso.
- Roles `SUPERADMIN`, `ADMIN`, `USER` y `CADET`.
- Catálogo de productos y categorías.
- Carrito por usuario.
- Creación y cancelación de pedidos, con descuento por cupón.
- Asignación de pedidos a cadetes y actualización del estado logístico.
- Direcciones y costos de envío.

**Fuera del alcance actual:** pagos online, subida de imágenes (solo se guarda `UrlImagePrimary`), notificaciones push, tests automatizados, CI y Docker.

> El proyecto no está listo para producción tal como está. Antes de usarlo con usuarios reales hay que resolver concurrencia de stock, idempotencia de pedidos, rate limiting, entrega confiable de emails y observabilidad.

## 🚀 Tecnologías

- **Framework:** .NET 8.0
- **Persistencia:** Entity Framework Core 8 + SQL Server
- **Seguridad:** JWT Bearer + BCrypt.Net-Next
- **Tokens de usuario:** generación criptográfica, hash SHA-256 en base de datos y expiración configurable
- **Email:** MailKit (SMTP); en Development se usa un sender de consola si no hay contraseña SMTP
- **Validación:** Data Annotations en DTOs/entidades, validadores en `Application` y reglas en el dominio
- **Diseño:** Repository + Unit of Work, con separación de servicios de lectura/escritura
- **Documentación:** Swagger/OpenAPI habilitado solo en `Development`

## 🏗️ Arquitectura

| Capa | Responsabilidad |
| --- | --- |
| `Domain` | Entidades, enums, excepciones e interfaces de repositorio. Contiene las reglas centrales de negocio y no referencia otros proyectos de la solución. |
| `Application` | Casos de uso (`Services`), DTOs, validadores e interfaces de servicios externos. |
| `Infrastructure` | `GCContext`, migraciones, repositorios, Unit of Work, seguridad (JWT, BCrypt, tokens) y envío de emails. |
| `Web` | API REST: controllers, middleware global de errores, configuración de DI, autenticación y Swagger. |

La solución **no incluye proyectos de pruebas automatizadas ni pipeline de CI**.

## 🧩 Modelo funcional

- **Roles:** jerarquía de administración `SUPERADMIN` > `ADMIN` > `USER`; `CADET` es un rol operativo de logística. La jerarquía se valida al cambiar roles.
- **Productos:** `Available`, `OutOfStock`, `Hidden`, `Deleted`. Disponible/agotado se deriva del stock; `Hidden` y `Deleted` son decisiones explícitas.
- **Pedidos:** `LogisticsPending`, `InDelivery`, `Delivered`, `Cancelled`. La cancelación restaura stock.
- **Promociones:** `Percentage`, `FixedAmount`, `FreeShipping`, con monto mínimo y vigencia.
- **Sesiones:** JWT de 1 hora con `security_stamp` persistido; cambiar contraseña, rol o desactivar la cuenta invalida los tokens anteriores.

## 🔌 API

La API expone endpoints bajo `/api`. Los grupos principales son:

| Grupo | Ruta base | Acceso |
| --- | --- | --- |
| Autenticación | `/api/auth` | Público: registro, login, forgot/reset password, verificación y reenvío de email. |
| Usuarios | `/api/users` | Autenticado; listado y cambio de rol solo admin/superadmin; rutas de cadetes para cadetes y admin. |
| Productos | `/api/products` | Lectura pública; escritura solo admin/superadmin. |
| Categorías | `/api/categories` | Lectura pública; escritura solo admin/superadmin. |
| Carrito | `/api/cart/items` | Autenticado; cada usuario opera sobre su propio carrito. |
| Pedidos | `/api/orders` | Autenticado; el usuario ve los propios, admin ve todos y cambia estados; cadete actualiza sus pedidos asignados. |
| Promociones | `/api/promotions` | Lectura pública; escritura solo admin/superadmin. |
| Costos de envío | `/api/shipping-cost` | Lectura pública; escritura solo admin/superadmin. |
| Direcciones | `/api/addresses` | Dueño autenticado; lectura global solo superadmin/admin según endpoint. |

`GET /` devuelve un mensaje simple de estado de la API.

## 🛠️ Configuración y ejecución

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB o instancia completa)
- `dotnet-ef` 8.0.0:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

### 1. Clonar

El repositorio es un monorepo; el backend está en `GC-backend/`:

```bash
git clone https://github.com/MartinGaletto15/GreenCongelados.git
cd GC-backend
```

### 2. Configurar secretos

La API lee `appsettings.json`, `appsettings.Development.json`, User Secrets (solo en `Development`) y variables de entorno. Los valores sensibles se configuran con User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DbConnectionString" "Server=localhost;Database=GreenCongelados;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;" --project src/Web/Web.csproj
dotnet user-secrets set "JwtSettings:Secret" "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES" --project src/Web/Web.csproj
dotnet user-secrets set "MailKit:PasswordGmail" "TU_CONTRASEÑA_DE_APLICACION" --project src/Web/Web.csproj
```

En producción usá las variables equivalentes: `ConnectionStrings__DbConnectionString`, `JwtSettings__Secret` y `MailKit__PasswordGmail`. Generá una clave JWT nueva; no reutilices valores que hayan estado versionados en el repositorio.

La aplicación valida al arrancar que existan la connection string y el secreto JWT, y que las expiraciones de `UserTokens` sean mayores que cero. **El código no valida la longitud del secreto JWT**: usá al menos 32 caracteres.

`appsettings.json` define los valores no secretos de los tokens:

| Clave | Descripción |
| --- | --- |
| `UserTokens:PasswordResetExpirationMinutes` | Vigencia del token de restablecimiento, en minutos (por defecto `15`). Entero mayor que cero. |
| `UserTokens:EmailConfirmationExpirationMinutes` | Vigencia del token de confirmación, en minutos (por defecto `1440`). Entero mayor que cero. |

### 3. Configurar email

| Clave | Descripción |
| --- | --- |
| `MailKit:Email` | Cuenta que autentica y envía los correos. |
| `MailKit:PasswordGmail` | Contraseña de aplicación de Gmail; no uses la contraseña habitual de la cuenta. |
| `MailKit:Server` | Servidor SMTP (por ejemplo, `smtp.gmail.com`). |
| `MailKit:Port` | Puerto SMTP; el sender usa STARTTLS. |
| `MailKit:TimeoutSeconds` | Opcional; timeout de conexión/envío en segundos (por defecto `15`). |

`appsettings.Development.json` ya define `Email`, `Server` y `Port`; solo falta la contraseña. Si en `Development` no configurás `MailKit:PasswordGmail`, se usa `ConsoleEmailSender`: no se envían correos reales y el token aparece en el log de la aplicación. En `Production` la configuración SMTP es obligatoria y la API falla al arrancar si falta.

Todas las claves de `MailKit` se pueden sobrescribir con variables de entorno usando `__` (`MailKit__Email`, `MailKit__Server`, `MailKit__Port`, `MailKit__PasswordGmail`). La contraseña debe llamarse exactamente `MailKit:PasswordGmail`; el código no lee otro nombre.

### 4. Aplicar migraciones

Desde `GC-backend/`:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

### 5. Ejecutar

```bash
dotnet run --project src/Web
```

Con el perfil `http`, la API queda en `http://localhost:5027` y Swagger en `http://localhost:5027/swagger`. Swagger solo está habilitado en `Development`.

### 6. Verificación rápida

```bash
dotnet build GC-backend.sln
```

Con la API levantada:

```bash
curl http://localhost:5027/
```

Debe devolver `API de GreenCongelados en funcionamiento.`

## 🌐 CORS

La política CORS `AuthDemo` se aplica en todos los entornos y permite solo `http://localhost:5500` y `http://127.0.0.1:5500` (demo del frontend). Si el frontend corre en otro origen, agregalo en `Program.cs`.

## 📁 Archivos adicionales

- `DER.drawio`: diagrama entidad-relación actualizable con [draw.io](https://app.diagrams.net/).
- `GC-backend.sln`: solución con los cuatro proyectos.

## 🧪 Testing

No hay tests automatizados en este repositorio. Para una contribución, se recomienda agregar:

- Tests unitarios de reglas de dominio (stock, estados de pedido, promociones).
- Tests de integración de autenticación, carrito y pedidos.
- Tests de concurrencia para creación/cancelación de pedidos y consumo de tokens.
- CI con build, tests y auditoría de dependencias.

## ⚠️ Limitaciones conocidas

- No hay pasarela de pagos ni estado de pago en `Order`.
- No hay subida de archivos; las imágenes se referencian por URL.
- No hay outbox/cola: el envío SMTP es directo y best-effort.
- No hay paginación ni filtros en los listados.
- No hay rate limiting/lockout en login, forgot-password o reenvío de email.
- El access token dura 1 hora y no hay refresh token.
- No hay Dockerfile, pipeline de deploy ni health checks.
