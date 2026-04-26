using MVC_PROJECT.Models;
using MVC_PROJECT.ViewModels.Attendance;
using MVC_PROJECT.ViewModels.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceSession?> GetSessionByIdAsync(int sessionId);
        Task<List<AttendanceSession>> GetSessionsBySectionAsync(int courseSectionId);
        Task<List<AttendanceSession>> GetLectureSessionsByCourseAsync(int courseId);
        Task<List<AttendanceRecord>> GetRecordsBySessionAsync(int sessionId);
        Task<List<AttendanceRecord>> GetRecordsByStudentCourseAsync(int studentId, int courseId);
        Task<AttendanceSession> CreateSessionAsync(int courseSectionId,AttendanceSessionType sessionType,AttendanceMethod method,string? attendanceCode = null); 
        Task CloseSessionAsync(int sessionId);
        Task SaveAttendanceRecordsAsync(int sessionId, List<AttendanceRecord> records);
        Task<bool> SubmitAttendanceCodeAsync(int sessionId, string code, int enrollmentId);

        Task<string> GenerateAttendanceCodeAsync(int sessionId);
        // Student-specific methods
        Task<LectureAttendanceViewModel> GetLectureAttendanceViewModelAsync(int courseId);
        Task<StudentAttendanceHistoryViewModel> GetStudentAttendanceAsync(int courseId, int studentId);
        Task<SectionAttendanceViewModel> GetSectionAttendanceViewModelAsync(int sectionId);
    }
}
