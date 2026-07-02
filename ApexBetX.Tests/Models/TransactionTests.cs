using ApexBetX.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ApexBetX.Tests.Models
{
    public class TransactionTests
    {
        private static IList<ValidationResult> ValidateModel(Transaction transaction)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(transaction);

            Validator.TryValidateObject(transaction, context, results, true);

            return results;
        }

        [Fact]
        public void Transaction_Should_Be_Created_With_Valid_Details()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                TransactionDate = DateTime.Today,
                CaptureDate = DateTime.Now,
                Amount = 250.00m,
                TransactionType = "Credit",
                Description = "Deposit",
                AccountId = 1
            };

            // Assert
            Assert.Equal(1, transaction.TransactionId);
            Assert.Equal(250.00m, transaction.Amount);
            Assert.Equal("Credit", transaction.TransactionType);
            Assert.Equal("Deposit", transaction.Description);
            Assert.Equal(1, transaction.AccountId);
        }

        [Fact]
        public void Transaction_Should_Belong_To_A_BettingAccount()
        {
            // Arrange
            var account = new BettingAccount
            {
                AccountId = 1,
                AccountNumber = "ACC001",
                UserId = 1
            };

            var transaction = new Transaction
            {
                TransactionId = 1,
                TransactionDate = DateTime.Today,
                CaptureDate = DateTime.Now,
                Amount = 100,
                AccountId = 1,
                BettingAccount = account
            };

            // Assert
            Assert.NotNull(transaction.BettingAccount);
            Assert.Equal(account.AccountId, transaction.BettingAccount.AccountId);
        }

        [Fact]
        public void Transaction_Stores_Correct_Amount()
        {
            // Arrange
            var transaction = new Transaction
            {
                Amount = 500.75m
            };

            // Assert
            Assert.Equal(500.75m, transaction.Amount);
        }

        [Fact]
        public void Transaction_Stores_Correct_Dates()
        {
            // Arrange
            var transactionDate = DateTime.Today;
            var captureDate = DateTime.Now;

            var transaction = new Transaction
            {
                TransactionDate = transactionDate,
                CaptureDate = captureDate
            };

            // Assert
            Assert.Equal(transactionDate, transaction.TransactionDate);
            Assert.Equal(captureDate, transaction.CaptureDate);
        }

        [Fact]
        public void Transaction_Can_Have_A_Description()
        {
            // Arrange
            var transaction = new Transaction
            {
                Description = "Winning Bet Payout"
            };

            // Assert
            Assert.Equal("Winning Bet Payout", transaction.Description);
        }
    }
}