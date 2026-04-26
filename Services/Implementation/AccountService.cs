using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using System;
using System.Threading.Tasks;
using MVC_PROJECT.Services.Interfaces;

namespace MVC_PROJECT.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> ValidateLoginAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            // Find user by username (case-insensitive)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());

            // Validate password (plain text comparison for development)
            if (user == null || user.Password != password)
            {
                return null;
            }

            return user;
        }

        public string GetRedirectUrlByRole(User user)
        {
            return user.Role switch
            {
                Role.Admin => "/Admin/Dashboard",
                Role.Doctor => "/Doctor/Dashboard",
                Role.TA => "/TA/Dashboard",
                Role.Student => "/Student/Dashboard",
                _ => "/"
            };
        }
    }
}
