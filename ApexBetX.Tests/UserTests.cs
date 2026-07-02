using ApexBetX.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ApexBetX.Tests
{
    public class UserTests
    {
        private static IList<ValidationResult> ValidateModel(User user)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(user);

            Validator.TryValidateObject(user, context, results, true);

            return results;
        }

        [Fact]
        public void User_Should_Be_Created_With_Valid_Details()
        {
            var user = new User
            {
                UserId = 1,
                IDNumber = "0101015009087",
                FirstName = "Esihle",
                Surname = "Mlinjana",
                Email = "test@test.com",
                Phone = "0712345678"
            };

            Assert.Equal(1, user.UserId);
            Assert.Equal("0101015009087", user.IDNumber);
            Assert.Equal("Esihle", user.FirstName);
            Assert.Equal("Mlinjana", user.Surname);
            Assert.Equal("test@test.com", user.Email);
            Assert.Equal("0712345678", user.Phone);
        }

        [Fact]
        public void User_IDNumber_Is_Required()
        {
            var user = new User
            {
                FirstName = "Esihle",
                Surname = "Mlinjana"
            };

            var results = ValidateModel(user);

            Assert.Contains(results, r => r.MemberNames.Contains("IDNumber"));
        }

        [Fact]
        public void User_FirstName_Is_Required()
        {
            var user = new User
            {
                IDNumber = "0101015009087",
                Surname = "Mlinjana"
            };

            var results = ValidateModel(user);

            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void User_Surname_Is_Required()
        {
            var user = new User
            {
                IDNumber = "0101015009087",
                FirstName = "Esihle"
            };

            var results = ValidateModel(user);

            Assert.Contains(results, r => r.MemberNames.Contains("Surname"));
        }

        [Fact]
        public void User_Can_Have_Many_BettingAccounts()
        {
            var user = new User
            {
                UserId = 1,
                IDNumber = "0101015009087",
                FirstName = "Esihle",
                Surname = "Mlinjana",
                BettingAccounts = new List<BettingAccount>
                {
                    new BettingAccount { AccountId = 1, AccountNumber = "ACC001", UserId = 1 },
                    new BettingAccount { AccountId = 2, AccountNumber = "ACC002", UserId = 1 }
                }
            };

            Assert.NotNull(user.BettingAccounts);
            Assert.Equal(2, user.BettingAccounts.Count);
        }
    }
}