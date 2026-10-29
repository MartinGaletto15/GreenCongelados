# Backend

Esta es una API construida con **.NET 8** siguiendo los principios de **Clean Architecture** (Arquitectura Limpia).

## 🚀 Tecnologías y Herramientas

- **Framework:** .NET 8.0
- **Persistencia:** Entity Framework Core con **SQL Server**
- **Seguridad:** BCrypt.Net para hash de contraseñas y JWT para autenticación.
- **Validación:** Data Annotations en algunas entidades y validaciones propias en la capa `Application`.
- **Diseño:** Patrón Repository y Unit of Work.

## 🏗️ Arquitectura del Proyecto

El proyecto está dividido en cuatro capas dentro de `src/`, siguiendo los principios de Clean Architecture para separar responsabilidades:

1.  **Domain (Dominio):** Contiene las entidades, excepciones base, interfaces de repositorios y lógica central del negocio sin dependencias externas.
2.  **Application (Aplicación):** Define los casos de uso (Services), DTOs, validadores y las interfaces necesarias para que la aplicación funcione.
3.  **Infrastructure (Infraestructura):** Implementación de la persistencia de datos (Contexto de base de datos, Migraciones), servicios externos y seguridad.
4.  **Web:** Capa de presentación (API REST). Contiene controladores, middlewares y la configuración del `Program.cs`.

La solución actualmente no incluye proyectos de pruebas automatizadas.

## 🛠️ Configuración y Ejecución

### Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB o Instancia completa)
- Herramienta de línea de comandos `dotnet-ef` 8.0.0. Si no está instalada:
  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.0
  ```

### Pasos para Empezar

1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/MartinGaletto15/GreenCongelados.git
    cd GC-backend
    ```

2.  **Configurar la configuración local y los secretos:**
    La API lee la configuración de `appsettings.json`, `appsettings.Development.json`, User Secrets (solo en `Development`) y variables de entorno. Los valores sensibles o específicos de tu máquina (`ConnectionStrings:DbConnectionString`, `JwtSettings:Secret` y `MailKit:PasswordGmail`) se configuran con User Secrets y no se versionan:

    ```bash
    dotnet user-secrets set "ConnectionStrings:DbConnectionString" "Server=localhost;Database=GreenCongelados;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;" --project src/Web/Web.csproj
    dotnet user-secrets set "JwtSettings:Secret" "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES" --project src/Web/Web.csproj
    dotnet user-secrets set "MailKit:PasswordGmail" "TU_CONTRASEÑA_DE_APLICACION" --project src/Web/Web.csproj
    ```

    En producción usa las variables de entorno equivalentes (`ConnectionStrings__DbConnectionString`, `JwtSettings__Secret`, `MailKit__PasswordGmail`). Genera una clave JWT nueva: no reutilices valores que hayan estado versionados en el repositorio.

    `appsettings.json` define los valores por defecto (no secretos) de `UserTokens`, que controla la vigencia de los tokens de usuario:

    | Clave | Descripción |
    | --- | --- |
    | `UserTokens:PasswordResetExpirationMinutes` | Vigencia del token de restablecimiento de contraseña, en minutos. Entero mayor que cero (por defecto `15`). |
    | `UserTokens:EmailConfirmationExpirationMinutes` | Vigencia del token de confirmación de email, en minutos. Entero mayor que cero (por defecto `1440`). |

    La API valida `UserTokens:*`, `ConnectionStrings:DbConnectionString` y `JwtSettings:Secret` al arrancar: si falta alguno o la expiración no es mayor que cero, la aplicación falla con un mensaje que indica la clave a corregir. También se pueden sobrescribir con variables de entorno (`UserTokens__PasswordResetExpirationMinutes`, etc.).

    Para enviar correos de restablecimiento de contraseña, `SmtpEmailSender` requiere estos valores de configuración:

    | Clave | Descripción |
    | --- | --- |
    | `MailKit:Email` | Cuenta de correo que autentica y envía los mensajes. |
    | `MailKit:PasswordGmail` | Contraseña de aplicación de Gmail; no uses la contraseña habitual de la cuenta. |
    | `MailKit:Server` | Servidor SMTP del proveedor de correo (por ejemplo, `smtp.gmail.com` para Gmail). |
    | `MailKit:Port` | Puerto SMTP; el remitente usa STARTTLS. |
    | `MailKit:TimeoutSeconds` | Opcional. Timeout de la conexión/envío SMTP en segundos (por defecto `15`). |

    Si ejecutas la API en `Development` sin `MailKit:PasswordGmail`, se usa un sender de consola: no se envían correos reales, pero el token de confirmación o restablecimiento aparece en el log de la aplicación. Así puedes probar el flujo completo sin configurar Gmail. En `Production` la configuración SMTP es obligatoria y la API falla al arrancar si falta.

    En el caso de variables de entorno, también se admite `MailKit__Email`, `MailKit__Server` y `MailKit__Port`. El código solo leerá la clave `MailKit:PasswordGmail`/`MailKit__PasswordGmail`; un nombre distinto no será interpretado.

3.  **Aplicar Migraciones:**
    Ejecuta el siguiente comando para crear las tablas en tu base de datos:
    ```bash
    dotnet ef database update --project src/Infrastructure --startup-project src/Web
    ```

4.  **Ejecutar la API:**
    ```bash
    dotnet run --project src/Web
    ```
    Con el perfil de desarrollo (`Development`), puedes acceder a la documentación interactiva en `http://localhost:5027/swagger`. Swagger no está habilitado en otros entornos.

## 📁 Archivos Adicionales

- `GCDiagramDB.drawio`: Diagrama de base de datos actualizable con [draw.io](https://app.diagrams.net/).
