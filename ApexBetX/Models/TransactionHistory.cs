
namespace ApexBetX.Models
{
    public class TransactionHistory
    {
        public int TransactionHistoryId { get; set; }

        public int TransactionId { get; set; }
        public int AccountId { get; set; }

        public DateTime OldTransactionDate { get; set; }
        public decimal OldAmount { get; set; }
        public string? OldTransactionType { get; set; }
        public string? OldDescription { get; set; }

        public DateTime EditedDate { get; set; } = DateTime.Now;
    }
}
