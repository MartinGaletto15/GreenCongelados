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
        public DbSet<Direction> Directions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductPromotion> ProductPromotions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURACIÓN DE RELACIONES M:M (CLAVES COMPUESTAS)
            // OrderDetail (Order <-> Product)
            modelBuilder.Entity<OrderDetail>().HasKey(od => new { od.IdOrder, od.IdProduct });
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order).WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.IdOrder)
                .OnDelete(DeleteBehavior.Cascade); // Order CASCADE DELETE
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.IdProduct)
                .OnDelete(DeleteBehavior.Restrict); // Product RESTRICT DELETE

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


            // CONFIGURACIÓN DE RELACIONES 1:N (CLAVES FORÁNEAS SIMPLES)
            // Order a User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User).WithMany(u => u.Orders)
                .HasForeignKey(o => o.IdUser)
                .OnDelete(DeleteBehavior.Restrict);

            // Order a Direction
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Direction).WithMany(d => d.Orders)
                .HasForeignKey(o => o.IdDirection)
                .OnDelete(DeleteBehavior.Restrict);

            // Order a Payment
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment).WithMany()
                .HasForeignKey(o => o.IdPayment)
                .OnDelete(DeleteBehavior.Restrict);

            // Order a Promotion (Opcional)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promotion).WithMany(p => p.Orders)
                .HasForeignKey(o => o.IdPromotion)
                .IsRequired(false) // Permite valores NULL en la FK
                .OnDelete(DeleteBehavior.SetNull); // Promotion SET NULL DELETE

            // ProductPromotion a Product
            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Product).WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.IdProduct)
                .OnDelete(DeleteBehavior.Cascade); // Product CASCADE DELETE

            // Mapeo de Enums a String (para almacenar nombres en lugar de números)
            modelBuilder.Entity<Direction>().Property(d => d.City).HasConversion<string>();
            modelBuilder.Entity<Order>().Property(o => o.OrderStatus).HasConversion<string>();
            modelBuilder.Entity<Promotion>().Property(p => p.DiscountType).HasConversion<string>();
            modelBuilder.Entity<ProductPromotion>().Property(pp => pp.DiscountType).HasConversion<string>();
        }
    }
}