using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ApexBetX.Models
{
    public class PendingUserCreation
    {
        [Key]
        public int PendingUserCreationId { get; set; }

        public string IDNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string VerificationToken { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
