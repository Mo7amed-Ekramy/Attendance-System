using System;
using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Notifications
{
    public class NotificationItemViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationsListViewModel
    {
        public List<NotificationItemViewModel> Notifications { get; set; } = new List<NotificationItemViewModel>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}
