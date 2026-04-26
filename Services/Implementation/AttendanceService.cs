using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Attendance;
using MVC_PROJECT.ViewModels.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Implementation
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;
        private readonly IDashboardService _dashboardService;

        public AttendanceService(AppDbContext context, IDashboardService dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }


        public async Task<StudentAttendanceHistoryViewModel> GetStudentAttendanceAsync(int courseId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.CoursePolicy)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.DepartmentSection)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseSection.CourseId == courseId);

            if (enrollment == null)
                return new StudentAttendanceHistoryViewModel();

            var records = await _context.AttendanceRecords
                .Include(r => r.AttendanceSession)
                .Where(r => r.EnrollmentId == enrollment.Id)
                .OrderBy(r => r.AttendanceSession.Date)
                .ToListAsync();

            int sectionAttendance = records.Count(r =>
                r.AttendanceSession.SessionType == AttendanceSessionType.Section && r.IsPresent);

            int sectionAbsent = records.Count(r =>
                r.AttendanceSession.SessionType == AttendanceSessionType.Section && !r.IsPresent);

            int lectureAttendance = records.Count(r =>
                r.AttendanceSession.SessionType == AttendanceSessionType.Lecture && r.IsPresent);

            int lectureAbsent = records.Count(r =>
                r.AttendanceSession.SessionType == AttendanceSessionType.Lecture && !r.IsPresent);

            int totalAbsences = sectionAbsent + lectureAbsent;

            int allowedAbsences = enrollment.CourseSection.Course.CoursePolicy?.AllowedAbsences ?? 0;

            return new StudentAttendanceHistoryViewModel
            {
                CourseId = enrollment.CourseSection.Course.Id,
                CourseCode = enrollment.CourseSection.Course.Code,
                CourseName = enrollment.CourseSection.Course.Name,
                SectionNumber = enrollment.CourseSection.DepartmentSection.Number,

                SectionAttendanceCount = sectionAttendance,
                SectionAbsentCount = sectionAbsent,
                LectureAttendanceCount = lectureAttendance,
                LectureAbsentCount = lectureAbsent,

                TotalAbsences = totalAbsences,
                AllowedAbsences = allowedAbsences,
                AbsenceStatus = totalAbsences >= allowedAbsences ? "At Risk" : "Safe",

                AttendanceRecords = records.Select(r => new StudentAttendanceRecordViewModel
                {
                    AttendanceSessionId = r.AttendanceSessionId,
                    SessionType = r.AttendanceSession.SessionType.ToString(),
                    Date = r.AttendanceSession.Date,
                    IsPresent = r.IsPresent,
                    AttendanceCode = r.AttendanceSession.AttendanceCode
                }).ToList()
            };
        }
        public async Task<SectionAttendanceViewModel> GetSectionAttendanceViewModelAsync(int sectionId)
        {
            var section = await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(cs => cs.Id == sectionId);

            if (section == null)
                return new SectionAttendanceViewModel();

            var students = section.Enrollments.Select(e => new SectionAttendanceStudentViewModel
            {
                EnrollmentId = e.Id,
                FullName = e.Student.FullName,
                IsPresent = false
            }).ToList();

            return new SectionAttendanceViewModel
            {
                CourseSectionId = section.Id,
                CourseName = section.Course.Name,
                CourseCode = section.Course.Code,
                DepartmentName = section.DepartmentSection.Department.Name,
                SectionNumber = section.DepartmentSection.Number,
                Date = DateTime.Now,
                IsLocked = false,
                Students = students
            };
        }
        public async Task<AttendanceSession?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.AttendanceSessions
                .Include(s => s.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Include(s => s.Records)
                    .ThenInclude(r => r.Enrollment)
                        .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<List<AttendanceSession>> GetSessionsBySectionAsync(int courseSectionId)
        {
            return await _context.AttendanceSessions
                .Where(s => s.CourseSectionId == courseSectionId)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<List<AttendanceSession>> GetLectureSessionsByCourseAsync(int courseId)
        {
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            return await _context.AttendanceSessions
                .Where(s => sectionIds.Contains(s.CourseSectionId) && s.SessionType == AttendanceSessionType.Lecture)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetRecordsBySessionAsync(int sessionId)
        {
            return await _context.AttendanceRecords
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Student)
                .Where(r => r.AttendanceSessionId == sessionId)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetRecordsByStudentCourseAsync(int studentId, int courseId)
        {
            var enrollmentIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Join(_context.CourseSections.Where(cs => cs.CourseId == courseId),
                    e => e.CourseSectionId,
                    cs => cs.Id,
                    (e, cs) => e.Id)
                .ToListAsync();

            var sessionIds = await _context.AttendanceSessions
                .Where(s => enrollmentIds.Contains(s.CourseSectionId))
                .Select(s => s.Id)
                .ToListAsync();

            return await _context.AttendanceRecords
                .Include(r => r.AttendanceSession)
                .Where(r => enrollmentIds.Contains(r.EnrollmentId) && sessionIds.Contains(r.AttendanceSessionId))
                .ToListAsync();
        }

        public async Task<AttendanceSession> CreateSessionAsync(int courseSectionId,AttendanceSessionType sessionType,AttendanceMethod method,string? attendanceCode = null)
        {
            var courseSection = await _context.CourseSections.FindAsync(courseSectionId);

            if (courseSection == null)
            {
                throw new ArgumentException($"CourseSection with ID {courseSectionId} not found.");
            }

            var session = new AttendanceSession
            {
                Date = DateTime.UtcNow,
                SessionType = sessionType,
                Method = method,
                CourseSectionId = courseSectionId,
                AttendanceCode = attendanceCode,
                IsClosed = false
            };

            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<LectureAttendanceViewModel> GetLectureAttendanceViewModelAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Enrollments)
                        .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return new LectureAttendanceViewModel();

            var students = course.CourseSections
                .SelectMany(cs => cs.Enrollments)
                .GroupBy(e => e.StudentId)
                .Select(g => g.First())
                .Select(e => new LectureAttendanceStudentViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.Student.Id,
                    UniversityId = e.Student.UniversityId,
                    FullName = e.Student.FullName,
                    IsPresent = false
                })
                .ToList();

            return new LectureAttendanceViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                Date = DateTime.Now,
                AttendanceCode = string.Empty,
                IsLocked = false,
                Students = students
            };
        }
        public async Task<string> GenerateAttendanceCodeAsync(int sessionId)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);

            if (session == null)
                throw new ArgumentException($"AttendanceSession with ID {sessionId} not found.");

            if (session.IsClosed)
                throw new InvalidOperationException("Cannot generate code for a closed session.");

            string code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            session.AttendanceCode = code;

            await _context.SaveChangesAsync();

            return code;
        }

        public async Task CloseSessionAsync(int sessionId)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null)
            {
                throw new ArgumentException($"AttendanceSession with ID {sessionId} not found.");
            }

            session.IsClosed = true;
            await _context.SaveChangesAsync();
        }

        public async Task SaveAttendanceRecordsAsync(int sessionId, List<AttendanceRecord> records)
        {
            var session = await _context.AttendanceSessions
                .Include(s => s.Records)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                throw new ArgumentException($"AttendanceSession with ID {sessionId} not found.");
            }

            if (session.IsClosed)
            {
                throw new InvalidOperationException($"Cannot save attendance records for closed session {sessionId}.");
            }

            var enrollmentIds = records.Select(r => r.EnrollmentId).Distinct().ToList();
            var enrollments = await _context.Enrollments
                .Where(e => enrollmentIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id);

            foreach (var record in records)
            {
                if (!enrollments.ContainsKey(record.EnrollmentId))
                {
                    throw new ArgumentException($"Enrollment with ID {record.EnrollmentId} not found.");
                }
            }

            _context.AttendanceRecords.RemoveRange(session.Records);

            foreach (var record in records)
            {
                record.AttendanceSessionId = sessionId;
                _context.AttendanceRecords.Add(record);
            }

            await _context.SaveChangesAsync();

            // Notify students that their attendance has been updated
            foreach (var record in records)
            {
                await _dashboardService.NotifyStudentAttendanceUpdated(record.EnrollmentId);
            }

            // Notify TA that section attendance has been updated
            await _dashboardService.NotifySectionAttendanceUpdated(session.CourseSectionId);
        }

        public async Task<bool> SubmitAttendanceCodeAsync(int sessionId, string code, int enrollmentId)
        {
            var session = await _context.AttendanceSessions.FindAsync(sessionId);
            if (session == null)
            {
                return false;
            }

            if (session.IsClosed)
            {
                return false;
            }

            if (session.AttendanceCode == null || !session.AttendanceCode.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return false;
            }

            // Check if student is enrolled in the course section
            if (enrollment.CourseSectionId != session.CourseSectionId)
            {
                return false;
            }

            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(r => r.AttendanceSessionId == sessionId && r.EnrollmentId == enrollmentId);

            if (existingRecord != null)
            {
                return false;
            }

            var record = new AttendanceRecord
            {
                AttendanceSessionId = sessionId,
                EnrollmentId = enrollmentId,
                IsPresent = true
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
