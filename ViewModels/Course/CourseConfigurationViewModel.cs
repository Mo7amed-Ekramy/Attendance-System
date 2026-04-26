using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.ViewModels.Course
{
    public class CourseConfigurationViewModel
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public int SectionAttendanceMarks { get; set; }

        public int QuizMarks { get; set; }

        public int LectureAttendanceMarks { get; set; }

        public int AllowedAbsences { get; set; }

        public int BestQuizzesCount { get; set; }
    }

    public class SaveCourseConfigurationViewModel
    {
        public int CourseId { get; set; }

        [Range(0, 100, ErrorMessage = "Section attendance marks must be between 0 and 100.")]
        public int SectionAttendanceMarks { get; set; }

        [Range(0, 100, ErrorMessage = "Quiz marks must be between 0 and 100.")]
        public int QuizMarks { get; set; }

        [Range(0, 100, ErrorMessage = "Lecture attendance marks must be between 0 and 100.")]
        public int LectureAttendanceMarks { get; set; }

        [Range(0, 100, ErrorMessage = "Allowed absences must be between 0 and 100.")]
        public int AllowedAbsences { get; set; }

        [Range(0, 50, ErrorMessage = "Best quizzes count must be between 0 and 50.")]
        public int BestQuizzesCount { get; set; }
    }
}