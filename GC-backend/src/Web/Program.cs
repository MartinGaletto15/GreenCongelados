using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service Configuration (Add Services to the container) ---
// Here you add services like Entity Framework, CORS, Authentication, etc.
builder.Services.AddControllers(); // If using MVC/API controllers
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI
builder.Services.AddSwaggerGen();

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

// Controller mapping (if AddControllers was used above)
app.MapControllers(); 

app.MapGet("/", () => "Hello World!"); 

// --- 3. Application Execution ---
app.Run();