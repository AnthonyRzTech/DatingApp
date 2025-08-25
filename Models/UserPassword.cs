namespace WebMatcha.Models;

public class UserPassword
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public User User { get; set; } = null!;
}