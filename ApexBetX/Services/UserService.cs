using ApexBetX.Data;
using ApexBetX.Models;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool CanArchiveUser(User user)
        {
            return user.BettingAccounts == null ||
                   !user.BettingAccounts.Any() ||
                   user.BettingAccounts.All(a => a.IsClosed);
        }
        public async Task<bool> IDNumberExistsAsync(string idNumber)
        {
            return await _context.Users.AnyAsync(u => u.IDNumber == idNumber);
        }
        public async Task<bool> DuplicateIDNumberExistsAsync(int userId, string idNumber)
        {
            return await _context.Users
                .AnyAsync(u => u.UserId != userId &&
                               u.IDNumber == idNumber);
        }
    }
}