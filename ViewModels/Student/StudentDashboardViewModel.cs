using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentSectionNumber { get; set; }
        public int Level { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAbsences { get; set; }
        public int AllowedAbsences { get; set; }
        public string AbsenceStatus { get; set; }
        public decimal TotalCourseworkMark { get; set; }
        public List<StudentCourseItemViewModel> Courses { get; set; } = new List<StudentCourseItemViewModel>();
        public List<StudentNotificationViewModel> RecentNotifications { get; set; } = new List<StudentNotificationViewModel>();
    }
}
