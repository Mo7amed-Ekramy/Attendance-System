using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Doctor;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICourseService _courseService;

        public DoctorController(
            IDashboardService dashboardService,
            ICourseService courseService)
        {
            _dashboardService = dashboardService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int doctorId = GetCurrentDoctorId();

            if (doctorId <= 0)
                return Unauthorized("Unable to identify doctor.");

            var viewModel = await _dashboardService.GetDoctorDashboardAsync(doctorId);

            if (viewModel == null)
                return NotFound("Doctor dashboard data was not found.");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            int doctorId = GetCurrentDoctorId();

            if (doctorId <= 0)
                return Unauthorized("Unable to identify doctor.");

            var courses = await _courseService.GetCoursesByDoctorAsync(doctorId);
            var courseViewModels = new List<DoctorCourseItemViewModel>();

            foreach (var course in courses)
            {
                courseViewModels.Add(new DoctorCourseItemViewModel
                {
                    CourseId = course.Id,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    Semester = course.Semester,
                    TotalSections = await _courseService.GetSectionCountByCourseAsync(course.Id),
                    TotalStudents = await _courseService.GetStudentCountByCourseAsync(course.Id)
                });
            }

            return View(courseViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            int doctorId = GetCurrentDoctorId();

            if (doctorId <= 0)
                return Unauthorized("Unable to identify doctor.");

            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
                return NotFound($"Course with ID {courseId} not found.");

            if (course.DoctorId != doctorId)
                return Forbid();

            var viewModel = await _courseService.GetCourseDetailsForDoctorAsync(courseId);

            if (viewModel == null || viewModel.CourseId == 0)
                return NotFound("Course details were not found.");

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CourseReport(int courseId)
        {
            int doctorId = GetCurrentDoctorId();

            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
                return NotFound();

            if (course.DoctorId != doctorId)
                return Forbid();

            var report = await _courseService.GetCourseReportAsync(courseId);

            return View(report);
        }
        [HttpGet]
        public async Task<IActionResult> ExportCourseReport(int courseId)
        {
            int doctorId = GetCurrentDoctorId();

            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
                return NotFound();

            if (course.DoctorId != doctorId)
                return Forbid();

            var report = await _courseService.GetCourseReportAsync(courseId);

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Course Report");

            worksheet.Cell(1, 1).Value = "Student Name";
            worksheet.Cell(1, 2).Value = "University ID";
            worksheet.Cell(1, 3).Value = "Section";
            worksheet.Cell(1, 4).Value = "Total Sessions";
            worksheet.Cell(1, 5).Value = "Present";
            worksheet.Cell(1, 6).Value = "Absent";
            worksheet.Cell(1, 7).Value = "Attendance Rate";
            worksheet.Cell(1, 8).Value = "Status";

            int row = 2;

            foreach (var student in report.Students)
            {
                worksheet.Cell(row, 1).Value = student.StudentName;
                worksheet.Cell(row, 2).Value = student.UniversityId;
                worksheet.Cell(row, 3).Value = student.SectionNumber;
                worksheet.Cell(row, 4).Value = student.TotalSessions;
                worksheet.Cell(row, 5).Value = student.PresentCount;
                worksheet.Cell(row, 6).Value = student.AbsentCount;
                worksheet.Cell(row, 7).Value = $"{student.AttendanceRate}%";
                worksheet.Cell(row, 8).Value = student.Status;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();

            string fileName =
                $"{report.CourseCode}_Attendance_Report.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        private int GetCurrentDoctorId()
        {
            return User.GetUserId();
        }
    }
}