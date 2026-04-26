using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Admin;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = await _adminService.GetAdminDashboardAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Students()
        {
            var students = await _adminService.GetStudentsAsync();
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _adminService.GetDoctorsAsync();
            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> TAs()
        {
            var tas = await _adminService.GetTAsAsync();
            return View(tas);
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var courses = await _adminService.GetCoursesAsync();
            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> Sections()
        {
            var sections = await _adminService.GetSectionsAsync();
            return View(sections);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var user = await _adminService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            return View(user);
        }

        #region Users CRUD

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.CreateUserAsync(dto);
                return RedirectToAction(nameof(Users));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var dto = new UpdateUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.UpdateUserAsync(dto);
                return RedirectToAction(nameof(Users));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _adminService.DeleteUserAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Users));
        }

        #endregion

        #region Students CRUD

        [HttpGet]
        public IActionResult CreateStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.CreateStudentAsync(dto);
                return RedirectToAction(nameof(Students));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _adminService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            var dto = new UpdateStudentDto
            {
                Id = student.Id,
                UserName = student.UserName,
                UniversityId = student.UniversityId,
                FullName = student.FullName,
                Level = student.Level,
                DepartmentId = 0, // TODO: Map from DepartmentName
                DepartmentSectionId = student.SectionNumber ?? 0 // TODO: Map correctly
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.UpdateStudentAsync(dto);
                return RedirectToAction(nameof(Students));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                await _adminService.DeleteStudentAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Students));
        }

        #endregion

        #region Doctors CRUD

        [HttpGet]
        public IActionResult CreateDoctor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.CreateDoctorAsync(dto);
                return RedirectToAction(nameof(Doctors));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            var doctor = await _adminService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID {id} not found.");
            }

            var dto = new UpdateUserDto
            {
                Id = doctor.Id,
                UserName = doctor.UserName,
                FullName = doctor.FullName,
                Email = doctor.Email,
                Role = "Doctor"
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.UpdateDoctorAsync(dto);
                return RedirectToAction(nameof(Doctors));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            try
            {
                await _adminService.DeleteDoctorAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Doctors));
        }

        #endregion

        #region TAs CRUD

        [HttpGet]
        public IActionResult CreateTA()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTA(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.CreateTAAsync(dto);
                return RedirectToAction(nameof(TAs));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTA(int id)
        {
            var ta = await _adminService.GetTAByIdAsync(id);
            if (ta == null)
            {
                return NotFound($"TA with ID {id} not found.");
            }

            var dto = new UpdateUserDto
            {
                Id = ta.Id,
                UserName = ta.UserName,
                FullName = ta.FullName,
                Email = ta.Email,
                Role = "TA"
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTA(UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.UpdateTAAsync(dto);
                return RedirectToAction(nameof(TAs));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTA(int id)
        {
            try
            {
                await _adminService.DeleteTAAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(TAs));
        }

        #endregion

        #region Courses CRUD

        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.CreateCourseAsync(dto);
                return RedirectToAction(nameof(Courses));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _adminService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound($"Course with ID {id} not found.");
            }

            var dto = new UpdateCourseDto
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Semester = course.Semester,
                Level = course.Level,
                DoctorId = 0 // TODO: Map from DoctorName
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.UpdateCourseAsync(dto);
                return RedirectToAction(nameof(Courses));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                await _adminService.DeleteCourseAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Courses));
        }

        #endregion

        #region Sections CRUD

        [HttpGet]
        public IActionResult CreateSection()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSection(CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.CreateSectionAsync(dto);
                return RedirectToAction(nameof(Sections));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditSection(int id)
        {
            var section = await _adminService.GetSectionByIdAsync(id);
            if (section == null)
            {
                return NotFound($"Section with ID {id} not found.");
            }

            var dto = new UpdateSectionDto
            {
                Id = section.Id,
                CourseId = 0, // TODO: Map from CourseCode
                DepartmentSectionId = 0, // TODO: Map from DepartmentName + SectionNumber
                TAId = 0 // TODO: Map from TAName
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSection(UpdateSectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _adminService.UpdateSectionAsync(dto);
                return RedirectToAction(nameof(Sections));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                await _adminService.DeleteSectionAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Sections));
        }

        #endregion
    }
}
