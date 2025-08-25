using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace WebMatcha.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string html);
    Task SendVerificationEmailAsync(string email, string username, string verificationToken);
    Task SendPasswordResetEmailAsync(string email, string username, string resetToken);
    Task SendEmailChangeVerificationAsync(string newEmail, string username, string verificationToken);
}

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _baseUrl;

    public EmailService()
    {
        // Get SMTP settings from environment variables
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
        _smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
        _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "noreply@webmatcha.com";
        _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "WebMatcha";
        _baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5192";
    }

    public async Task SendEmailAsync(string to, string subject, string html)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_fromName, _fromEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = html };

        using var smtp = new SmtpClient();
        
        try
        {
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            
            if (!string.IsNullOrEmpty(_smtpUser) && !string.IsNullOrEmpty(_smtpPass))
            {
                await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
            }
            
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
            // In development, we'll just log the error
            // In production, you might want to throw or handle differently
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }

    public async Task SendVerificationEmailAsync(string email, string username, string verificationToken)
    {
        var verificationUrl = $"{_baseUrl}/verify-email?token={verificationToken}";
        
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Verify Your Email - WebMatcha</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff4458; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f4f4f4; padding: 20px; margin-top: 20px; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #ff4458; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to WebMatcha!</h1>
        </div>
        <div class='content'>
            <h2>Hi {username},</h2>
            <p>Thank you for registering with WebMatcha. Please verify your email address by clicking the button below:</p>
            <a href='{verificationUrl}' class='button'>Verify Email</a>
            <p>Or copy and paste this link into your browser:</p>
            <p>{verificationUrl}</p>
            <p>This link will expire in 24 hours.</p>
            <p>If you didn't create an account, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, "Verify Your WebMatcha Account", html);
    }

    public async Task SendPasswordResetEmailAsync(string email, string username, string resetToken)
    {
        var resetUrl = $"{_baseUrl}/reset-password?token={resetToken}";
        
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reset Your Password - WebMatcha</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff4458; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f4f4f4; padding: 20px; margin-top: 20px; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #ff4458; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hi {username},</h2>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            <a href='{resetUrl}' class='button'>Reset Password</a>
            <p>Or copy and paste this link into your browser:</p>
            <p>{resetUrl}</p>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request a password reset, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, "Reset Your WebMatcha Password", html);
    }
    
    public async Task SendEmailChangeVerificationAsync(string newEmail, string username, string verificationToken)
    {
        var verifyUrl = $"{_baseUrl}/api/verify-email-change/{verificationToken}";
        
        var html = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f4f4f4; padding: 20px; margin-top: 20px; }}
        .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Confirm Your New Email Address</h1>
        </div>
        <div class='content'>
            <h2>Hi {username},</h2>
            <p>You requested to change your email address to this one. Please click the button below to confirm this change:</p>
            <a href='{verifyUrl}' class='button'>Confirm Email Change</a>
            <p>Or copy and paste this link into your browser:</p>
            <p>{verifyUrl}</p>
            <p>This link will expire in 24 hours.</p>
            <p>If you didn't request this change, please ignore this email and your email address will remain unchanged.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(newEmail, "Confirm Your New Email Address - WebMatcha", html);
    }
}