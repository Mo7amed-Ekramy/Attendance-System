namespace MVC_PROJECT.ViewModels.TA
{
    public class TASectionItemViewModel
    {
        public int CourseSectionId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public int StudentCount { get; set; }
        public int PendingAttendanceSessions { get; set; }
        public int PendingQuizzes { get; set; }
    }
}
