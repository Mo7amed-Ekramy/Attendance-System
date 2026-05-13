namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentNotificationViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string? Message { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
