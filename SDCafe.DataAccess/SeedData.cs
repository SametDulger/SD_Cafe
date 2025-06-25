using SDCafe.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SDCafe.DataAccess
{
    public static class SeedData
    {
        public static void Initialize(SDCafeDbContext context)
        {
            if (context.Users.Any())
                return;

            var adminPassword = HashPassword("admin123");
            var users = new List<User>
            {
                new User { FirstName = "Admin", LastName = "User", Email = "admin@sdcafe.com", Password = adminPassword, Role = UserRole.Admin },
                new User { FirstName = "Manager", LastName = "User", Email = "manager@sdcafe.com", Password = adminPassword, Role = UserRole.Manager },
                new User { FirstName = "Waiter", LastName = "User", Email = "waiter@sdcafe.com", Password = adminPassword, Role = UserRole.Waiter },
                new User { FirstName = "Cashier", LastName = "User", Email = "cashier@sdcafe.com", Password = adminPassword, Role = UserRole.Cashier },
                new User { FirstName = "Kitchen", LastName = "Staff", Email = "kitchen@sdcafe.com", Password = adminPassword, Role = UserRole.Kitchen },
                new User { FirstName = "Accounting", LastName = "Staff", Email = "accounting@sdcafe.com", Password = adminPassword, Role = UserRole.Accounting }
            };

            context.Users.AddRange(users);

            var categories = new List<Category>
            {
                new Category { Name = "İçecekler", Description = "Sıcak ve soğuk içecekler" },
                new Category { Name = "Yemekler", Description = "Ana yemekler" },
                new Category { Name = "Tatlılar", Description = "Tatlı çeşitleri" },
                new Category { Name = "Kahvaltı", Description = "Kahvaltı menüsü" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = new List<Product>
            {
                new Product { Name = "Türk Kahvesi", Description = "Geleneksel Türk kahvesi", Price = 15.00m, CategoryId = categories[0].Id },
                new Product { Name = "Espresso", Description = "Tek shot espresso", Price = 12.00m, CategoryId = categories[0].Id },
                new Product { Name = "Latte", Description = "Sütlü kahve", Price = 18.00m, CategoryId = categories[0].Id },
                new Product { Name = "Çay", Description = "Demli çay", Price = 8.00m, CategoryId = categories[0].Id },
                new Product { Name = "Hamburger", Description = "Dana eti hamburger", Price = 45.00m, CategoryId = categories[1].Id },
                new Product { Name = "Pizza", Description = "Margarita pizza", Price = 55.00m, CategoryId = categories[1].Id },
                new Product { Name = "Pasta", Description = "Spaghetti carbonara", Price = 40.00m, CategoryId = categories[1].Id },
                new Product { Name = "Tiramisu", Description = "İtalyan tatlısı", Price = 25.00m, CategoryId = categories[2].Id },
                new Product { Name = "Cheesecake", Description = "New York cheesecake", Price = 22.00m, CategoryId = categories[2].Id },
                new Product { Name = "Kahvaltı Tabağı", Description = "Serpme kahvaltı", Price = 65.00m, CategoryId = categories[3].Id }
            };

            context.Products.AddRange(products);

            var tables = new List<Table>
            {
                new Table { TableNumber = "1", Capacity = 4 },
                new Table { TableNumber = "2", Capacity = 4 },
                new Table { TableNumber = "3", Capacity = 6 },
                new Table { TableNumber = "4", Capacity = 6 },
                new Table { TableNumber = "5", Capacity = 2 },
                new Table { TableNumber = "6", Capacity = 2 },
                new Table { TableNumber = "7", Capacity = 8 },
                new Table { TableNumber = "8", Capacity = 4 }
            };

            context.Tables.AddRange(tables);
            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 