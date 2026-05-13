using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Hubs;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Notifications;

namespace MVC_PROJECT.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            AppDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<NotificationsListViewModel> GetNotificationsByUserAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var notificationItems = new List<NotificationItemViewModel>();

            foreach (var notification in notifications)
            {
                notificationItems.Add(new NotificationItemViewModel
                {
                    NotificationId = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type.ToString(),
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                });
            }

            return new NotificationsListViewModel
            {
                Notifications = notificationItems,
                UnreadCount = notifications.Count(n => !n.IsRead),
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
                Message = notification.Message,
                Type = notification.Type.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FindAsync(notificationId);

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

        public async Task CreateQuizAnnouncementAsync(
            int sectionId,
            string quizTitle,
            string courseName,
            DateTime quizDate,
            decimal maxMark)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseSectionId == sectionId)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;

                if (student == null)
                    continue;

                var notification = new Notification
                {
                    UserId = student.UserId,

                    Title = $"New Quiz Added - {courseName}",

                    Message =
                        $"Quiz: {quizTitle}\n" +
                        $"Date: {quizDate:dd MMM yyyy - hh:mm tt}\n" +
                        $"Max Mark: {maxMark}",

                    Type = AttendanceSessionType.Quiz,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            // SignalR - Group Broadcast
            await _hubContext.Clients.Group($"Section-{sectionId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = $"New Quiz Added - {courseName}",

                    message =
                        $"Quiz: {quizTitle}\n" +
                        $"Date: {quizDate:dd MMM yyyy - hh:mm tt}\n" +
                        $"Max Mark: {maxMark}",

                    type = AttendanceSessionType.Quiz.ToString(),
                    createdAt = DateTime.UtcNow
                });

            // SignalR - Direct User Notifications
            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;

                if (student == null)
                    continue;

                if (student.UserId > 0)
                {
                    await _hubContext.Clients
                        .User(student.UserId.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            title = $"New Quiz Added - {courseName}",

                            message =
                                $"Quiz: {quizTitle}\n" +
                                $"Date: {quizDate:dd MMM yyyy - hh:mm tt}\n" +
                                $"Max Mark: {maxMark}",

                            type = AttendanceSessionType.Quiz.ToString(),
                            createdAt = DateTime.UtcNow
                        });
                }
            }
        }

        public async Task CreateAbsenceWarningAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);

            if (student == null)
                return;

            var notification = new Notification
            {
                UserId = student.UserId,
                Title = "Warning: You exceeded the allowed absence limit",

                Message =
                    "Your absence percentage exceeded the allowed limit. " +
                    "Please contact your instructor.",

                Type = AttendanceSessionType.Section,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            await _hubContext.Clients
                .Group($"Student-{studentId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    createdAt = notification.CreatedAt
                });
        }

        public async Task CreateAttendanceUpdateAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);

            if (student == null)
                return;

            var notification = new Notification
            {
                UserId = student.UserId,
                Title = "Attendance Updated",

                Message =
                    "Your attendance records have been updated successfully.",

                Type = AttendanceSessionType.Section,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            await _hubContext.Clients
                .Group($"Student-{studentId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    createdAt = notification.CreatedAt
                });
        }

        public async Task NotifyQuizGradesUploadedAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.CourseSection)
                .ThenInclude(cs => cs.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return;

            var grades = await _context.QuizGrades
                .Include(g => g.Enrollment)
                .ThenInclude(e => e.Student)
                .Where(g => g.QuizId == quizId)
                .ToListAsync();

            foreach (var grade in grades)
            {
                var student = grade.Enrollment.Student;

                if (student == null)
                    continue;

                var notification = new Notification
                {
                    UserId = student.UserId,

                    Title = $"Quiz Grades Uploaded - {quiz.CourseSection.Course.Name}",

                    Message =
                        $"Quiz: {quiz.Title}\n" +
                        $"Your Mark: {grade.Mark} / {quiz.MaxMark}",

                    Type = AttendanceSessionType.Quiz,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);

                // SignalR Group
                await _hubContext.Clients
                    .Group($"Student-{student.Id}")
                    .SendAsync("ReceiveNotification", new
                    {
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type.ToString(),
                        createdAt = notification.CreatedAt
                    });

                // SignalR Direct User
                if (student.UserId > 0)
                {
                    await _hubContext.Clients
                        .User(student.UserId.ToString())
                        .SendAsync("ReceiveNotification", new
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