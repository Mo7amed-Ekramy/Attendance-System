namespace MVC_PROJECT.ViewModels.Doctor
{
    public class DoctorCourseItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Semester { get; set; }
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
    }
}
