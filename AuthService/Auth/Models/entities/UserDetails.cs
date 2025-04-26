namespace NewsPage.Models.entities
{
    public class UserDetails
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Sex { get; set; }

        public DateTime Birthday { get; set; }

        public string? Avatar { get; set; }

        public Guid UserAccountId { get; set; }
    }
}
