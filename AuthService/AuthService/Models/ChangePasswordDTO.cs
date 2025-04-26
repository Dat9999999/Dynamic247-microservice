using System.ComponentModel.DataAnnotations;

namespace NewsPage.Models
{
    public class ChangePasswordDTO
    {
        [Required]
        
        public string Email { get; set; }

        [Required] 
        public string NewPassword { get; set; }

        [Required]
        public string Otp { get; set; }
    }
}
