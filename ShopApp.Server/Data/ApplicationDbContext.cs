using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;
using ShopApp.Server.Models;
using ShopApp.Server.Shared.Models;

namespace ShopApp.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Настройка для SQLite
            if (Database.IsSqlite())
            {
                foreach (var entity in builder.Model.GetEntityTypes())
                {
                    foreach (var property in entity.GetProperties())
                    {
                        if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                        {
                            property.SetColumnType("TEXT"); // Устанавливаем TEXT для всех строк
                        }
                    }
                }
            }

            // Создание роли Admin при миграции
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2", // Уникальный идентификатор роли
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
            
            builder.Entity<IdentityUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<IdentityUser>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique();
            
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .Property(o => o.ClientId)
                .HasConversion<string>();

            // Настройка связи для Favorites
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка TPT для Product
            builder.Entity<Product>().ToTable("Products");
            builder.Entity<Tool>().ToTable("Tools");
            builder.Entity<Accessory>().ToTable("Accessories");
            builder.Entity<Clothing>().ToTable("Clothing");
            builder.Entity<MasterClass>().ToTable("MasterClasses");
            builder.Entity<Yarn>().ToTable("Yarns");
            builder.Entity<YarnBobbin>().ToTable("YarnBobbins");
        }

        // DbSet для моделей
        public DbSet<Product> Products { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<Accessory> Accessories { get; set; }
        public DbSet<Clothing> Clothing { get; set; }
        public DbSet<MasterClass> MasterClasses { get; set; }
        public DbSet<Yarn> Yarns { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<YarnBobbin> YarnBobbins { get; set; }
    }
}