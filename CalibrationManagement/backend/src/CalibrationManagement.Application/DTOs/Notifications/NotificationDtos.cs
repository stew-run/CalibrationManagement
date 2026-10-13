using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Application.DTOs;

public class NotificationDto : PagedRow
{
    public int NotificationId { get; set; }
    public int? EquipmentId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationQuery : PagedQuery
{
    public bool UnreadOnly { get; set; }
}
