using ApexBetX.Models;
using ApexBetX.Services;
using Xunit;

namespace ApexBetX.Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public void User_With_No_Accounts_Can_Be_Deleted()
        {
            var service = new UserService();

            var user = new User
            {
                BettingAccounts = new List<BettingAccount>()
            };

            Assert.True(service.CanDeleteUser(user));
        }

        [Fact]
        public void User_With_Open_Account_Cannot_Be_Deleted()
        {
            var service = new UserService();

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
            var service = new UserService();

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
    }
}