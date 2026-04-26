using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentQuizMarksViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int SectionNumber { get; set; }
        public int QuizMarks { get; set; }
        public int BestQuizzesCount { get; set; }
        public decimal AverageQuizMark { get; set; }
        public List<StudentQuizMarkItemViewModel> Quizzes { get; set; } = new List<StudentQuizMarkItemViewModel>();
    }
}
