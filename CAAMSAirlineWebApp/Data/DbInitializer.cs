using CAAMSAirlineWebApp.Models;

namespace CAAMSAirlineWebApp.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Skip if already seeded
            if (context.AppUsers.Any(u => u.Username == "admin"))
                return;

            // Users only — flight data is managed in Azure SQL
            context.AppUsers.AddRange(
                new AppUser { Username = "admin", PasswordHash = "Admin@123", Role = "Admin", FirstName = "Ahmad", LastName = "Razali", DOB = new DateTime(1985, 6, 15), CreatedAt = DateTime.UtcNow },
                new AppUser { Username = "staff", PasswordHash = "Staff@123", Role = "Staff", FirstName = "Sara", LastName = "Hassan", DOB = new DateTime(1992, 8, 10), CreatedAt = DateTime.UtcNow },
                new AppUser { Username = "customer", PasswordHash = "Customer@123", Role = "Customer", FirstName = "John", LastName = "Doe", DOB = new DateTime(1990, 3, 22), CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            context.Staffs.AddRange(
                new Staff { Username = "admin", FirstName = "Ahmad", LastName = "Razali", Role = "Admin" },
                new Staff { Username = "staff", FirstName = "Sara", LastName = "Hassan", Role = "Staff" }
            );
            context.Customers.Add(new Customer
            {
                Username = "customer",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "0123456789",
                DOB = new DateTime(1990, 3, 22)
            });
            context.SaveChanges();
        }
    }
}