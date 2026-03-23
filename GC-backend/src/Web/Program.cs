using Infrastructure.Repositories;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Aplication.Interfaces.UserServices;
using Aplication.Services;
using Aplication.Interfaces;
using Aplication.Interfaces.Address;
using Aplication.Interfaces.Categories;
using Aplication.Interfaces.Order;
using Aplication.Interfaces.OrderItem;
using Aplication.Interfaces.ProductCategory;
using Aplication.Interfaces.Product;
using Aplication.Interfaces.Promotion;
using Aplication.Interfaces.ShippingCost;
using Infrastructure.Security;
using Aplication.Interfaces.Security;
using Aplication.Interfaces.CartItem;
using Aplication.Interfaces.User;
using Web.Middlewares;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service Configuration (Add Services to the container) ---
// Here you add services like Entity Framework, CORS, Authentication, etc.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }); // If using MVC/API controllers
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
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
                ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");

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
        ValidateIssuer = false, // true en producción
        ValidateAudience = false, // true en producción
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
        Description = "Ingrese el token JWT usando: Bearer {token}"
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

app.UseAuthentication();
app.UseAuthorization();

// Controller mapping (if AddControllers was used above)
app.MapControllers(); 

app.MapGet("/", () => "Hello World!"); 

// --- 3. Application Execution ---
app.Run();