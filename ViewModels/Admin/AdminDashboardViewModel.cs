using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Counts
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalTAs { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCourses { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalSections { get; set; }
        public int TotalEnrollments { get; set; }

        // Recent items
        public List<AdminUserItemViewModel> RecentUsers { get; set; } = new List<AdminUserItemViewModel>();
        public List<AdminCourseItemViewModel> RecentCourses { get; set; } = new List<AdminCourseItemViewModel>();
    }
}
