using System.ComponentModel.DataAnnotations;

namespace ApexBetX.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Display(Name = "ID Number")]
        [Required(ErrorMessage = "ID Number is required.")]
        [StringLength(13, MinimumLength = 13,
            ErrorMessage = "ID Number must be exactly 13 digits.")]
        [RegularExpression(@"^\d{13}$",
            ErrorMessage = "ID Number must contain only numbers.")]
        public string? IDNumber { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string? FirstName { get; set; }

        [Display(Name = "Surname")]
        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters.")]
        public string? Surname { get; set; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? Phone { get; set; }

        public ICollection<BettingAccount> BettingAccounts { get; set; } = new List<BettingAccount>();
        public bool IsArchived { get; set; } = false;
    }
}