using System.ComponentModel.DataAnnotations;

namespace NewsPage.Models
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [MaxLength(50, ErrorMessage = "Mật khẩu không được quá 50 ký tự")]
        public string Password { get; set; }
        public required string FullName { get; set; }
        public required string Sex { get; set; }
        public string Role { get; set; }
        public DateTime Birthday { get; set; }
    }
}
