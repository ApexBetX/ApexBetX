using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApexBetX.Tests.Services
{
    public class UserServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void User_With_No_Accounts_Can_Be_Deleted()
        {
            var context = GetDbContext();
            var service = new UserService(context);

            var user = new User
            {
                BettingAccounts = new List<BettingAccount>()
            };

            Assert.True(service.CanDeleteUser(user));
        }

        [Fact]
        public void User_With_Open_Account_Cannot_Be_Deleted()
        {
            var context = GetDbContext();
            var service = new UserService(context);

            var user = new User
            {
                BettingAccounts = new List<BettingAccount>
                {
                    new BettingAccount { IsClosed = false }
                }
            };

            Assert.False(service.CanDeleteUser(user));
        }

        [Fact]
        public void User_With_All_Closed_Accounts_Can_Be_Deleted()
        {
            var context = GetDbContext();
            var service = new UserService(context);

            var user = new User
            {
                BettingAccounts = new List<BettingAccount>
                {
                    new BettingAccount { IsClosed = true },
                    new BettingAccount { IsClosed = true }
                }
            };

            Assert.True(service.CanDeleteUser(user));
        }

        [Fact]
        public async Task IDNumberExistsAsync_Returns_True_When_IDNumber_Already_Exists()
        {
            var context = GetDbContext();

            context.Users.Add(new User
            {
                IDNumber = "9901015009087",
                FirstName = "Test",
                Surname = "User",
                Email = "test@test.com",
                Phone = "0712345678"
            });

            await context.SaveChangesAsync();

            var service = new UserService(context);

            var result = await service.IDNumberExistsAsync("9901015009087");

            Assert.True(result);
        }

        [Fact]
        public async Task IDNumberExistsAsync_Returns_False_When_IDNumber_Does_Not_Exist()
        {
            var context = GetDbContext();
            var service = new UserService(context);

            var result = await service.IDNumberExistsAsync("8801015009087");

            Assert.False(result);
        }
    }
}