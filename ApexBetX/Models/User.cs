using System.ComponentModel.DataAnnotations;

namespace ApexBetX.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string IDNumber { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public ICollection<BettingAccount>? BettingAccounts { get; set; }
    }
}
