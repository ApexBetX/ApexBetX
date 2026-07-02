using ApexBetX.Models;

namespace ApexBetX.Services
{
    public class TransactionService
    {
        public bool IsValidTransactionDate(DateTime transactionDate)
        {
            return transactionDate <= DateTime.Today;
        }

        public bool IsValidAmount(decimal amount)
        {
            return amount != 0;
        }

        public decimal CalculateNewBalance(decimal currentBalance, Transaction transaction)
        {
            if (transaction.TransactionType == "Credit")
            {
                return currentBalance + transaction.Amount;
            }

            if (transaction.TransactionType == "Debit")
            {
                return currentBalance - transaction.Amount;
            }

            return currentBalance;
        }
    }
}