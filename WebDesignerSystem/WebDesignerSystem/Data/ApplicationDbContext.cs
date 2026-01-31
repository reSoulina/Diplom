using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Таблицы
        public DbSet<Role> Roles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<ServiceAppointment> ServiceAppointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Переименовываем таблицы Identity
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // Настройка User -> Role (string к int)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка Category
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Настройка Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10,2)");

            // Настройка Order - ClientId теперь string!
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.CurrentStatus)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.CurrentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(10,2)");

            // Настройка OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(10,2)");

            // Настройка OrderStatusHistory - ChangedBy теперь string!
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Status)
                .WithMany(s => s.StatusHistories)
                .HasForeignKey(h => h.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка ServiceAppointment - ClientId теперь string!
            modelBuilder.Entity<ServiceAppointment>()
                .HasOne(sa => sa.Client)
                .WithMany(u => u.Appointments)
                .HasForeignKey(sa => sa.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceAppointment>()
                .HasOne(sa => sa.Service)
                .WithMany()
                .HasForeignKey(sa => sa.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed данные для OrderStatuses
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus
                {
                    Id = 1,
                    Name = "В очереди",
                    Description = "Заказ ожидает обработки",
                    DisplayOrder = 1,
                    Color = "#ffc107"
                },
                new OrderStatus
                {
                    Id = 2,
                    Name = "В процессе",
                    Description = "Заказ в работе",
                    DisplayOrder = 2,
                    Color = "#17a2b8"
                },
                new OrderStatus
                {
                    Id = 3,
                    Name = "Готов",
                    Description = "Заказ выполнен",
                    DisplayOrder = 3,
                    Color = "#28a745"
                },
                new OrderStatus
                {
                    Id = 4,
                    Name = "Отменен",
                    Description = "Заказ отменен",
                    DisplayOrder = 4,
                    Color = "#dc3545"
                }
            );

            // Seed данные для Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Client" },
                new Role { Id = 2, Name = "Designer" }
            );

            // Seed данные для Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Шаблоны сайтов",
                    Description = "Готовые шаблоны для различных типов сайтов"
                },
                new Category
                {
                    Id = 2,
                    Name = "Дизайн логотипов",
                    Description = "Разработка уникальных логотипов"
                },
                new Category
                {
                    Id = 3,
                    Name = "Консультации",
                    Description = "Профессиональные консультации по веб-дизайну"
                }
            );
        }
    }
}