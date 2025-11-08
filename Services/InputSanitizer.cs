using System.Text.RegularExpressions;
using System.Net;

namespace WebMatcha.Services;

/// <summary>
/// InputSanitizer - Protection XSS pour tous les inputs utilisateurs
/// CRITICAL: Security requirement - prevents XSS attacks
/// </summary>
public class InputSanitizer
{
    // Dangerous HTML/JS patterns that must be removed
    private static readonly Regex ScriptTagPattern = new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex OnEventPattern = new(@"on\w+\s*=", RegexOptions.IgnoreCase);
    private static readonly Regex JavascriptPattern = new(@"javascript\s*:", RegexOptions.IgnoreCase);
    private static readonly Regex StyleTagPattern = new(@"<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex IframePattern = new(@"<iframe[^>]*>.*?</iframe>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex ObjectPattern = new(@"<object[^>]*>.*?</object>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex EmbedPattern = new(@"<embed[^>]*>", RegexOptions.IgnoreCase);

    /// <summary>
    /// Sanitize text input - removes dangerous HTML/JS but preserves safe content
    /// </summary>
    public static string SanitizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string sanitized = input;

        // Remove dangerous patterns
        sanitized = ScriptTagPattern.Replace(sanitized, string.Empty);
        sanitized = StyleTagPattern.Replace(sanitized, string.Empty);
        sanitized = IframePattern.Replace(sanitized, string.Empty);
        sanitized = ObjectPattern.Replace(sanitized, string.Empty);
        sanitized = EmbedPattern.Replace(sanitized, string.Empty);
        sanitized = OnEventPattern.Replace(sanitized, string.Empty);
        sanitized = JavascriptPattern.Replace(sanitized, string.Empty);

        // HTML encode for extra safety
        sanitized = WebUtility.HtmlEncode(sanitized);

        return sanitized.Trim();
    }

    /// <summary>
    /// Sanitize username - only allows alphanumeric and underscore
    /// </summary>
    public static string SanitizeUsername(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove all non-alphanumeric characters except underscore
        return Regex.Replace(input, @"[^a-zA-Z0-9_]", string.Empty);
    }

    /// <summary>
    /// Sanitize email - basic validation and sanitization
    /// </summary>
    public static string SanitizeEmail(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove whitespace and dangerous characters
        var sanitized = input.Trim();
        sanitized = Regex.Replace(sanitized, @"[<>""']", string.Empty);

        return sanitized;
    }

    /// <summary>
    /// Sanitize tags (comma-separated) - only allows safe characters
    /// </summary>
    public static string SanitizeTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Split, sanitize each tag, rejoin
        var tags = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => Regex.Replace(tag.Trim(), @"[^a-zA-Z0-9\-_\s]", string.Empty))
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct();

        return string.Join(',', tags);
    }

    /// <summary>
    /// Sanitize biography/description - allows some formatting but removes dangerous content
    /// </summary>
    public static string SanitizeBiography(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string sanitized = input;

        // Remove all dangerous patterns
        sanitized = ScriptTagPattern.Replace(sanitized, string.Empty);
        sanitized = StyleTagPattern.Replace(sanitized, string.Empty);
        sanitized = IframePattern.Replace(sanitized, string.Empty);
        sanitized = ObjectPattern.Replace(sanitized, string.Empty);
        sanitized = EmbedPattern.Replace(sanitized, string.Empty);
        sanitized = OnEventPattern.Replace(sanitized, string.Empty);
        sanitized = JavascriptPattern.Replace(sanitized, string.Empty);

        // Limit length
        if (sanitized.Length > 1000)
            sanitized = sanitized.Substring(0, 1000);

        return sanitized.Trim();
    }

    /// <summary>
    /// Validate URL to prevent XSS via javascript: or data: URLs
    /// </summary>
    public static bool IsUrlSafe(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        url = url.Trim().ToLower();

        // Block dangerous URL schemes
        if (url.StartsWith("javascript:") ||
            url.StartsWith("data:") ||
            url.StartsWith("vbscript:") ||
            url.StartsWith("file:"))
        {
            return false;
        }

        // Only allow http, https, or relative URLs
        return url.StartsWith("http://") ||
               url.StartsWith("https://") ||
               url.StartsWith("/");
    }
}
