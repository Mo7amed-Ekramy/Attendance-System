using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Hubs;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.ViewModels.Doctor;
using MVC_PROJECT.ViewModels.Student;
using MVC_PROJECT.ViewModels.TA;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC_PROJECT.Services.Interfaces;


namespace MVC_PROJECT.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IGradeCalculationService _gradeCalculationService;
        private readonly IHubContext<DashboardHub> _hubContext;

        public DashboardService(AppDbContext context, IGradeCalculationService gradeCalculationService, IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _gradeCalculationService = gradeCalculationService;
            _hubContext = hubContext;
        }

        public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.DepartmentSection)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return new StudentDashboardViewModel();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            var courses = new List<StudentCourseItemViewModel>();
            var totalAbsences = 0;
            var totalCourseworkMark = 0m;

            foreach (var enrollment in enrollments)
            {
                var courseId = enrollment.CourseSection.CourseId;
                var enrollmentId = enrollment.Id;

                var absenceCount = await GetAbsenceCountAsync(enrollmentId, courseId);
                var policy = await _gradeCalculationService.GetCoursePolicyAsync(courseId);
                var allowedAbsences = policy?.AllowedAbsences ?? 0;

                var courseworkMark = await _gradeCalculationService.CalculateTotalCourseworkGradeAsync(enrollmentId, courseId);
                var isAbsentLimitExceeded = await _gradeCalculationService.IsAbsentLimitExceededAsync(enrollmentId, courseId);

                totalAbsences += absenceCount;
                totalCourseworkMark += courseworkMark;

                courses.Add(new StudentCourseItemViewModel
                {
                    CourseId = courseId,
                    CourseCode = enrollment.CourseSection.Course.Code,
                    CourseName = enrollment.CourseSection.Course.Name,
                    DoctorName = enrollment.CourseSection.Course.Doctor?.FullName ?? "Unknown",
                    SectionNumber = enrollment.CourseSection.DepartmentSection.Number,
                    AbsenceCount = absenceCount,
                    AllowedAbsences = allowedAbsences,
                    AbsenceStatus = isAbsentLimitExceeded ? "Exceeded" : "OK",
                    CourseworkMark = courseworkMark
                });
            }

            // Get recent notifications
            var studentNotifications = await _context.Notifications
                .Where(n => n.UserId == student.UserId)
                .OrderByDescending(n => n.Id)
                .Take(5)
                .ToListAsync();

            var recentNotifications = new List<StudentNotificationViewModel>();
            foreach (var notification in studentNotifications)
            {
                recentNotifications.Add(new StudentNotificationViewModel
                {
                    NotificationId = notification.Id,
                    Title = notification.Title,
                    Type = notification.Type.ToString(),
                    IsRead = notification.IsRead
                });
            }

            return new StudentDashboardViewModel
            {
                StudentId = student.Id,
                FullName = student.FullName,
                DepartmentName = student.Department?.Name ?? "Unknown",
                DepartmentSectionNumber = student.DepartmentSection?.Number ?? 0,
                Level = student.Level,
                TotalCourses = enrollments.Count,
                TotalAbsences = totalAbsences,
                AllowedAbsences = courses.Any() ? courses.Sum(c => c.AllowedAbsences) : 0,
                AbsenceStatus = "OK",
                TotalCourseworkMark = totalCourseworkMark,
                Courses = courses,
                RecentNotifications = recentNotifications
            };
        }

        public async Task<TADashboardViewModel> GetTADashboardAsync(int taId)
        {
            var ta = await _context.Users.FindAsync(taId);
            if (ta == null)
            {
                return new TADashboardViewModel();
            }

            var sections = await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.TAId == taId)
                .ToListAsync();

            var sectionItems = new List<TASectionItemViewModel>();
            var totalStudents = 0;

            foreach (var section in sections)
            {
                var pendingAttendanceSessions = await _context.AttendanceSessions
                    .CountAsync(s => s.CourseSectionId == section.Id && !s.IsClosed);

                var pendingQuizzes = await _context.Quizzes
                    .Include(q => q.QuizGrades)
                    .Where(q => q.CourseSectionId == section.Id)
                    .CountAsync(q => q.QuizGrades.Count == 0);

                totalStudents += section.Enrollments.Count;

                sectionItems.Add(new TASectionItemViewModel
                {
                    CourseSectionId = section.Id,
                    CourseId = section.CourseId,
                    CourseCode = section.Course.Code,
                    CourseName = section.Course.Name,
                    DepartmentName = section.DepartmentSection.Department.Name,
                    SectionNumber = section.DepartmentSection.Number,
                    StudentCount = section.Enrollments.Count,
                    PendingAttendanceSessions = pendingAttendanceSessions,
                    PendingQuizzes = pendingQuizzes
                });
            }

            return new TADashboardViewModel
            {
                TAId = taId,
                FullName = ta.FullName,
                DepartmentName = sectionItems.Any() ? sectionItems.First().DepartmentName : "Unknown",
                TotalSections = sections.Count,
                TotalStudents = totalStudents,
                PendingAttendanceSessions = sectionItems.Sum(s => s.PendingAttendanceSessions),
                Sections = sectionItems
            };
        }

        public async Task<DoctorDashboardViewModel> GetDoctorDashboardAsync(int doctorId)
        {
            var doctor = await _context.Users.FindAsync(doctorId);
            if (doctor == null)
            {
                return new DoctorDashboardViewModel();
            }

            var courses = await _context.Courses
                .Include(c => c.CourseSections)
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync();

            var courseItems = new List<DoctorCourseItemViewModel>();
            var totalSections = 0;
            var totalStudents = 0;

            foreach (var course in courses)
            {
                var courseSections = await _context.CourseSections
                    .Include(cs => cs.Enrollments)
                    .Where(cs => cs.CourseId == course.Id)
                    .ToListAsync();

                var sectionCount = courseSections.Sum(cs => cs.Enrollments.Count);
                totalSections += courseSections.Count;
                totalStudents += sectionCount;

                courseItems.Add(new DoctorCourseItemViewModel
                {
                    CourseId = course.Id,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    Semester = course.Semester,
                    TotalSections = courseSections.Count,
                    TotalStudents = sectionCount
                });
            }

            return new DoctorDashboardViewModel
            {
                DoctorId = doctorId,
                FullName = doctor.FullName,
                TotalCourses = courses.Count,
                TotalSections = totalSections,
                TotalStudents = totalStudents,
                Courses = courseItems
            };
        }

        private async Task<int> GetAbsenceCountAsync(int enrollmentId, int courseId)
        {
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            var sessionIds = await _context.AttendanceSessions
                .Where(s => sectionIds.Contains(s.CourseSectionId))
                .Select(s => s.Id)
                .ToListAsync();

            var records = await _context.AttendanceRecords
                .Where(r => r.EnrollmentId == enrollmentId && sessionIds.Contains(r.AttendanceSessionId))
                .ToListAsync();

            var totalSessions = sessionIds.Count;
            var presentCount = records.Count(r => r.IsPresent);

            return totalSessions - presentCount;
        }

        // Real-time notification methods for Student
        public async Task NotifyStudentAttendanceUpdated(int studentId)
        {
            await _hubContext.Clients.Group($"Student-{studentId}")
                .SendAsync("AttendanceUpdated", new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Your attendance has been updated"
                });
        }

        public async Task NotifyStudentQuizUpdated(int studentId)
        {
            await _hubContext.Clients.Group($"Student-{studentId}")
                .SendAsync("QuizUpdated", new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Your quiz grades have been updated"
                });
        }

        public async Task NotifyStudentAbsenceWarning(int studentId)
        {
            await _hubContext.Clients.Group($"Student-{studentId}")
                .SendAsync("AbsenceWarning", new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Warning: You have exceeded the allowed absence limit"
                });
        }

        // Real-time notification methods for TA
        public async Task NotifySectionAttendanceUpdated(int sectionId)
        {
            await _hubContext.Clients.Group($"Section-{sectionId}")
                .SendAsync("SectionAttendanceUpdated", new
                {
                    timestamp = DateTime.UtcNow,
                    sectionId = sectionId,
                    message = "Section attendance has been updated"
                });
        }

        public async Task NotifyStudentSubmittedAttendance(int sectionId)
        {
            await _hubContext.Clients.Group($"Section-{sectionId}")
                .SendAsync("StudentSubmittedAttendance", new
                {
                    timestamp = DateTime.UtcNow,
                    sectionId = sectionId,
                    message = "A student has submitted attendance"
                });
        }

        // Real-time notification methods for Doctor
        public async Task NotifyLectureAttendanceUpdated(int courseId)
        {
            await _hubContext.Clients.Group($"Doctor-{courseId}")
                .SendAsync("LectureAttendanceUpdated", new
                {
                    timestamp = DateTime.UtcNow,
                    courseId = courseId,
                    message = "Lecture attendance has been updated"
                });
        }

        public async Task NotifyCourseStatsUpdated(int courseId)
        {
            await _hubContext.Clients.Group($"Doctor-{courseId}")
                .SendAsync("CourseStatsUpdated", new
                {
                    timestamp = DateTime.UtcNow,
                    courseId = courseId,
                    message = "Course statistics have been updated"
                });
        }
    }
}
