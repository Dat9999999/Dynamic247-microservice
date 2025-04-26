namespace NewsPage.Models.entities
{
    public class UserAccounts
    {
        public Guid Id { get; set; }

        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }

        public DateTime CreatedAt { get; set; }
        public required string Status { get; set; }
        public required bool IsVerified { get; set; }
    }
}
