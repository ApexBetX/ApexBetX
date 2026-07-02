using ApexBetX.Models;
using ApexBetX.Services;
using Xunit;

namespace ApexBetX.Tests.Services
{
    public class TransactionServiceTests
    {
        [Fact]
        public void Future_Transaction_Date_Is_Invalid()
        {
            var service = new TransactionService();
            var futureDate = DateTime.Today.AddDays(1);

            Assert.False(service.IsValidTransactionDate(futureDate));
        }

        [Fact]
        public void Today_Transaction_Date_Is_Valid()
        {
            var service = new TransactionService();

            Assert.True(service.IsValidTransactionDate(DateTime.Today));
        }

        [Fact]
        public void Zero_Amount_Is_Invalid()
        {
            var service = new TransactionService();

            Assert.False(service.IsValidAmount(0));
        }

        [Fact]
        public void Non_Zero_Amount_Is_Valid()
        {
            var service = new TransactionService();

            Assert.True(service.IsValidAmount(100));
        }

        [Fact]
        public void Credit_Increases_Balance()
        {
            var service = new TransactionService();

            var transaction = new Transaction
            {
                Amount = 50,
                TransactionType = "Credit"
            };

            var result = service.CalculateNewBalance(100, transaction);

            Assert.Equal(150, result);
        }

        [Fact]
        public void Debit_Decreases_Balance()
        {
            var service = new TransactionService();

            var transaction = new Transaction
            {
                Amount = 30,
                TransactionType = "Debit"
            };

            var result = service.CalculateNewBalance(100, transaction);

            Assert.Equal(70, result);
        }
    }
}