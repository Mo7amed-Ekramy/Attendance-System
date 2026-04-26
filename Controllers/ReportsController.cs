using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Reports;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "Admin,Doctor,TA")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("/Reports/Section/{sectionId}")]
        [Authorize(Roles = "Admin,TA")]
        public async Task<IActionResult> Section(int sectionId)
        {
            if (sectionId <= 0)
                return BadRequest("Invalid section ID.");

            var report = await _reportService.GetSectionReportAsync(sectionId);
            return View(report);
        }

        [HttpGet("/Reports/Lecture/{sessionId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Lecture(int sessionId)
        {
            if (sessionId <= 0)
                return BadRequest("Invalid session ID.");

            var report = await _reportService.GetLectureReportAsync(sessionId);
            return View(report);
        }

        [HttpGet("/Reports/Course/{courseId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Course(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            var report = await _reportService.GetCourseReportAsync(courseId);
            return View(report);
        }

        [HttpGet("/Reports/Student/{studentId}/{courseId}")]
        [Authorize(Roles = "Admin,Doctor,TA")]
        public async Task<IActionResult> Student(int studentId, int courseId)
        {
            if (studentId <= 0 || courseId <= 0)
                return BadRequest("Invalid student ID or course ID.");

            var report = await _reportService.GetStudentCourseReportAsync(studentId, courseId);
            return View(report);
        }

        [HttpGet("/Reports/AtRisk/{courseId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AtRisk(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            var atRiskStudents = await _reportService.GetAtRiskStudentsAsync(courseId);
            return View(atRiskStudents);
        }
    }
}
