using System.ComponentModel.DataAnnotations;

namespace NewsPage.Models
{
    public class VerifyOtplDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Mã OTP phải đúng 6 ký tự")]
        [MaxLength(6, ErrorMessage = "Mã OTP phải đúng 6 ký tự")]
        public string Otp { get; set; }
    }
}
