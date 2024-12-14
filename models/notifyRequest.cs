public class NotificationRequest
{
    public int UserId { get; set; }
    public string Message { get; set; }
    public string NotificationType { get; set; }  // This is needed for the notification type
}
