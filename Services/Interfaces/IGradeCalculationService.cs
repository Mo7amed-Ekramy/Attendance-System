using MVC_PROJECT.Models;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IGradeCalculationService
    {
        Task<CoursePolicy?> GetCoursePolicyAsync(int courseId);
        Task<decimal> CalculateSectionAttendanceGradeAsync(int enrollmentId, int courseId);
        Task<decimal> CalculateLectureAttendanceGradeAsync(int enrollmentId, int courseId);
        Task<decimal> CalculateQuizGradeAsync(int enrollmentId, int courseId);
        Task<decimal> CalculateTotalCourseworkGradeAsync(int enrollmentId, int courseId);
        Task<bool> IsAbsentLimitExceededAsync(int enrollmentId, int courseId);
    }
}
