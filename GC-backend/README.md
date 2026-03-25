# Backend

Esta es una API construida con **.NET 8** siguiendo los principios de **Clean Architecture** (Arquitectura Limpia).

## 🚀 Tecnologías y Herramientas

- **Framework:** .NET 8.0
- **Persistencia:** Entity Framework Core con **SQL Server**
- **Seguridad:** BCrypt.Net para hash de contraseñas y JWT para autenticación.
- **Validación:** FluentValidation (para lógica de negocio en la capa de Aplicación).
- **Diseño:** Patrón Repository y Unit of Work.

## 🏗️ Arquitectura del Proyecto

El proyecto está dividido en cuatro capas principales dentro de la carpeta `src/`, siguiendo la metodología de Clean Architecture para asegurar la separación de responsabilidades y la testabilidad:

1.  **Domain (Dominio):** Contiene las entidades, excepciones base, interfaces de repositorios y lógica central del negocio sin dependencias externas.
2.  **Application (Aplicación):** Define los casos de uso (Services), DTOs, validadores y las interfaces necesarias para que la aplicación funcione.
3.  **Infrastructure (Infraestructura):** Implementación de la persistencia de datos (Contexto de base de datos, Migraciones), servicios externos y seguridad.
4.  **Web:** Capa de presentación (API REST). Contiene controladores, middlewares y la configuración del `Program.cs`.

## 🛠️ Configuración y Ejecución

### Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB o Instancia completa)

### Pasos para Empezar

1.  **Clonar el repositorio:**
    ```bash
    git clone [url-del-repo]
    cd GC-backend
    ```

2.  **Configurar la base de datos:**
    Asegúrate de ajustar la cadena de conexión en `src/Web/appsettings.Development.json`.

3.  **Aplicar Migraciones:**
    Ejecuta el siguiente comando para crear las tablas en tu base de datos:
    ```bash
    dotnet ef database update --project src/Infrastructure --startup-project src/Web
    ```

4.  **Ejecutar la API:**
    ```bash
    dotnet run --project src/Web
    ```
    Luego puedes acceder a la documentación interactiva en `/swagger`.

## 📁 Archivos Adicionales

- `GCDiagramDB.drawio`: Diagrama de base de datos actualizable con [draw.io](https://app.diagrams.net/).
