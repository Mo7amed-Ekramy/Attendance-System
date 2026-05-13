using System;
using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Attendance
{
    public class StudentAttendanceHistoryViewModel
    {
        public int CourseId { get; set; }

        public string CourseCode { get; set; }

        public string CourseName { get; set; }

        public string DepartmentName { get; set; }

        public int SectionNumber { get; set; }

        public int SectionAttendanceCount { get; set; }

        public int SectionAbsentCount { get; set; }

        public int LectureAttendanceCount { get; set; }

        public int LectureAbsentCount { get; set; }

        public int TotalAbsences { get; set; }

        public int AllowedAbsences { get; set; }

        public string AbsenceStatus { get; set; }

        public List<StudentAttendanceRecordViewModel> AttendanceRecords { get; set; }
            = new List<StudentAttendanceRecordViewModel>();
    }

    public class StudentAttendanceRecordViewModel
    {
        public int AttendanceSessionId { get; set; }

        public string SessionType { get; set; }

        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }

        public string? AttendanceCode { get; set; }
    }
}