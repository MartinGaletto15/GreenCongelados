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

2.  **Configurar la base de datos:**
    Configura `ConnectionStrings:DbConnectionString` en `src/Web/appsettings.Development.json` con una cadena de conexión válida para tu instancia de SQL Server.

    La API también requiere `JwtSettings:Secret`. Configúrala en el mismo archivo para desarrollo local o mediante la variable de entorno `JwtSettings__Secret`. No guardes secretos reales en el control de versiones.

    Para enviar correos de restablecimiento de contraseña, `SmtpEmailSender` requiere estos valores de configuración:

    | Clave | Descripción |
    | --- | --- |
    | `MailKit:Email` | Cuenta de correo que autentica y envía los mensajes. |
    | `MailKit:PasswordGmail` | Contraseña de aplicación de Gmail; no uses la contraseña habitual de la cuenta. |
    | `MailKit:Server` | Servidor SMTP del proveedor de correo (por ejemplo, `smtp.gmail.com` para Gmail). |
    | `MailKit:Port` | Puerto SMTP; el remitente usa STARTTLS. |

    Para evitar filtrar credenciales, la contraseña de Gmail no debe dejarse en `appsettings*.json` versionados. Lo recomendado es usar User Secrets o variables de entorno:

    ```bash
    dotnet user-secrets set "MailKit:PasswordGmail" "TU_CONTRASEÑA_DE_APLICACION" --project src/Web/Web.csproj
    ```

    O bien:

    ```bash
    setx MailKit__PasswordGmail "TU_CONTRASEÑA_DE_APLICACION"
    ```

    En el caso de variables de entorno, también se admite `MailKit__Email`, `MailKit__Server` y `MailKit__Port`. Si prefieres mantener el desarrollo local más seguro, usa `dotnet user-secrets` y deja `PasswordGmail` vacío o omitido en los archivos de configuración guardados en el repositorio. El código solo leerá la clave `MailKit:PasswordGmail`/`MailKit__PasswordGmail`; un nombre distinto no será interpretado.

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
