using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Course;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Configure(int courseId)
        {
            if (courseId <= 0)
            {
                return BadRequest("Invalid course ID.");
            }

            int doctorId = User.GetUserId();
            if (doctorId <= 0)
            {
                return BadRequest("Unable to identify doctor.");
            }

            // Verify doctor is assigned to this course
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound($"Course with ID {courseId} not found.");
            }

            if (course.DoctorId != doctorId)
            {
                return Forbid("You are not assigned to this course.");
            }

            var viewModel = await _courseService.GetCourseConfigurationAsync(courseId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveConfiguration(SaveCourseConfigurationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int doctorId = User.GetUserId();
            if (doctorId <= 0)
            {
                return BadRequest("Unable to identify doctor.");
            }

            // Verify doctor is assigned to this course
            var course = await _courseService.GetCourseByIdAsync(viewModel.CourseId);
            if (course == null)
            {
                return NotFound($"Course with ID {viewModel.CourseId} not found.");
            }

            if (course.DoctorId != doctorId)
            {
                return Forbid("You are not assigned to this course.");
            }

            await _courseService.UpdateCourseConfigurationAsync(viewModel);

            return RedirectToAction("CourseDetails", "Doctor", new { courseId = viewModel.CourseId });
        }
    }
}
