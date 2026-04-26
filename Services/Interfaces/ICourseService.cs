using MVC_PROJECT.Models;
using MVC_PROJECT.ViewModels.Course;
using MVC_PROJECT.ViewModels.Doctor;
using MVC_PROJECT.ViewModels.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface ICourseService
    {
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<List<Course>> GetCoursesByDoctorAsync(int doctorId);
        Task<List<Course>> GetCoursesByDepartmentAsync(int departmentId);
        Task<List<CourseDepartment>> GetCourseDepartmentsAsync(int courseId);
        Task<List<CourseSection>> GetCourseSectionsAsync(int courseId);
        Task<CourseConfigurationViewModel> GetCourseConfigurationAsync(int courseId);
        Task UpdateCourseConfigurationAsync(SaveCourseConfigurationViewModel viewModel);

        // Student-specific methods
        Task<List<StudentCourseItemViewModel>> GetStudentCoursesAsync(int studentId);
        Task<StudentCourseDetailsViewModel> GetCourseDetailsAsync(int courseId, int studentId);

        // Doctor-specific methods
        Task<DoctorCourseDetailsViewModel> GetCourseDetailsForDoctorAsync(int courseId);
        Task<int> GetSectionCountByCourseAsync(int courseId);
        Task<int> GetStudentCountByCourseAsync(int courseId);
    }
}
