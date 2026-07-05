using ApexBetX.Models;

namespace ApexBetX.Services
{
    public class TransactionService
    {
        public bool IsValidTransactionDate(DateTime transactionDate)
        {
            return transactionDate.Date <= DateTime.Today;
        }

        public bool IsValidAmount(decimal amount)
        {
            return amount != 0;
        }

        public bool CanAddTransaction(BettingAccount account)
        {
            return account != null && !account.IsClosed;
        }

        public decimal CalculateNewBalance(decimal currentBalance, Transaction transaction)
        {
            if (transaction.TransactionType == "Credit")
                return currentBalance + transaction.Amount;

            if (transaction.TransactionType == "Debit")
                return currentBalance - transaction.Amount;

            return currentBalance;
        }

        public void SetCaptureDate(Transaction transaction)
        {
            transaction.CaptureDate = DateTime.Now;
        }
    }
}