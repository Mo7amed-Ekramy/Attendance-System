namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentCourseItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string DoctorName { get; set; }
        public int SectionNumber { get; set; }
        public int AbsenceCount { get; set; }
        public int AllowedAbsences { get; set; }
        public string AbsenceStatus { get; set; }
        public decimal CourseworkMark { get; set; }
    }
}
