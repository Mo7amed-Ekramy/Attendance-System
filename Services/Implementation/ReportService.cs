using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.ViewModels.Reports;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC_PROJECT.Services.Interfaces;

namespace MVC_PROJECT.Services.Implementation
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IGradeCalculationService _gradeCalculationService;

        public ReportService(AppDbContext context, IGradeCalculationService gradeCalculationService)
        {
            _context = context;
            _gradeCalculationService = gradeCalculationService;
        }

        public async Task<SectionReportViewModel> GetSectionReportAsync(int courseSectionId)
        {
            var section = await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(cs => cs.Id == courseSectionId);

            if (section == null)
            {
                return new SectionReportViewModel();
            }

            var students = new List<SectionReportStudentViewModel>();

            foreach (var enrollment in section.Enrollments)
            {
                var courseId = section.CourseId;
                var enrollmentId = enrollment.Id;

                var presentCount = await GetPresentCountAsync(enrollmentId, courseId);
                var totalSessions = await GetTotalSessionsAsync(enrollmentId, courseId);
                var absentCount = totalSessions - presentCount;

                var policy = await _gradeCalculationService.GetCoursePolicyAsync(courseId);
                var allowedAbsences = policy?.AllowedAbsences ?? 0;

                var quizMark = await _gradeCalculationService.CalculateQuizGradeAsync(enrollmentId, courseId);
                var sectionAttendanceMark = await _gradeCalculationService.CalculateSectionAttendanceGradeAsync(enrollmentId, courseId);
                var totalCourseworkMark = await _gradeCalculationService.CalculateTotalCourseworkGradeAsync(enrollmentId, courseId);

                var isAbsentLimitExceeded = await _gradeCalculationService.IsAbsentLimitExceededAsync(enrollmentId, courseId);

                students.Add(new SectionReportStudentViewModel
                {
                    StudentId = enrollment.StudentId,
                    UniversityId = enrollment.Student.UniversityId,
                    FullName = enrollment.Student.FullName,
                    PresentCount = presentCount,
                    AbsenceCount = absentCount,
                    AllowedAbsences = allowedAbsences,
                    Status = isAbsentLimitExceeded ? "At Risk" : "OK",
                    QuizMark = quizMark,
                    SectionAttendanceMark = sectionAttendanceMark,
                    TotalCourseworkMark = totalCourseworkMark
                });
            }

            return new SectionReportViewModel
            {
                CourseSectionId = section.Id,
                CourseCode = section.Course.Code,
                CourseName = section.Course.Name,
                DepartmentName = section.DepartmentSection.Department.Name,
                SectionNumber = section.DepartmentSection.Number,
                TotalStudents = students.Count,
                Students = students
            };
        }

        public async Task<LectureReportViewModel> GetLectureReportAsync(int attendanceSessionId)
        {
            var session = await _context.AttendanceSessions
                .Include(s => s.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Include(s => s.Records)
                    .ThenInclude(r => r.Enrollment)
                        .ThenInclude(e => e.Student)
                            .ThenInclude(st => st.DepartmentSection)
                .FirstOrDefaultAsync(s => s.Id == attendanceSessionId);

            if (session == null)
            {
                return new LectureReportViewModel();
            }

            var courseSections = await _context.CourseSections
                .Where(cs => cs.CourseId == session.CourseSection.CourseId)
                .ToListAsync();

            var sectionIds = courseSections.Select(cs => cs.Id).ToList();

            var students = new List<LectureReportStudentViewModel>();

            foreach (var courseSection in courseSections)
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Student)
                    .Where(e => e.CourseSectionId == courseSection.Id)
                    .ToListAsync();

                foreach (var enrollment in enrollments)
                {
                    var record = session.Records.FirstOrDefault(r => r.EnrollmentId == enrollment.Id);
                    var isPresent = record != null && record.IsPresent;

                    students.Add(new LectureReportStudentViewModel
                    {
                        StudentId = enrollment.StudentId,
                        UniversityId = enrollment.Student.UniversityId,
                        FullName = enrollment.Student.FullName,
                        DepartmentName = enrollment.Student.Department?.Name ?? "Unknown",
                        SectionNumber = enrollment.Student.DepartmentSection?.Number ?? 0,
                        IsPresent = isPresent,
                        SubmittedAt = record?.AttendanceSession?.Date ?? DateTime.MinValue
                    });
                }
            }

            var presentCount = session.Records.Count(r => r.IsPresent);
            var absentCount = students.Count - presentCount;

            return new LectureReportViewModel
            {
                CourseId = session.CourseSection.CourseId,
                CourseCode = session.CourseSection.Course.Code,
                CourseName = session.CourseSection.Course.Name,
                LectureDate = session.Date,
                AttendanceCode = session.AttendanceCode,
                TotalStudents = students.Count,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                Students = students
            };
        }

        public async Task<CourseReportViewModel> GetCourseReportAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.DepartmentSection)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return new CourseReportViewModel();
            }

            var students = new List<CourseReportStudentViewModel>();
            var allEnrollments = new List<Enrollment>();

            foreach (var section in course.CourseSections)
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Student)
                        .ThenInclude(s => s.Department)
                    .Include(e => e.CourseSection)
                        .ThenInclude(cs => cs.DepartmentSection)
                    .Where(e => e.CourseSectionId == section.Id)
                    .ToListAsync();

                allEnrollments.AddRange(enrollments);
            }

            foreach (var enrollment in allEnrollments)
            {
                var enrollmentId = enrollment.Id;
                var presentCount = await GetPresentCountAsync(enrollmentId, courseId);
                var totalSessions = await GetTotalSessionsAsync(enrollmentId, courseId);
                var absentCount = totalSessions - presentCount;

                var policy = await _gradeCalculationService.GetCoursePolicyAsync(courseId);
                var allowedAbsences = policy?.AllowedAbsences ?? 0;

                var quizMark = await _gradeCalculationService.CalculateQuizGradeAsync(enrollmentId, courseId);
                var sectionAttendanceMark = await _gradeCalculationService.CalculateSectionAttendanceGradeAsync(enrollmentId, courseId);
                var lectureAttendanceMark = await _gradeCalculationService.CalculateLectureAttendanceGradeAsync(enrollmentId, courseId);
                var totalCourseworkMark = await _gradeCalculationService.CalculateTotalCourseworkGradeAsync(enrollmentId, courseId);

                var isAbsentLimitExceeded = await _gradeCalculationService.IsAbsentLimitExceededAsync(enrollmentId, courseId);

                students.Add(new CourseReportStudentViewModel
                {
                    StudentId = enrollment.StudentId,
                    UniversityId = enrollment.Student.UniversityId,
                    FullName = enrollment.Student.FullName,
                    DepartmentName = enrollment.Student.Department?.Name ?? "Unknown",
                    SectionNumber = enrollment.CourseSection.DepartmentSection.Number,
                    PresentCount = presentCount,
                    AbsenceCount = absentCount,
                    AllowedAbsences = allowedAbsences,
                    QuizMark = quizMark,
                    SectionAttendanceMark = sectionAttendanceMark,
                    LectureAttendanceMark = lectureAttendanceMark,
                    TotalCourseworkMark = totalCourseworkMark,
                    Status = isAbsentLimitExceeded ? "At Risk" : "OK"
                });
            }

            return new CourseReportViewModel
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                TotalSections = course.CourseSections.Count,
                TotalStudents = students.Count,
                Students = students
            };
        }

        public async Task<StudentCourseReportViewModel> GetStudentCourseReportAsync(int studentId, int courseId)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return new StudentCourseReportViewModel();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.DepartmentSection)
                .Where(e => e.StudentId == studentId && e.CourseSection.CourseId == courseId)
                .ToListAsync();

            var courses = new List<StudentCourseReportItemViewModel>();

            foreach (var enrollment in enrollments)
            {
                var enrollmentId = enrollment.Id;
                var presentCount = await GetPresentCountAsync(enrollmentId, courseId);
                var totalSessions = await GetTotalSessionsAsync(enrollmentId, courseId);
                var absentCount = totalSessions - presentCount;

                var policy = await _gradeCalculationService.GetCoursePolicyAsync(courseId);
                var allowedAbsences = policy?.AllowedAbsences ?? 0;

                var quizMark = await _gradeCalculationService.CalculateQuizGradeAsync(enrollmentId, courseId);
                var sectionAttendanceMark = await _gradeCalculationService.CalculateSectionAttendanceGradeAsync(enrollmentId, courseId);
                var lectureAttendanceMark = await _gradeCalculationService.CalculateLectureAttendanceGradeAsync(enrollmentId, courseId);
                var totalCourseworkMark = await _gradeCalculationService.CalculateTotalCourseworkGradeAsync(enrollmentId, courseId);

                var isAbsentLimitExceeded = await _gradeCalculationService.IsAbsentLimitExceededAsync(enrollmentId, courseId);

                courses.Add(new StudentCourseReportItemViewModel
                {
                    CourseId = enrollment.CourseSection.CourseId,
                    CourseCode = enrollment.CourseSection.Course.Code,
                    CourseName = enrollment.CourseSection.Course.Name,
                    SectionNumber = enrollment.CourseSection.DepartmentSection.Number,
                    PresentCount = presentCount,
                    AbsenceCount = absentCount,
                    AllowedAbsences = allowedAbsences,
                    QuizMark = quizMark,
                    SectionAttendanceMark = sectionAttendanceMark,
                    LectureAttendanceMark = lectureAttendanceMark,
                    TotalCourseworkMark = totalCourseworkMark,
                    Status = isAbsentLimitExceeded ? "At Risk" : "OK"
                });
            }

            return new StudentCourseReportViewModel
            {
                StudentId = student.Id,
                UniversityId = student.UniversityId,
                FullName = student.FullName,
                Level = student.Level,
                DepartmentName = student.Department?.Name ?? "Unknown",
                Courses = courses
            };
        }

        public async Task<List<CourseReportStudentViewModel>> GetAtRiskStudentsAsync(int courseId)
        {
            var courseReport = await GetCourseReportAsync(courseId);
            return courseReport.Students
                .Where(s => s.Status == "At Risk")
                .ToList();
        }

        private async Task<int> GetPresentCountAsync(int enrollmentId, int courseId)
        {
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            var sessionIds = await _context.AttendanceSessions
                .Where(s => sectionIds.Contains(s.CourseSectionId))
                .Select(s => s.Id)
                .ToListAsync();

            return await _context.AttendanceRecords
                .CountAsync(r => r.EnrollmentId == enrollmentId && sessionIds.Contains(r.AttendanceSessionId) && r.IsPresent);
        }

        private async Task<int> GetTotalSessionsAsync(int enrollmentId, int courseId)
        {
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            return await _context.AttendanceSessions
                .CountAsync(s => sectionIds.Contains(s.CourseSectionId));
        }
    }
}
