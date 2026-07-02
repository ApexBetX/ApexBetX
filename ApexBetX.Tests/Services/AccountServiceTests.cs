using ApexBetX.Models;
using ApexBetX.Services;
using Xunit;

namespace ApexBetX.Tests.Services
{
    public class AccountServiceTests
    {
        [Fact]
        public void Account_With_Zero_Balance_Can_Be_Closed()
        {
            var service = new AccountService();

            var account = new BettingAccount
            {
                Balance = 0
            };

            Assert.True(service.CanCloseAccount(account));
        }

        [Fact]
        public void Account_With_Non_Zero_Balance_Cannot_Be_Closed()
        {
            var service = new AccountService();

            var account = new BettingAccount
            {
                Balance = 100
            };

            Assert.False(service.CanCloseAccount(account));
        }

        [Fact]
        public void Open_Account_Can_Add_Transaction()
        {
            var service = new AccountService();

            var account = new BettingAccount
            {
                IsClosed = false
            };

            Assert.True(service.CanAddTransaction(account));
        }

        [Fact]
        public void Closed_Account_Cannot_Add_Transaction()
        {
            var service = new AccountService();

            var account = new BettingAccount
            {
                IsClosed = true
            };

            Assert.False(service.CanAddTransaction(account));
        }
    }
}