using ApexBetX.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ApexBetX.Tests
{
    public class BettingAccountTests
    {
        private static IList<ValidationResult> ValidateModel(BettingAccount account)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(account);

            Validator.TryValidateObject(account, context, results, true);

            return results;
        }

        [Fact]
        public void BettingAccount_Should_Be_Created_With_Valid_Details()
        {
            var account = new BettingAccount
            {
                AccountId = 1,
                AccountNumber = "ACC001",
                Balance = 100,
                IsClosed = false,
                CreatedDate = DateTime.Now,
                UserId = 1
            };

            Assert.Equal(1, account.AccountId);
            Assert.Equal("ACC001", account.AccountNumber);
            Assert.Equal(100, account.Balance);
            Assert.False(account.IsClosed);
            Assert.Equal(1, account.UserId);
        }

        [Fact]
        public void BettingAccount_AccountNumber_Is_Required()
        {
            var account = new BettingAccount
            {
                Balance = 100,
                IsClosed = false,
                CreatedDate = DateTime.Now,
                UserId = 1
            };

            var results = ValidateModel(account);

            Assert.Contains(results, r => r.MemberNames.Contains("AccountNumber"));
        }

        [Fact]
        public void BettingAccount_Belongs_To_One_User()
        {
            var account = new BettingAccount
            {
                AccountId = 1,
                AccountNumber = "ACC001",
                UserId = 1,
                User = new User
                {
                    UserId = 1,
                    IDNumber = "0101015009087",
                    FirstName = "Esihle",
                    Surname = "Mlinjana"
                }
            };

            Assert.NotNull(account.User);
            Assert.Equal(1, account.User.UserId);
            Assert.Equal("Esihle", account.User.FirstName);
        }

        [Fact]
        public void BettingAccount_Can_Have_Many_Transactions()
        {
            var account = new BettingAccount
            {
                AccountId = 1,
                AccountNumber = "ACC001",
                UserId = 1,
                Transactions = new List<Transaction>
                {
                    new Transaction { TransactionId = 1, AccountId = 1, Amount = 50 },
                    new Transaction { TransactionId = 2, AccountId = 1, Amount = 100 }
                }
            };

            Assert.NotNull(account.Transactions);
            Assert.Equal(2, account.Transactions.Count);
        }

        [Fact]
        public void BettingAccount_Can_Be_Closed()
        {
            var account = new BettingAccount
            {
                AccountId = 1,
                AccountNumber = "ACC001",
                IsClosed = true
            };

            Assert.True(account.IsClosed);
        }
    }
}