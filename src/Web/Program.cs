using Infrastructure.Repositories;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.UserServices;
using Application.Services;
using Application.Interfaces;
using Application.Interfaces.EmailSender;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.EmailVerification;
using Application.Interfaces.Address;
using Application.Interfaces.Categories;
using Application.Interfaces.Order;
using Application.Interfaces.OrderItem;
using Application.Interfaces.ProductCategory;
using Application.Interfaces.Product;
using Application.Interfaces.Promotion;
using Application.Interfaces.ShippingCost;
using Infrastructure.Security;
using Infrastructure.Notifications;
using Application.Interfaces.Security;
using Application.Interfaces.CartItem;
using Application.Interfaces.User;
using Application.Configuration;
using Web.Middlewares;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Render inyecta PORT y espera que la app escuche en 0.0.0.0.
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// Service Configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Respuesta uniforme para errores de DataAnnotations, igual al contrato del middleware.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        string message = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors)
            .Where(error => error.Exception == null && !string.IsNullOrWhiteSpace(error.ErrorMessage))
            .Select(error => error.ErrorMessage)
            .FirstOrDefault()
            ?? "Los datos enviados no son válidos.";

        return new BadRequestObjectResult(new { error = message, code = "VALIDATION_ERROR" });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserTokenProvider, UserTokenProvider>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IUserSessionValidator, UserSessionValidator>();

// Tokens de usuario: expiraciones obligatorias, validadas al arrancar.
builder.Services.AddOptions<UserTokenOptions>()
    .Bind(builder.Configuration.GetSection(UserTokenOptions.SectionName))
    .Validate(o => o.PasswordResetExpirationMinutes > 0, "UserTokens:PasswordResetExpirationMinutes debe configurarse con un valor mayor que cero.")
    .Validate(o => o.EmailConfirmationExpirationMinutes > 0, "UserTokens:EmailConfirmationExpirationMinutes debe configurarse con un valor mayor que cero.")
    .ValidateOnStart();

// Email: sin credenciales SMTP se usa un sender que solo registra el token en el log.
// Esto permite desplegar la demo sin configurar una casilla real.
var smtpSection = builder.Configuration.GetSection(SmtpOptions.SectionName);
var hasSmtpPassword = !string.IsNullOrWhiteSpace(builder.Configuration["MailKit:PasswordGmail"]);

if (hasSmtpPassword)
{
    builder.Services.AddOptions<SmtpOptions>()
        .Bind(smtpSection)
        .Validate(o => !string.IsNullOrWhiteSpace(o.Email), "MailKit:Email es obligatorio.")
        .Validate(o => !string.IsNullOrWhiteSpace(o.Server), "MailKit:Server es obligatorio.")
        .Validate(o => o.Port > 0, "MailKit:Port debe ser mayor que cero.")
        .ValidateOnStart();

    builder.Services.AddScoped<SmtpEmailSender>();
    builder.Services.AddScoped<IEmailSenderService, BestEffortEmailSender>();
}
else
{
    builder.Services.Configure<SmtpOptions>(smtpSection);
    builder.Services.AddScoped<IEmailSenderService, ConsoleEmailSender>();
}

builder.Services.AddScoped<IUserWriteService, UserService>();
builder.Services.AddScoped<IUserReadOnlyService, UserService>();
builder.Services.AddScoped<IAddressReadOnlyService, AddressService>();
builder.Services.AddScoped<IAddressWriteService, AddressService>();
builder.Services.AddScoped<ICategoryReadOnlyService, CategoryService>();
builder.Services.AddScoped<ICategoryWriteService, CategoryService>();
builder.Services.AddScoped<IOrderReadOnlyService, OrderService>();
builder.Services.AddScoped<IOrderWriteService, OrderService>();
builder.Services.AddScoped<IOrderItemReadOnlyService, OrderItemService>();
builder.Services.AddScoped<IOrderItemWriteService, OrderItemService>();
builder.Services.AddScoped<IProductCategoryReadOnlyService, ProductCategoryService>();
builder.Services.AddScoped<IProductCategoryWriteService, ProductCategoryService>();
builder.Services.AddScoped<IProductReadOnlyService, ProductService>();
builder.Services.AddScoped<IProductWriteService, ProductService>();
builder.Services.AddScoped<IPromotionReadOnlyService, PromotionService>();
builder.Services.AddScoped<IPromotionWriteService, PromotionService>();
builder.Services.AddScoped<IShippingCostReadOnlyService, ShippingCostService>();
builder.Services.AddScoped<IShippingCostWriteService, ShippingCostService>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICartItemReadOnlyService, CartItemService>();
builder.Services.AddScoped<ICartItemWriteService, CartItemService>();
builder.Services.AddScoped<ICadetReadOnlyService, CadetService>();
builder.Services.AddScoped<ICadetWriteService, CadetService>();
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

var jwtSecret = builder.Configuration["JwtSettings:Secret"] 
                ?? throw new InvalidOperationException("JwtSettings:Secret no está configurado. Defínelo con User Secrets o la variable de entorno JwtSettings__Secret.");

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:Secret debe tener al menos 32 caracteres.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // true in production
        ValidateAudience = false, // true in production
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var securityStamp = context.Principal?.FindFirstValue(JwtClaimNames.SecurityStamp);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail("El token no identifica a un usuario.");
                return;
            }

            var sessionValidator = context.HttpContext.RequestServices
                .GetRequiredService<IUserSessionValidator>();

            // Se consulta la versión persistida: los tokens emitidos antes de un
            // cambio de contraseña/rol (o de eliminar la cuenta) dejan de valer.
            if (!await sessionValidator.IsTokenValidAsync(userId, securityStamp))
            {
                context.Fail("La sesión expiró. Inicia sesión nuevamente.");
            }
        }
    };
});

builder.Services.AddAuthorization();

// CORS: los orígenes permitidos se configuran en Cors:AllowedOrigins.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AuthDemo", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
    });
});

// Swagger: disponible en todos los entornos cuando Swagger:Enabled es true.
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "GreenCongelados", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the JWT token using: Bearer {token}"
        });


        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
}

var rawConnectionString = builder.Configuration.GetConnectionString("DbConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:DbConnectionString no está configurada. Defínela con User Secrets o la variable de entorno ConnectionStrings__DbConnectionString.");

// Acepta tanto el formato Npgsql (Host=...;Port=...) como la URI que entrega Supabase.
var connectionString = PostgresConnectionString.Normalize(rawConnectionString);

builder.Services.AddDbContext<GCContext>(options => 
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// Middleware Configuration

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AuthDemo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "GreenCongelados API",
    timestamp = DateTime.UtcNow
}));

// Database startup: migraciones controladas por configuración y datos demo
// idempotentes en cada arranque.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GCContext>();

    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        await dbContext.Database.MigrateAsync();
    }

    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.Run();