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
                    CreatedAt = notification.CreatedAt
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
                CreatedAt = notification.CreatedAt
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

        public async Task CreateQuizAnnouncementAsync(int sectionId, string quizTitle)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseSectionId == sectionId)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;
                if (student != null)
                {
                    var notification = new Notification
                    {
                        UserId = student.UserId,
                        Title = $"New quiz: {quizTitle}",
                        Message = $"A new quiz '{quizTitle}' has been scheduled for your section.",
                        Type = AttendanceSessionType.Quiz,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();

            // Send real-time notifications to all students in the section (group broadcast)
            await _hubContext.Clients.Group($"Section-{sectionId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = $"New quiz: {quizTitle}",
                    message = $"A new quiz '{quizTitle}' has been scheduled for your section.",
                    type = AttendanceSessionType.Quiz.ToString(),
                    createdAt = DateTime.UtcNow
                });

            // Also send directly to each student's SignalR user id for reliable delivery
            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;
                if (student == null) continue;

                try
                {
                    if (student.UserId > 0)
                    {
                        await _hubContext.Clients.User(student.UserId.ToString()).SendAsync("ReceiveNotification", new
                        {
                            title = $"New quiz: {quizTitle}",
                            message = $"A new quiz '{quizTitle}' has been scheduled for your section.",
                            type = AttendanceSessionType.Quiz.ToString(),
                            createdAt = DateTime.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    // swallow and continue (logging will help diagnose unexpected issues)
                    // If logging is needed, it should be added via ILogger injection; keeping minimal changes
                }
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
        public async Task NotifyQuizGradesUploadedAsync(int quizId)
        {
            // Get quiz, section, and grades
            var quiz = await _context.Quizzes.Include(q => q.CourseSection).FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return;

            var grades = await _context.QuizGrades
                .Include(g => g.Enrollment)
                .ThenInclude(e => e.Student)
                .Where(g => g.QuizId == quizId)
                .ToListAsync();

            foreach (var grade in grades)
            {
                var student = grade.Enrollment.Student;
                if (student == null) continue;

                var notification = new Notification
                {
                    UserId = student.UserId,
                    Title = $"Quiz grades uploaded: {quiz.Title}",
                    Message = $"Your grade for quiz '{quiz.Title}' has been uploaded.",
                    Type = AttendanceSessionType.Section, // Or a new enum value if needed
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                // SignalR real-time notification
                await _hubContext.Clients.Group($"Student-{student.Id}").SendAsync("ReceiveNotification", new
                {
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    createdAt = notification.CreatedAt
                });

                // Also send directly to the application user (reliable if client connections are associated with the user id)
                if (student.UserId > 0)
                {
                    await _hubContext.Clients.User(student.UserId.ToString()).SendAsync("ReceiveNotification", new
                    {
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type.ToString(),
                        createdAt = notification.CreatedAt
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
