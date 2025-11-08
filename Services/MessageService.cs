using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

/// <summary>
/// MessageService - Refactoré avec requêtes SQL manuelles
/// </summary>
public class MessageService
{
    private readonly string _connectionString;
    private readonly MatchingService _matchingService;

    public MessageService(IConfiguration configuration, MatchingService matchingService)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _matchingService = matchingService;
    }

    public async Task<Message?> SendMessageAsync(int senderId, int receiverId, string content)
    {
        // Check if users are matched (can only message matches)
        var isMatched = await _matchingService.IsMatchedAsync(senderId, receiverId);
        if (!isMatched)
        {
            return null; // Can't send message if not matched
        }

        // Check if either user has blocked the other
        var isBlocked = await _matchingService.IsBlockedAsync(senderId, receiverId);
        if (isBlocked)
        {
            return null; // Can't send message if blocked
        }

        const string sql = @"
            INSERT INTO messages (sender_id, receiver_id, content, sent_at, is_read)
            VALUES (@SenderId, @ReceiverId, @Content, @SentAt, false)
            RETURNING id, sender_id AS SenderId, receiver_id AS ReceiverId, content, sent_at AS SentAt, is_read AS IsRead
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var message = await connection.QueryFirstOrDefaultAsync<Message>(sql, new
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow
        });

        return message;
    }

    public async Task<List<Message>> GetConversationAsync(int user1Id, int user2Id)
    {
        const string sql = @"
            SELECT
                id, sender_id AS SenderId, receiver_id AS ReceiverId,
                content, sent_at AS SentAt, is_read AS IsRead
            FROM messages
            WHERE (sender_id = @User1Id AND receiver_id = @User2Id) OR (sender_id = @User2Id AND receiver_id = @User1Id)
            ORDER BY sent_at ASC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var messages = await connection.QueryAsync<Message>(sql, new { User1Id = user1Id, User2Id = user2Id });
        return messages.ToList();
    }

    public async Task<List<Message>> GetRecentMessagesAsync(int userId, int otherUserId, int count = 50)
    {
        const string sql = @"
            SELECT * FROM (
                SELECT
                    id, sender_id AS SenderId, receiver_id AS ReceiverId,
                    content, sent_at AS SentAt, is_read AS IsRead
                FROM messages
                WHERE (sender_id = @UserId AND receiver_id = @OtherUserId) OR (sender_id = @OtherUserId AND receiver_id = @UserId)
                ORDER BY sent_at DESC
                LIMIT @Count
            ) AS recent_messages
            ORDER BY sent_at ASC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var messages = await connection.QueryAsync<Message>(sql, new { UserId = userId, OtherUserId = otherUserId, Count = count });
        return messages.ToList();
    }

    public async Task MarkMessagesAsReadAsync(int receiverId, int senderId)
    {
        const string sql = @"
            UPDATE messages
            SET is_read = true
            WHERE sender_id = @SenderId AND receiver_id = @ReceiverId AND is_read = false
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { SenderId = senderId, ReceiverId = receiverId });
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        const string sql = "SELECT COUNT(*) FROM messages WHERE receiver_id = @UserId AND is_read = false";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<Dictionary<int, int>> GetUnreadCountsByUserAsync(int userId)
    {
        const string sql = @"
            SELECT sender_id, COUNT(*) AS count
            FROM messages
            WHERE receiver_id = @UserId AND is_read = false
            GROUP BY sender_id
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var results = await connection.QueryAsync<(int sender_id, int count)>(sql, new { UserId = userId });
        return results.ToDictionary(x => x.sender_id, x => x.count);
    }

    public async Task<List<ConversationSummary>> GetConversationsAsync(int userId)
    {
        const string sql = @"
            WITH latest_messages AS (
                SELECT DISTINCT ON (other_user_id)
                    CASE
                        WHEN sender_id = @UserId THEN receiver_id
                        ELSE sender_id
                    END AS other_user_id,
                    content AS last_message,
                    sent_at AS last_message_time
                FROM messages
                WHERE sender_id = @UserId OR receiver_id = @UserId
                ORDER BY other_user_id, sent_at DESC
            ),
            unread_counts AS (
                SELECT sender_id AS other_user_id, COUNT(*) AS unread_count
                FROM messages
                WHERE receiver_id = @UserId AND is_read = false
                GROUP BY sender_id
            )
            SELECT
                lm.other_user_id AS OtherUserId,
                u.first_name || ' ' || u.last_name AS OtherUserName,
                u.profile_photo_url AS OtherUserPhoto,
                lm.last_message AS LastMessage,
                lm.last_message_time AS LastMessageTime,
                COALESCE(uc.unread_count, 0) AS UnreadCount,
                u.is_online AS IsOnline
            FROM latest_messages lm
            INNER JOIN users u ON u.id = lm.other_user_id
            LEFT JOIN unread_counts uc ON uc.other_user_id = lm.other_user_id
            ORDER BY lm.last_message_time DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var conversations = await connection.QueryAsync<ConversationSummary>(sql, new { UserId = userId });
        return conversations.ToList();
    }

    public async Task<Message?> GetLastMessageAsync(int user1Id, int user2Id)
    {
        const string sql = @"
            SELECT
                id, sender_id AS SenderId, receiver_id AS ReceiverId,
                content, sent_at AS SentAt, is_read AS IsRead
            FROM messages
            WHERE (sender_id = @User1Id AND receiver_id = @User2Id) OR (sender_id = @User2Id AND receiver_id = @User1Id)
            ORDER BY sent_at DESC
            LIMIT 1
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Message>(sql, new { User1Id = user1Id, User2Id = user2Id });
    }
}

public class ConversationSummary
{
    public int OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserPhoto { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}
