using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApexBetX.Models
{
    public class BettingAccount
    {
        [Key]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Account Number is required.")]
        [StringLength(20, ErrorMessage = "Account Number cannot exceed 20 characters.")]
        public string? AccountNumber { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Balance cannot be negative.")]
        public decimal Balance { get; set; }

        public bool IsClosed { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "A user must be selected.")]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public User? User { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}