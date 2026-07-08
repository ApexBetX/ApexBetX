using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ApexBetX.ViewModels
{
    public class VerifyTokenViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, MinimumLength = 6)]
        [Display(Name = "Verification Code")]
        public string Token { get; set; } = string.Empty;
    }
}
