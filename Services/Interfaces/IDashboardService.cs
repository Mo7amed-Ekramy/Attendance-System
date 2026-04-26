using MVC_PROJECT.ViewModels.Doctor;
using MVC_PROJECT.ViewModels.Student;
using MVC_PROJECT.ViewModels.TA;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<StudentDashboardViewModel> GetStudentDashboardAsync(int studentId);
        Task<TADashboardViewModel> GetTADashboardAsync(int taId);
        Task<DoctorDashboardViewModel> GetDoctorDashboardAsync(int doctorId);

        // Real-time notification methods for Student
        Task NotifyStudentAttendanceUpdated(int studentId);
        Task NotifyStudentQuizUpdated(int studentId);
        Task NotifyStudentAbsenceWarning(int studentId);

        // Real-time notification methods for TA
        Task NotifySectionAttendanceUpdated(int sectionId);
        Task NotifyStudentSubmittedAttendance(int sectionId);

        // Real-time notification methods for Doctor
        Task NotifyLectureAttendanceUpdated(int courseId);
        Task NotifyCourseStatsUpdated(int courseId);
    }
}
