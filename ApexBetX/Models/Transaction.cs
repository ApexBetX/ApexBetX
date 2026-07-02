using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApexBetX.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime CaptureDate { get; set; }

        public decimal Amount { get; set; }

        public string? TransactionType { get; set; }

        public string? Description { get; set; }

        [ForeignKey("BettingAccount")]
        public int AccountId { get; set; }

        public BettingAccount? BettingAccount { get; set; }
    }
}
