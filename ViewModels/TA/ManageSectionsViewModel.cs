namespace MVC_PROJECT.ViewModels.TA
{
    public class ManageSectionsViewModel
    {
        public int SectionId { get; set; }

        public string CourseName { get; set; }

        public string SectionNumber { get; set; }

        public int StudentsCount { get; set; }

        public int QuizzesCount { get; set; }

        public double AttendanceRate { get; set; }
    }
}