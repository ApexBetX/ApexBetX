using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApexBetX.Tests.Services
{
    public class AccountServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Account_With_Zero_Balance_Can_Be_Closed()
        {
            var context = GetDbContext();
            var service = new AccountService(context);

            var account = new BettingAccount { Balance = 0 };

            Assert.True(service.CanCloseAccount(account));
        }

        [Fact]
        public void Account_With_Non_Zero_Balance_Cannot_Be_Closed()
        {
            var context = GetDbContext();
            var service = new AccountService(context);

            var account = new BettingAccount { Balance = 100 };

            Assert.False(service.CanCloseAccount(account));
        }

        [Fact]
        public void Open_Account_Can_Add_Transaction()
        {
            var context = GetDbContext();
            var service = new AccountService(context);

            var account = new BettingAccount { IsClosed = false };

            Assert.True(service.CanAddTransaction(account));
        }

        [Fact]
        public void Closed_Account_Cannot_Add_Transaction()
        {
            var context = GetDbContext();
            var service = new AccountService(context);

            var account = new BettingAccount { IsClosed = true };

            Assert.False(service.CanAddTransaction(account));
        }

        [Fact]
        public async Task AccountNumberExistsAsync_Returns_True_When_Account_Already_Exists()
        {
            var context = GetDbContext();

            context.BettingAccounts.Add(new BettingAccount
            {
                AccountNumber = "ACC1001",
                UserId = 1,
                Balance = 0,
                CreatedDate = DateTime.Now
            });

            await context.SaveChangesAsync();

            var service = new AccountService(context);

            var result = await service.AccountNumberExistsAsync("ACC1001");

            Assert.True(result);
        }

        [Fact]
        public async Task AccountNumberExistsAsync_Returns_False_When_Account_Does_Not_Exist()
        {
            var context = GetDbContext();
            var service = new AccountService(context);

            var result = await service.AccountNumberExistsAsync("ACC9999");

            Assert.False(result);
        }
    }
}