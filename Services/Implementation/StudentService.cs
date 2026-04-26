using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.ViewModels.Student;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC_PROJECT.Services.Interfaces;

namespace MVC_PROJECT.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        private readonly IGradeCalculationService _gradeCalculationService;

        public StudentService(AppDbContext context, IGradeCalculationService gradeCalculationService)
        {
            _context = context;
            _gradeCalculationService = gradeCalculationService;
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.Department)
                .Include(s => s.DepartmentSection)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Student?> GetStudentByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.Department)
                .Include(s => s.DepartmentSection)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Student?> GetStudentByUniversityIdAsync(string universityId)
        {
            return await _context.Students
                .Include(s => s.Department)
                .Include(s => s.DepartmentSection)
                .FirstOrDefaultAsync(s => s.UniversityId == universityId);
        }

        public async Task<List<Student>> GetStudentsByDepartmentAsync(int departmentId)
        {
            return await _context.Students
                .Include(s => s.DepartmentSection)
                .Where(s => s.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsBySectionAsync(int departmentSectionId)
        {
            return await _context.Students
                .Where(s => s.DepartmentSectionId == departmentSectionId)
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByCourseSectionAsync(int courseSectionId)
        {
            var enrollmentIds = await _context.Enrollments
                .Where(e => e.CourseSectionId == courseSectionId)
                .Select(e => e.StudentId)
                .ToListAsync();

            return await _context.Students
                .Include(s => s.DepartmentSection)
                .Where(s => enrollmentIds.Contains(s.Id))
                .ToListAsync();
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
                RecentNotifications = new List<StudentNotificationViewModel>()
            };
        }

        public async Task<List<StudentCourseItemViewModel>> GetStudentCoursesAsync(int studentId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            var courses = new List<StudentCourseItemViewModel>();

            foreach (var enrollment in enrollments)
            {
                var courseId = enrollment.CourseSection.CourseId;
                var enrollmentId = enrollment.Id;

                var absenceCount = await GetAbsenceCountAsync(enrollmentId, courseId);
                var policy = await _gradeCalculationService.GetCoursePolicyAsync(courseId);
                var allowedAbsences = policy?.AllowedAbsences ?? 0;

                var courseworkMark = await _gradeCalculationService.CalculateTotalCourseworkGradeAsync(enrollmentId, courseId);
                var isAbsentLimitExceeded = await _gradeCalculationService.IsAbsentLimitExceededAsync(enrollmentId, courseId);

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

            return courses;
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
    }
}
