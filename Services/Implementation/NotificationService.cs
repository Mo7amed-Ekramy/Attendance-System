using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Hubs;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.ViewModels.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVC_PROJECT.Services.Interfaces;


namespace MVC_PROJECT.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<NotificationsListViewModel> GetNotificationsByUserAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Id)
                .ToListAsync();

            var notificationItems = new List<NotificationItemViewModel>();

            foreach (var notification in notifications)
            {
                notificationItems.Add(new NotificationItemViewModel
                {
                    NotificationId = notification.Id,
                    Title = notification.Title,
                    Type = notification.Type.ToString(),
                    IsRead = notification.IsRead,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var unreadCount = notifications.Count(n => !n.IsRead);

            return new NotificationsListViewModel
            {
                Notifications = notificationItems,
                UnreadCount = unreadCount,
                TotalCount = notifications.Count
            };
        }

        public async Task<NotificationItemViewModel> GetNotificationByIdAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                return new NotificationItemViewModel();
            }

            return new NotificationItemViewModel
            {
                NotificationId = notification.Id,
                Title = notification.Title,
                Type = notification.Type.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateQuizAnnouncementAsync(int courseId, string courseName)
        {
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => sectionIds.Contains(e.CourseSectionId))
                .ToListAsync();

            var studentIds = new List<int>();
            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;
                if (student != null)
                {
                    studentIds.Add(student.Id);

                    var notification = new Notification
                    {
                        UserId = student.UserId,
                        Title = $"New quiz scheduled for {courseName}",
                        Type = AttendanceSessionType.Section,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();

            // Send real-time notifications to all students in course sections
            foreach (var sectionId in sectionIds)
            {
                await _hubContext.Clients.Group($"Section-{sectionId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        title = $"New quiz scheduled for {courseName}",
                        type = AttendanceSessionType.Section.ToString(),
                        createdAt = DateTime.UtcNow
                    });
            }
        }

        public async Task CreateAbsenceWarningAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = student.UserId,
                Title = "Warning: You have exceeded the allowed absence limit",
                Type = AttendanceSessionType.Section,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Send real-time notification to the student
            await _hubContext.Clients.Group($"Student-{studentId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = "Warning: You have exceeded the allowed absence limit",
                    type = AttendanceSessionType.Section.ToString(),
                    createdAt = DateTime.UtcNow
                });
        }

        public async Task CreateAttendanceUpdateAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = student.UserId,
                Title = "Your attendance has been updated",
                Type = AttendanceSessionType.Section,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Student-{studentId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = "Your attendance has been updated",
                    type = AttendanceSessionType.Section.ToString(),
                    createdAt = DateTime.UtcNow
                });
        }
    }
}
