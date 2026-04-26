using System;

namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentAttendanceRecordViewModel
    {
        public int AttendanceSessionId { get; set; }
        public string SessionType { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public string? AttendanceCode { get; set; }
    }
}
