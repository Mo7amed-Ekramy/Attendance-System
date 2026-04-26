namespace MVC_PROJECT.ViewModels.Doctor
{
    public class DoctorCourseDetailsViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Semester { get; set; }
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
        public List<DoctorSectionItemViewModel> Sections { get; set; } = new List<DoctorSectionItemViewModel>();
    }

    public class DoctorSectionItemViewModel
    {
        public int CourseSectionId { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public string TAName { get; set; }
        public int StudentCount { get; set; }
        public int TotalAttendanceSessions { get; set; }
        public int TotalQuizzes { get; set; }
    }
}
