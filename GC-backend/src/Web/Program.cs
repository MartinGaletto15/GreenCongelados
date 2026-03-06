using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuración de Servicios (Add Services to the container) ---
// Aquí agregas servicios como Entity Framework, CORS, Autenticación, etc.
builder.Services.AddControllers(); // Si usas controladores MVC/API
builder.Services.AddEndpointsApiExplorer(); // Para Swagger/OpenAPI
builder.Services.AddSwaggerGen();

// Base de datos
var connectionString = builder.Configuration.GetConnectionString("DbConnectionString");
builder.Services.AddDbContext<GCContext>(options => 
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// --- 2. Configuración del Middleware (Configure the HTTP request pipeline) ---

// Configuración de entorno de desarrollo (Development environment)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeo de Controladores (si usaste AddControllers arriba)
app.MapControllers(); 

app.MapGet("/", () => "Hello World!"); 

// --- 3. Ejecución de la aplicación ---
app.Run();