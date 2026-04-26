using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Doctor;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private int GetCurrentDoctorId()
        {
            return User.GetUserId();
        }
    }
}