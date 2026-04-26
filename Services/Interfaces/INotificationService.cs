using MVC_PROJECT.ViewModels.Notifications;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationsListViewModel> GetNotificationsByUserAsync(int userId);
        Task<NotificationItemViewModel> GetNotificationByIdAsync(int notificationId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task CreateQuizAnnouncementAsync(int courseId, string courseName);
        Task CreateAbsenceWarningAsync(int studentId);
        Task CreateAttendanceUpdateAsync(int studentId);
    }
}
