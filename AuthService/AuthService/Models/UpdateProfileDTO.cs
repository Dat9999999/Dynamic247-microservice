namespace NewsPage.Models
{
    public class UpdateProfileDTO
    {
        public string Sex { get; set; }
        public string FullName { get; set; }
        public IFormFile? Avatar { get; set; }
        public DateTime Birthday{ get; set; }
    }
}
