using MVC_PROJECT.Models;
using MVC_PROJECT.ViewModels.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<Student?> GetStudentByUserIdAsync(int userId);
        Task<Student?> GetStudentByUniversityIdAsync(string universityId);
        Task<List<Student>> GetStudentsByDepartmentAsync(int departmentId);
        Task<List<Student>> GetStudentsBySectionAsync(int departmentSectionId);
        Task<List<Student>> GetStudentsByCourseSectionAsync(int courseSectionId);
        Task<StudentDashboardViewModel> GetStudentDashboardAsync(int studentId);
        Task<List<StudentCourseItemViewModel>> GetStudentCoursesAsync(int studentId);
    }
}
