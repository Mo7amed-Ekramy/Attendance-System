using System;
using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.TA
{
    public class TASectionDetailsViewModel
    {
        public int CourseSectionId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public int StudentCount { get; set; }
        public List<AttendanceSessionSummaryViewModel> SectionAttendanceSessions { get; set; } = new List<AttendanceSessionSummaryViewModel>();
        public List<QuizSummaryViewModel> Quizzes { get; set; } = new List<QuizSummaryViewModel>();
    }

    public class AttendanceSessionSummaryViewModel
    {
        public int AttendanceSessionId { get; set; }
        public string SessionType { get; set; }
        public DateTime Date { get; set; }
        public string? AttendanceCode { get; set; }
        public bool IsClosed { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

    public class QuizSummaryViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public DateTime Date { get; set; }
        public decimal MaxMark { get; set; }
        public int SubmittedCount { get; set; }
        public int TotalStudents { get; set; }
        public bool IsClosed { get; set; }
    }
}
