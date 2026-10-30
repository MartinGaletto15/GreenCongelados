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

// 1. Service Configuration
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
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(errorMessage => !string.IsNullOrWhiteSpace(errorMessage))
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

// Email: en Development sin credenciales se usa un sender que solo registra el token en el log.
var smtpSection = builder.Configuration.GetSection(SmtpOptions.SectionName);

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddOptions<SmtpOptions>()
        .Bind(smtpSection)
        .Validate(o => !string.IsNullOrWhiteSpace(o.Email), "MailKit:Email es obligatorio.")
        .Validate(o => !string.IsNullOrWhiteSpace(o.PasswordGmail), "MailKit:PasswordGmail es obligatorio.")
        .Validate(o => !string.IsNullOrWhiteSpace(o.Server), "MailKit:Server es obligatorio.")
        .Validate(o => o.Port > 0, "MailKit:Port debe ser mayor que cero.")
        .ValidateOnStart();
}
else
{
    builder.Services.Configure<SmtpOptions>(smtpSection);
}

var hasSmtpPassword = !string.IsNullOrWhiteSpace(builder.Configuration["MailKit:PasswordGmail"]);

if (builder.Environment.IsDevelopment() && !hasSmtpPassword)
{
    builder.Services.AddScoped<IEmailSenderService, ConsoleEmailSender>();
}
else
{
    builder.Services.AddScoped<SmtpEmailSender>();
    builder.Services.AddScoped<IEmailSenderService, BestEffortEmailSender>();
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

// CORS para la demo frontend local (auth-demo).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AuthDemo", policy =>
    {
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

var connectionString = builder.Configuration.GetConnectionString("DbConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:DbConnectionString no está configurada. Defínela con User Secrets o la variable de entorno ConnectionStrings__DbConnectionString.");
builder.Services.AddDbContext<GCContext>(options => 
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// 2. Middleware Configuration

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors("AuthDemo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 

app.MapGet("/", () => "Hello World!"); 

app.Run();