using System;
using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Attendance
{
    public class SectionAttendanceViewModel
    {
        public int CourseSectionId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public DateTime Date { get; set; }
        public bool IsLocked { get; set; }
        public List<SectionAttendanceStudentViewModel> Students { get; set; } = new List<SectionAttendanceStudentViewModel>();
    }

    public class SectionAttendanceStudentViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public bool IsPresent { get; set; }
    }

    public class SaveSectionAttendanceViewModel
    {
        public int AttendanceSessionId { get; set; }
        public int CourseSectionId { get; set; }
        public List<SectionAttendanceStudentViewModel> Students { get; set; } = new List<SectionAttendanceStudentViewModel>();
    }
}
