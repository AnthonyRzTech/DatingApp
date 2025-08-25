using Microsoft.EntityFrameworkCore;
using WebMatcha.Models;
using WebMatcha.Services;

namespace WebMatcha.Data;

public class MatchaDbContext : DbContext
{
    public MatchaDbContext(DbContextOptions<MatchaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<ProfileView> ProfileViews { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<UserPassword> UserPasswords { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<PasswordReset> PasswordResets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.SexualPreference).HasMaxLength(20);
            entity.Property(e => e.Biography).HasMaxLength(500);
            
            // Store lists as JSON
            entity.Property(e => e.InterestTags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            
            entity.Property(e => e.PhotoUrls)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            entity.Ignore(e => e.Age);
            entity.Ignore(e => e.Distance);
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Like entity
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.LikerId, e.LikedId }).IsUnique();
        });

        // Configure Match entity
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.User1Id, e.User2Id }).IsUnique();
        });

        // Configure ProfileView entity
        modelBuilder.Entity<ProfileView>(entity =>
        {
            entity.ToTable("profile_views");
            entity.HasKey(e => e.Id);
        });

        // Configure Block entity
        modelBuilder.Entity<Block>(entity =>
        {
            entity.ToTable("blocks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BlockerId, e.BlockedId }).IsUnique();
        });

        // Configure Report entity
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(e => e.Id);
        });

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
        });

        // Configure Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SenderId, e.ReceiverId });
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
        });

        // Configure UserPassword entity
        modelBuilder.Entity<UserPassword>(entity =>
        {
            entity.ToTable("user_passwords");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
        });

        // Configure EmailVerification entity
        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.ToTable("email_verifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Token).IsUnique();
        });

        // Configure PasswordReset entity
        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.ToTable("password_resets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Token).IsUnique();
        });
    }
}