namespace MVC_PROJECT.ViewModels.TA
{
    public class SectionDetailsViewModel
    {
        public string CourseName { get; set; }

        public string SectionNumber { get; set; }

        public int StudentsCount { get; set; }

        public int QuizzesCount { get; set; }

        public double AttendanceRate { get; set; }

        public List<SectionStudentViewModel> Students { get; set; }

        public List<SectionQuizViewModel> Quizzes { get; set; }
    }

    public class SectionStudentViewModel
    {
        public string FullName { get; set; }

        public string UniversityId { get; set; }
    }

    public class SectionQuizViewModel
    {
        public int QuizId { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public decimal MaxMark { get; set; }

        public bool IsClosed { get; set; }
    }
}