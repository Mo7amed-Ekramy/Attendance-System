using MVC_PROJECT.ViewModels.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetAdminDashboardAsync();
        Task<List<AdminUserItemViewModel>> GetUsersAsync();
        Task<List<AdminStudentItemViewModel>> GetStudentsAsync();
        Task<List<AdminDoctorItemViewModel>> GetDoctorsAsync();
        Task<List<AdminTAItemViewModel>> GetTAsAsync();
        Task<List<AdminCourseItemViewModel>> GetCoursesAsync();
        Task<List<AdminSectionItemViewModel>> GetSectionsAsync();
        Task<AdminUserItemViewModel?> GetUserByIdAsync(int userId);

        // User CRUD
        Task CreateUserAsync(CreateUserDto dto);
        Task UpdateUserAsync(UpdateUserDto dto);
        Task DeleteUserAsync(int id);

        // Student CRUD
        Task<AdminStudentItemViewModel?> GetStudentByIdAsync(int id);
        Task CreateStudentAsync(CreateStudentDto dto);
        Task UpdateStudentAsync(UpdateStudentDto dto);
        Task DeleteStudentAsync(int id);

        // Doctor CRUD (Users with Role.Doctor)
        Task<AdminDoctorItemViewModel?> GetDoctorByIdAsync(int id);
        Task CreateDoctorAsync(CreateUserDto dto);
        Task UpdateDoctorAsync(UpdateUserDto dto);
        Task DeleteDoctorAsync(int id);

        // TA CRUD (Users with Role.TA)
        Task<AdminTAItemViewModel?> GetTAByIdAsync(int id);
        Task CreateTAAsync(CreateUserDto dto);
        Task UpdateTAAsync(UpdateUserDto dto);
        Task DeleteTAAsync(int id);

        // Course CRUD
        Task<AdminCourseItemViewModel?> GetCourseByIdAsync(int id);
        Task CreateCourseAsync(CreateCourseDto dto);
        Task UpdateCourseAsync(UpdateCourseDto dto);
        Task DeleteCourseAsync(int id);

        // Section CRUD
        Task<AdminSectionItemViewModel?> GetSectionByIdAsync(int id);
        Task CreateSectionAsync(CreateSectionDto dto);
        Task UpdateSectionAsync(UpdateSectionDto dto);
        Task DeleteSectionAsync(int id);
    }
}
