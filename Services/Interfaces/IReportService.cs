using MVC_PROJECT.ViewModels.Reports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IReportService
    {
        Task<SectionReportViewModel> GetSectionReportAsync(int courseSectionId);
        Task<LectureReportViewModel> GetLectureReportAsync(int attendanceSessionId);
        Task<CourseReportViewModel> GetCourseReportAsync(int courseId);
        Task<StudentCourseReportViewModel> GetStudentCourseReportAsync(int studentId, int courseId);
        Task<List<CourseReportStudentViewModel>> GetAtRiskStudentsAsync(int courseId);
    }
}