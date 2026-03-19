using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistence
{
    public class GCContext : DbContext
    {
        public GCContext(DbContextOptions<GCContext> options) : base(options)
        {
        }

        // DB SETS
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ShippingCost> ShippingCost { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // OrderItem (Order <-> Product)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.IdOrder)
                .OnDelete(DeleteBehavior.Cascade); // Order CASCADE DELETE
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.IdProduct)
                .OnDelete(DeleteBehavior.Restrict); // Product RESTRICT DELETE

            // M:M RELATIONSHIP CONFIGURATION (COMPOSITE KEYS)
            // ProductCategory (Product <-> Category)
            modelBuilder.Entity<ProductCategory>().HasKey(pc => new { pc.IdCategory, pc.IdProduct });
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category).WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.IdCategory)
                .OnDelete(DeleteBehavior.Cascade); // Category CASCADE DELETE
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product).WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.IdProduct)
                .OnDelete(DeleteBehavior.Cascade); // Product CASCADE DELETE


            // 1:N RELATIONSHIP CONFIGURATION (SIMPLE FOREIGN KEYS)
            // Order to User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User).WithMany(u => u.OrdersPlaced)
                .HasForeignKey(o => o.IdUser)
                .OnDelete(DeleteBehavior.Restrict);

            // Order to Courier
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Courier).WithMany(u => u.OrdersToDeliver)
                .HasForeignKey(o => o.IdCourier)
                .OnDelete(DeleteBehavior.Restrict);

            // Order to Promotion (Optional)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promotion).WithMany(p => p.Orders)
                .HasForeignKey(o => o.IdPromotion)
                .IsRequired(false) // Allows NULL values in the FK
                .OnDelete(DeleteBehavior.SetNull); // Promotion SET NULL DELETE

            // Mapping Enums to String (to store names instead of numbers)
            modelBuilder.Entity<Address>().Property(a => a.City).HasConversion<string>();
            modelBuilder.Entity<Order>().Property(o => o.OrderStatus).HasConversion<string>();
            modelBuilder.Entity<Promotion>().Property(p => p.DiscountType).HasConversion<string>();

            // Cart to User (1:1)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User).WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.IdUser)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem (Cart <-> Product)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart).WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.IdCart)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product).WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.IdProduct)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}