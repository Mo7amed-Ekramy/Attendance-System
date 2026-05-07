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
        public List<StudentUpcomingSessionViewModel> UpcomingSessions { get; set; }
    = new List<StudentUpcomingSessionViewModel>();

        public List<StudentTeachingTeamViewModel> TeachingTeam { get; set; }
            = new List<StudentTeachingTeamViewModel>();
        public List<StudentQuizItemViewModel> QuizItems { get; set; }
    = new List<StudentQuizItemViewModel>();
    }
    public class StudentUpcomingSessionViewModel
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
    }

    public class StudentTeachingTeamViewModel
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
    public class StudentQuizItemViewModel
    {
        public string QuizTitle { get; set; }

        public DateTime QuizDate { get; set; }

        public int StudentMark { get; set; }

        public int MaxMark { get; set; }

        public bool IsCounted { get; set; }
    }
}
