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

        public decimal Balance { get; set; } = 0;

        public bool IsClosed { get; set; } = false;

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public User? User { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}