using System;
using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Quiz
{
    public class CreateQuizViewModel
    {
        public int CourseSectionId { get; set; }
        public string CourseName { get; set; }
        public int SectionNumber { get; set; }
        public string QuizTitle { get; set; }
        public decimal MaxMark { get; set; }
        public DateTime Date { get; set; }
    }

    public class RecordQuizMarksViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string CourseName { get; set; }
        public decimal MaxMark { get; set; }
        public bool IsClosed { get; set; }
        public List<QuizStudentMarkViewModel> Students { get; set; } = new List<QuizStudentMarkViewModel>();
    }

    public class QuizStudentMarkViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public decimal? Mark { get; set; }
    }

    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string CourseName { get; set; }
        public DateTime Date { get; set; }
        public decimal MaxMark { get; set; }
        public decimal AverageMark { get; set; }
        public decimal PercentageScore { get; set; }
        public List<QuizStudentMarkViewModel> Students { get; set; } = new List<QuizStudentMarkViewModel>();
    }
}
