using ApexBetX.Data;
using ApexBetX.Models;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Services
{
    public class AccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool CanCloseAccount(BettingAccount account)
        {
            return account.Balance == 0;
        }

        public bool CanAddTransaction(BettingAccount account)
        {
            return account.IsClosed == false;
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber)
        {
            return await _context.BettingAccounts
                .AnyAsync(a => a.AccountNumber == accountNumber);
        }
    }
}