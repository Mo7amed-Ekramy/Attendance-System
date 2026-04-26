using System;
using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.Reports
{
    public class SectionReportViewModel
    {
        public int CourseSectionId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public int TotalStudents { get; set; }
        public List<SectionReportStudentViewModel> Students { get; set; } = new List<SectionReportStudentViewModel>();
    }

    public class SectionReportStudentViewModel
    {
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public int PresentCount { get; set; }
        public int AbsenceCount { get; set; }
        public int AllowedAbsences { get; set; }
        public string Status { get; set; }
        public decimal QuizMark { get; set; }
        public decimal SectionAttendanceMark { get; set; }
        public decimal TotalCourseworkMark { get; set; }
    }

    public class LectureReportViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime LectureDate { get; set; }
        public string? AttendanceCode { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public List<LectureReportStudentViewModel> Students { get; set; } = new List<LectureReportStudentViewModel>();
    }

    public class LectureReportStudentViewModel
    {
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public bool IsPresent { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class CourseReportViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
        public List<CourseReportStudentViewModel> Students { get; set; } = new List<CourseReportStudentViewModel>();
    }

    public class CourseReportStudentViewModel
    {
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public int SectionNumber { get; set; }
        public int PresentCount { get; set; }
        public int AbsenceCount { get; set; }
        public int AllowedAbsences { get; set; }
        public decimal QuizMark { get; set; }
        public decimal SectionAttendanceMark { get; set; }
        public decimal LectureAttendanceMark { get; set; }
        public decimal TotalCourseworkMark { get; set; }
        public string Status { get; set; }
    }

    public class StudentCourseReportViewModel
    {
        public int StudentId { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public int Level { get; set; }
        public string DepartmentName { get; set; }
        public List<StudentCourseReportItemViewModel> Courses { get; set; } = new List<StudentCourseReportItemViewModel>();
    }

    public class StudentCourseReportItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int SectionNumber { get; set; }
        public int PresentCount { get; set; }
        public int AbsenceCount { get; set; }
        public int AllowedAbsences { get; set; }
        public decimal QuizMark { get; set; }
        public decimal SectionAttendanceMark { get; set; }
        public decimal LectureAttendanceMark { get; set; }
        public decimal TotalCourseworkMark { get; set; }
        public string Status { get; set; }
    }
}
