namespace MinimalApi_Auth.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Player";
        public int FailedLoginAttempts { get; set; } = 0;
        public bool IsLockedOut { get; set; } = false;
    }
}
