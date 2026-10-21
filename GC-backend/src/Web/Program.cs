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
using Web.Middlewares;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Service Configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationTokenService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

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
                ?? throw new InvalidOperationException("JWT secret key is not configured.");

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
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CineClub API", Version = "v1" });

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

var connectionString = builder.Configuration.GetConnectionString("DbConnectionString");
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 

app.MapGet("/", () => "Hello World!"); 

app.Run();