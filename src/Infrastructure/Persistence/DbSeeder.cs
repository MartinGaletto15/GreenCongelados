using Application.Interfaces.Security;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// Carga inicial de datos de demostración. Es idempotente: si la base ya
/// contiene usuarios, no vuelve a insertar nada.
public class DbSeeder
{
    public const string SuperAdminEmail = "superadmin@greencongelados.com";
    public const string SuperAdminPassword = "Super1234";
    public const string UserEmail = "user@greencongelados.com";
    public const string UserPassword = "User1234";
    public const string CadetEmail = "cadet@greencongelados.com";
    public const string CadetPassword = "Cadet1234";
    public const string DemoCouponCode = "BIENVENIDO10";

    private readonly GCContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        GCContext context,
        IPasswordHasher passwordHasher,
        ILogger<DbSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("DbSeeder: la base ya contiene datos, no se ejecuta.");
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var users = CreateUsers();
            var categories = CreateCategories();

            _context.Users.AddRange(users);
            _context.Categories.AddRange(categories);
            _context.ShippingCost.Add(new ShippingCost
            {
                Name = "Envío estándar Rosario",
                Cost = 1500m,
                IsActive = true
            });
            _context.Promotions.Add(new Promotion(
                DemoCouponCode,
                10m,
                DiscountType.Percentage,
                5000m,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddYears(1)));

            var products = CreateProducts();
            _context.Products.AddRange(products);

            await _context.SaveChangesAsync(cancellationToken);

            var demoUser = users.Single(user => user.Email == UserEmail);
            _context.Addresses.Add(new Address
            {
                IdUser = demoUser.IdUser,
                Street = "Córdoba 1234",
                Dpto = "4A",
                References = "Timbre 4A"
            });

            var productCategories = new[]
            {
                new ProductCategory { IdProduct = products[0].IdProduct, IdCategory = categories[0].IdCategory },
                new ProductCategory { IdProduct = products[1].IdProduct, IdCategory = categories[0].IdCategory },
                new ProductCategory { IdProduct = products[2].IdProduct, IdCategory = categories[1].IdCategory },
                new ProductCategory { IdProduct = products[3].IdProduct, IdCategory = categories[1].IdCategory },
                new ProductCategory { IdProduct = products[4].IdProduct, IdCategory = categories[2].IdCategory },
                new ProductCategory { IdProduct = products[5].IdProduct, IdCategory = categories[2].IdCategory },
                new ProductCategory { IdProduct = products[6].IdProduct, IdCategory = categories[3].IdCategory },
                new ProductCategory { IdProduct = products[7].IdProduct, IdCategory = categories[3].IdCategory },
                new ProductCategory { IdProduct = products[8].IdProduct, IdCategory = categories[4].IdCategory },
                new ProductCategory { IdProduct = products[9].IdProduct, IdCategory = categories[4].IdCategory }
            };
            _context.ProductCategories.AddRange(productCategories);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "DbSeeder: datos demo creados (3 usuarios, 5 categorías, 10 productos, envío, dirección y cupón).");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private List<User> CreateUsers()
    {
        return new List<User>
        {
            CreateVerifiedUser("Super", "Admin", SuperAdminEmail, SuperAdminPassword, Role.SUPERADMIN),
            CreateVerifiedUser("Usuario", "Demo", UserEmail, UserPassword, Role.USER),
            CreateVerifiedUser("Cadete", "Demo", CadetEmail, CadetPassword, Role.CADET)
        };
    }

    private User CreateVerifiedUser(string name, string lastName, string email, string password, Role role)
    {
        var user = new User(email, role);
        user.UpdateDetails(name, lastName);
        user.CompleteRegistration(name, lastName, _passwordHasher.Hash(password));
        return user;
    }

    private static List<Category> CreateCategories()
    {
        return new List<Category>
        {
            new() { Name = "Carnes", ImageUrl = "https://images.greencongelados.com/categorias/carnes.jpg" },
            new() { Name = "Pescados y mariscos", ImageUrl = "https://images.greencongelados.com/categorias/pescados.jpg" },
            new() { Name = "Verduras", ImageUrl = "https://images.greencongelados.com/categorias/verduras.jpg" },
            new() { Name = "Papas y snacks", ImageUrl = "https://images.greencongelados.com/categorias/papas.jpg" },
            new() { Name = "Comidas preparadas", ImageUrl = "https://images.greencongelados.com/categorias/preparadas.jpg" }
        };
    }

    private static List<Product> CreateProducts()
    {
        var products = new List<Product>
        {
            new("Milanesas de carne x 1 kg", null, "Milanesas de carne vacuna listas para cocinar.", "Milanesas de carne vacuna rebozadas y congeladas. Ideal para horno o fritura.", 8500m, 20, 1m, 15m),
            new("Hamburguesas de carne x 8 u", null, "Hamburguesas de carne vacuna congeladas.", "Ocho hamburguesas de carne vacuna de 100 g cada una.", 6200m, 15, 0.8m, 10m),
            new("Filet de merluza x 1 kg", null, "Filetes de merluza sin espinas.", "Filet de merluza congelado, listo para horno o plancha.", 9800m, 12, 1m, 20m),
            new("Langostinos pelados x 500 g", null, "Langostinos pelados y congelados.", "Langostinos pelados listos para saltear o usar en recetas.", 12500m, 8, 0.5m, 10m),
            new("Arvejas congeladas x 500 g", null, "Arvejas verdes congeladas.", "Arvejas seleccionadas y congeladas, listas para cocinar.", 3200m, 30, 0.5m, 8m),
            new("Brócoli congelado x 500 g", null, "Flores de brócoli congeladas.", "Brócoli fresco congelado para conservar color y textura.", 3600m, 25, 0.5m, 8m),
            new("Papas noisette x 1 kg", null, "Papas noisette congeladas.", "Papas noisette crocantes, ideales como guarnición.", 4800m, 0, 1m, 12m),
            new("Papas fritas congeladas x 1 kg", null, "Papas fritas congeladas corte clásico.", "Papas fritas prefritas y congeladas para horno o freidora.", 4300m, 20, 1m, 12m),
            new("Pizza de muzzarella congelada", null, "Pizza de muzzarella lista para horno.", "Pizza individual de muzzarella con masa artesanal congelada.", 5900m, 10, 0.45m, 18m),
            new("Empanadas de carne x 6 u", null, "Empanadas de carne cortada a cuchillo.", "Seis empanadas de carne congeladas, listas para horno.", 5200m, 15, 0.6m, 20m)
        };

        // Un producto oculto para validar que el listado público no lo exponga.
        products[9].Hide();

        return products;
    }
}
