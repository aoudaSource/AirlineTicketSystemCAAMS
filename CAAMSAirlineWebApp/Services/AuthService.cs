using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.DTOs;
using CAAMSAirlineWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> ValidateUserAsync(string username, string password)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            
            if (user.PasswordHash != password)
                return null;

            return user;
        }

        public async Task<(AppUser? User, string? Error)> RegisterCustomerAsync(RegisterRequest request)
        {
            bool usernameTaken = await _context.AppUsers.AnyAsync(u => u.Username == request.Username);
            if (usernameTaken)
                return (null, "Username is already taken.");

            bool emailTaken = await _context.Customers.AnyAsync(c => c.Email == request.Email);
            if (emailTaken)
                return (null, "An account with that email already exists.");

            var appUser = new AppUser
            {
                Username = request.Username,
                PasswordHash = request.Password, 
                Role = "Customer",
                FirstName = request.FirstName,
                LastName = request.LastName,
                DOB = request.DOB!.Value,
                CreatedAt = DateTime.UtcNow
            };

            var customer = new Customer
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                DOB = request.DOB!.Value
            };

            _context.AppUsers.Add(appUser);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return (appUser, null);
        }
    }
}