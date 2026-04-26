using MVC_PROJECT.Models;
using MVC_PROJECT.ViewModels.Student;
using MVC_PROJECT.ViewModels.TA;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface ISectionService
    {
        Task<CourseSection?> GetSectionByIdAsync(int sectionId);
        Task<List<CourseSection>> GetSectionsByTAAsync(int taId);
        Task<List<CourseSection>> GetSectionsByCourseAsync(int courseId);
        Task<Enrollment?> GetEnrollmentByStudentAndSectionAsync(int studentId, int courseSectionId);
        Task<List<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId);
        Task<bool> IsTAAssignedToSectionAsync(int taId, int sectionId);

        // TA-specific methods
        Task<List<TASectionItemViewModel>> GetTASectionsAsync(int taId);
        Task<TASectionDetailsViewModel> GetSectionDetailsAsync(int sectionId);
    }
}
