namespace MVC_PROJECT.ViewModels.Doctor
{
    public class DoctorDashboardViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public int TotalCourses { get; set; }
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
        public List<DoctorCourseItemViewModel> Courses { get; set; } = new List<DoctorCourseItemViewModel>();
    }
}
