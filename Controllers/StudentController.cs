using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IAttendanceService _attendanceService;
        private readonly IQuizService _quizService;
        private readonly INotificationService _notificationService;

        public StudentController(
            IStudentService studentService,
            ICourseService courseService,
            IAttendanceService attendanceService,
            IQuizService quizService,
            INotificationService notificationService)
        {
            _studentService = studentService;
            _courseService = courseService;
            _attendanceService = attendanceService;
            _quizService = quizService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int studentId = await GetCurrentStudentIdAsync();

            if (studentId <= 0)
                return Unauthorized("Unable to identify student.");

            var viewModel = await _studentService.GetStudentDashboardAsync(studentId);

            if (viewModel == null || viewModel.StudentId == 0)
                return NotFound("Student dashboard data was not found.");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            int studentId = await GetCurrentStudentIdAsync();

            if (studentId <= 0)
                return Unauthorized("Unable to identify student.");

            var courses = await _courseService.GetStudentCoursesAsync(studentId);

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            int studentId = await GetCurrentStudentIdAsync();

            if (studentId <= 0)
                return Unauthorized("Unable to identify student.");

            var viewModel = await _courseService.GetCourseDetailsAsync(courseId, studentId);

            if (viewModel == null || viewModel.CourseId == 0)
                return NotFound("Course not found or student is not enrolled.");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Attendance(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            int studentId = await GetCurrentStudentIdAsync();

            if (studentId <= 0)
                return Unauthorized("Unable to identify student.");

            var viewModel = await _attendanceService.GetStudentAttendanceAsync(courseId, studentId);

            if (viewModel == null || viewModel.CourseId == 0)
                return NotFound("Attendance data was not found for this course.");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> QuizMarks(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            int studentId = await GetCurrentStudentIdAsync();

            if (studentId <= 0)
                return Unauthorized("Unable to identify student.");

            var viewModel = await _quizService.GetStudentQuizGradesAsync(courseId, studentId);

            if (viewModel == null || viewModel.CourseId == 0)
                return NotFound("Quiz marks were not found for this course.");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            int userId = GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized("Unable to identify user.");

            var notificationsList = await _notificationService.GetNotificationsByUserAsync(userId);

            if (notificationsList == null)
                return View();

            return View(notificationsList.Notifications);
        }

        private int GetCurrentUserId()
        {
            return User.GetUserId();
        }

        private async Task<int> GetCurrentStudentIdAsync()
        {
            int userId = GetCurrentUserId();

            if (userId <= 0)
                return 0;

            var student = await _studentService.GetStudentByUserIdAsync(userId);

            return student?.Id ?? 0;
        }
    }
}