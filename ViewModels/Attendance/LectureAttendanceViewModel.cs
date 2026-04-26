using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Attendance
{
    public class LectureAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime Date { get; set; }
        public string AttendanceCode { get; set; }
        public bool IsLocked { get; set; }
        public List<LectureAttendanceStudentViewModel> Students { get; set; } = new List<LectureAttendanceStudentViewModel>();
    }

    public class LectureAttendanceStudentViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public bool IsPresent { get; set; }
    }

    public class SaveLectureAttendanceViewModel
    {
        public int AttendanceSessionId { get; set; }
        public int CourseSectionId { get; set; }
        public List<LectureAttendanceStudentViewModel> Students { get; set; } = new List<LectureAttendanceStudentViewModel>();
    }

    public class SubmitAttendanceCodeViewModel
    {
        public int AttendanceSessionId { get; set; }
        public int StudentId { get; set; }
        public string Code { get; set; }
    }
}
