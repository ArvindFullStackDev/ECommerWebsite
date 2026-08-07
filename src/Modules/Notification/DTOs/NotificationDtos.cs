namespace Notification.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}
