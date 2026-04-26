namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentCourseDetailsViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string DoctorName { get; set; }
        public int SectionNumber { get; set; }
        public int AbsenceCount { get; set; }
        public int AllowedAbsences { get; set; }
        public string AbsenceStatus { get; set; }
        public int TotalCourseworkMarks { get; set; }
        public int SectionAttendanceMarks { get; set; }
        public int QuizMarks { get; set; }
        public int LectureAttendanceMarks { get; set; }
        public int BestQuizzesCount { get; set; }
    }
}
