using Infrastructure.Repositories;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Aplication.Interfaces.UserServices;
using Aplication.Services;
using Aplication.Interfaces;
using Infrastructure.Utilities;
using Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service Configuration (Add Services to the container) ---
// Here you add services like Entity Framework, CORS, Authentication, etc.
builder.Services.AddControllers(); // If using MVC/API controllers
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI
builder.Services.AddSwaggerGen();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserWriteService, UserService>();
builder.Services.AddScoped<IUserReadOnlyService, UserService>();
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

// Database
var connectionString = builder.Configuration.GetConnectionString("DbConnectionString");
builder.Services.AddDbContext<GCContext>(options => 
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// --- 2. Middleware Configuration (Configure the HTTP request pipeline) ---

// Development environment configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
// Controller mapping (if AddControllers was used above)
app.MapControllers(); 

app.MapGet("/", () => "Hello World!"); 

// --- 3. Application Execution ---
app.Run();