namespace Blood_Donations_Project.Models
{
    public class PasswordReset
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
