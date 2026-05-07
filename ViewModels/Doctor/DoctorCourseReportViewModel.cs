namespace MVC_PROJECT.ViewModels.Doctor
{
    public class DoctorCourseReportViewModel
    {
        public int CourseId { get; set; }

        public string CourseCode { get; set; } = "";

        public string CourseName { get; set; } = "";

        public List<DoctorStudentReportItemViewModel> Students { get; set; }
            = new();
    }

    public class DoctorStudentReportItemViewModel
    {
        public string StudentName { get; set; } = "";

        public string UniversityId { get; set; } = "";

        public int SectionNumber { get; set; }

        public int TotalSessions { get; set; }

        public int PresentCount { get; set; }

        public int AbsentCount { get; set; }

        public double AttendanceRate { get; set; }

        public string Status { get; set; } = "";
    }
}