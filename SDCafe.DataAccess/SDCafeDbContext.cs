using Microsoft.EntityFrameworkCore;
using SDCafe.Entities;
using Microsoft.EntityFrameworkCore.Design;

namespace SDCafe.DataAccess
{
    public class SDCafeDbContext : DbContext
    {
        public SDCafeDbContext(DbContextOptions<SDCafeDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(e => e.Category)
                      .WithMany(e => e.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasIndex(e => e.TableNumber).IsUnique();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasOne(e => e.Table)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(e => e.TableId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Waiter)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(e => e.WaiterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(e => e.Order)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.PaymentNumber).IsUnique();
                entity.HasOne(e => e.Order)
                      .WithMany(e => e.Payments)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cashier)
                      .WithMany()
                      .HasForeignKey(e => e.CashierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

    public class SDCafeDbContextFactory : IDesignTimeDbContextFactory<SDCafeDbContext>
    {
        public SDCafeDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SDCafeDbContext>();
            optionsBuilder.UseSqlite("Data Source=../SDCafe.Web/SDCafe.db");
            return new SDCafeDbContext(optionsBuilder.Options);
        }
    }
} 