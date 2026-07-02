using ApexBetX.Models;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<BettingAccount> BettingAccounts { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
