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
            if (transaction.TransactionType == "Deposit")
                return currentBalance + transaction.Amount;

            if (transaction.TransactionType == "Withdrawal")
                return currentBalance - transaction.Amount;

            return currentBalance;
        }

        public void SetCaptureDate(Transaction transaction)
        {
            transaction.CaptureDate = DateTime.Now;
        }
        public decimal ReverseTransaction(decimal currentBalance, Transaction oldTransaction)
        {
            if (oldTransaction.TransactionType == "Deposit")
                return currentBalance - oldTransaction.Amount;

            if (oldTransaction.TransactionType == "Withdrawal")
                return currentBalance + oldTransaction.Amount;

            return currentBalance;
        }

        public decimal ApplyTransaction(decimal currentBalance, Transaction newTransaction)
        {
            if (newTransaction.TransactionType == "Deposit")
                return currentBalance + newTransaction.Amount;

            if (newTransaction.TransactionType == "Withdrawal")
                return currentBalance - newTransaction.Amount;

            return currentBalance;
        }
    }
}