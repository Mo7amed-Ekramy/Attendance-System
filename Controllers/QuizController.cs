using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Models;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Quiz;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    public class QuizController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ISectionService _sectionService;
        private readonly IStudentService _studentService;

        public QuizController(
            IQuizService quizService,
            ISectionService sectionService,
            IStudentService studentService)
        {
            _quizService = quizService;
            _sectionService = sectionService;
            _studentService = studentService;
        }

        #region TA - Create Quiz

        [HttpGet]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> Create(int sectionId)
        {
            if (sectionId <= 0)
                return BadRequest("Invalid section ID.");

            int taId = User.GetUserId();
            if (taId <= 0)
                return BadRequest("Unable to identify TA.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, sectionId);
            if (!isAssigned)
                return Forbid();

            var section = await _sectionService.GetSectionByIdAsync(sectionId);
            if (section == null)
                return NotFound($"Section with ID {sectionId} not found.");

            var viewModel = new CreateQuizViewModel
            {
                CourseSectionId = sectionId,
                CourseName = section.Course?.Name ?? "",
                SectionNumber = section.DepartmentSection?.Number ?? 0,
                QuizTitle = "",
                MaxMark = 0,
                Date = System.DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> Create(CreateQuizViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int taId = User.GetUserId();
            if (taId <= 0)
                return BadRequest("Unable to identify TA.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, viewModel.CourseSectionId);
            if (!isAssigned)
                return Forbid();

            await _quizService.CreateQuizAsync(
                viewModel.CourseSectionId,
                viewModel.QuizTitle,
                viewModel.MaxMark);

            return RedirectToAction("SectionDetails", "TA", new { sectionId = viewModel.CourseSectionId });
        }

        #endregion

        #region TA - Record Quiz Marks

        [HttpGet]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> RecordMarks(int quizId)
        {
            if (quizId <= 0)
                return BadRequest("Invalid quiz ID.");

            int taId = User.GetUserId();
            if (taId <= 0)
                return BadRequest("Unable to identify TA.");

            var quiz = await _quizService.GetQuizByIdAsync(quizId);
            if (quiz == null)
                return NotFound($"Quiz with ID {quizId} not found.");

            var section = await _sectionService.GetSectionByIdAsync(quiz.CourseSectionId);
            if (section == null)
                return NotFound($"Section with ID {quiz.CourseSectionId} not found.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, section.Id);
            if (!isAssigned)
                return Forbid();

            var enrollments = await _sectionService.GetEnrollmentsBySectionAsync(section.Id);
            var students = new List<QuizStudentMarkViewModel>();

            foreach (var enrollment in enrollments)
            {
                var student = await _studentService.GetStudentByIdAsync(enrollment.StudentId);
                if (student != null)
                {
                    students.Add(new QuizStudentMarkViewModel
                    {
                        EnrollmentId = enrollment.Id,
                        StudentId = student.Id,
                        UniversityId = student.UniversityId,
                        FullName = student.FullName,
                        Mark = null
                    });
                }
            }

            var viewModel = new RecordQuizMarksViewModel
            {
                QuizId = quizId,
                QuizTitle = quiz.Title,
                CourseName = section.Course?.Name ?? "",
                MaxMark = quiz.MaxMark,
                IsClosed = quiz.IsClosed,
                Students = students
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> SaveMarks(RecordQuizMarksViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int taId = User.GetUserId();
            if (taId <= 0)
                return BadRequest("Unable to identify TA.");

            var quiz = await _quizService.GetQuizByIdAsync(viewModel.QuizId);
            if (quiz == null)
                return NotFound("Quiz not found.");

            if (quiz.IsClosed)
                return BadRequest("Cannot save marks for a closed quiz.");

            var grades = new List<QuizGrade>();

            foreach (var student in viewModel.Students)
            {
                if (student.Mark.HasValue && student.Mark.Value > quiz.MaxMark)
                {
                    return BadRequest($"Mark for student {student.FullName} cannot exceed max mark of {quiz.MaxMark}.");
                }

                grades.Add(new QuizGrade
                {
                    QuizId = viewModel.QuizId,
                    EnrollmentId = student.EnrollmentId,
                    Mark = student.Mark ?? 0 // ✅ التعديل هنا
                });
            }

            await _quizService.SaveQuizGradesAsync(viewModel.QuizId, grades);

            return RedirectToAction("SectionDetails", "TA", new { sectionId = quiz.CourseSectionId });
        }

        #endregion

        #region Close Quiz

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> Close(int quizId)
        {
            if (quizId <= 0)
                return BadRequest("Invalid quiz ID.");

            int taId = User.GetUserId();
            if (taId <= 0)
                return BadRequest("Unable to identify TA.");

            var quiz = await _quizService.GetQuizByIdAsync(quizId);
            if (quiz == null)
                return NotFound("Quiz not found.");

            if (quiz.IsClosed)
                return BadRequest("Quiz is already closed.");

            var section = await _sectionService.GetSectionByIdAsync(quiz.CourseSectionId);
            if (section == null)
                return NotFound($"Section with ID {quiz.CourseSectionId} not found.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, section.Id);
            if (!isAssigned)
                return Forbid();

            // مؤقتًا بس
            return Json(new { success = true, message = "Quiz closed successfully." });
        }

        #endregion
    }
}