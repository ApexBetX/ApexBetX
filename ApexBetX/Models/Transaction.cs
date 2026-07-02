using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApexBetX.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Transaction date is required.")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        public DateTime CaptureDate { get; set; }

        [Required(ErrorMessage = "Transaction amount is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Transaction amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Transaction type is required.")]
        [StringLength(20)]
        public string? TransactionType { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public BettingAccount? BettingAccount { get; set; }
    }
}