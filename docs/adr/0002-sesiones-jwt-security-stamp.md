# 0002 — Sesiones JWT de una hora con `security_stamp` persistido

**Decidida:** ~2026-03-21 (JWT) · 2026-10-30 (`security_stamp`)
**Escrita:** 2026-11-24
**Dónde:** `JwtProvider`, `UserSessionValidator`, `User`, `Program.cs`
**Relacionada:** [0001](0001-concurrencia-de-stock.md) — misma idea de fondo: la
base es la que manda.

## Qué pasaba

La API la consume un frontend propio y no guarda sesión en el servidor. Necesitaba
un token que caducara solo, y además que **cambiar la contraseña, cambiar el rol
o desactivar la cuenta cortara las sesiones ya abiertas** sin esperar a que el
token venciera.

El problema es que un JWT firmado es autocontenido: el servidor no recuerda qué
emitió. Sin algo extra, un token robado o emitido antes de un cambio de
contraseña sigue valiendo hasta su vencimiento.

## Qué hice

Access token **JWT firmado con HS256**, sin refresh token, con estos claims:
`NameIdentifier`, nombre, email, rol y `security_stamp`.

- El token dura **1 hora** (hardcodeado en `JwtProvider`).
- `User.SecurityStamp` es un `Guid` persistido que se **rota** al cambiar la
  contraseña, al cambiar el rol y al desactivar la cuenta.
- En cada request autenticado, `OnTokenValidated` compara el claim
  `security_stamp` contra el valor de la base (`UserSessionValidator`). Si no
  coinciden —o el usuario ya no existe— la autenticación falla con "La sesión
  expiró".
- Al vencer el token, el usuario vuelve a iniciar sesión. No hay renovación
  silenciosa.

La autoridad para revocar es la base, no la memoria del proceso: aguanta
reinicios y funciona igual con varias instancias.

## Qué descarté y por qué

- **Solo expiración corta, sin stamp** — cambiar la contraseña no cerraba nada.
- **Sesión en servidor (cookie + store)** — estado e infraestructura extra, y
  encima tendría que resolver CSRF para una API que consume una SPA.
- **Refresh token con rotación** — exige almacenamiento, detección de reuso y
  revocación por dispositivo. No lo hice; queda como mejora futura.
- **Lista negra de tokens (Redis)** — infraestructura nueva para algo que el
  stamp ya resuelve entero. Y soy yo solo manteniendo esto: un Redis más es un
  Redis más que se puede caer un domingo.
- **ASP.NET Identity** — arrastra tablas, flujos y convenciones completas para un
  MVP con autenticación propia.

## Qué me cuesta

**A favor**

- Revocación inmediata al cambiar contraseña, rol o al desactivar la cuenta.
- Sin tabla de sesiones ni refresh tokens que guardar y rotar.
- Aguanta reinicios y múltiples instancias sin hacer nada especial.

**Lo que acepto**

- **Una consulta a la base por cada request autenticado.** Es el precio de la
  revocación inmediata; con el tráfico actual no se nota.
- **Re-login cada hora** para un usuario activo. Lo acepto a cambio de no
  implementar refresh tokens.
- **El rol viaja en el token.** Si lo cambio, el token queda viejo y el usuario
  tiene que volver a entrar para tener los permisos nuevos.
- **No hay revocación individual.** Rotar el stamp cierra todas las sesiones de
  ese usuario, no una sola.
- **`ValidateIssuer` y `ValidateAudience` están en `false`.** Hoy hay un solo
  servicio, así que no aportan nada; el día que haya un gateway o más de un
  servicio validando tokens, hay que activarlos.
- **El secreto es obligatorio al arrancar** (`JwtSettings:Secret`): la API no
  levanta sin él, ni en Development. Prefiero que falle fuerte a arrancar con un
  secreto de mentira.

## Cuándo lo reviso

- El re-login cada hora empieza a molestar a usuarios reales.
- Necesito "cerrar sesión en todos los dispositivos" o revocar un token puntual.
- El roundtrip por request se vuelve notorio (ahí la respuesta es caché del
  stamp o refresh tokens).
- Sumo OAuth, login con terceros o un segundo servicio validando tokens (ahí sí,
  issuer y audience).

## Dónde está

- `src/Infrastructure/Security/JwtProvider.cs`
- `src/Infrastructure/Security/UserSessionValidator.cs`
- `src/Domain/Entities/User.cs` (`UpdatePassword`, `ChangeRole`, `RotateSecurityStamp`)
- `src/Web/Program.cs` (`AddJwtBearer` → `OnTokenValidated`)
- `src/Infrastructure/Migrations/20261030223939_AddUserSecurityStamp.cs`
