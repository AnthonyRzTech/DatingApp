namespace WebMatcha.Models;

public enum NotificationType
{
    Like,
    Unlike,
    ProfileView,
    Match,
    Message
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string SexualPreference { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public List<string> InterestTags { get; set; } = new();
    public string ProfilePhotoUrl { get; set; } = "/images/default-avatar.png";
    public List<string> PhotoUrls { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int FameRating { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Age => DateTime.Now.Year - BirthDate.Year;
    public double? Distance { get; set; } // Calculated field
}

public class Like
{
    public int Id { get; set; }
    public int LikerId { get; set; }
    public int LikedId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Match
{
    public int Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime MatchedAt { get; set; }
}

public class ProfileView
{
    public int Id { get; set; }
    public int ViewerId { get; set; }
    public int ViewedId { get; set; }
    public DateTime ViewedAt { get; set; }
}

public class Block
{
    public int Id { get; set; }
    public int BlockerId { get; set; }
    public int BlockedId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Report
{
    public int Id { get; set; }
    public int ReporterId { get; set; }
    public int ReportedId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // "like", "view", "match", "unlike"
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}