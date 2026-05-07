using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Attendance;
using MVC_PROJECT.ViewModels.Quiz;
using System.Security.Claims;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "TA")]
    public class TAController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ISectionService _sectionService;
        private readonly AppDbContext _context;

        public TAController(
            IDashboardService dashboardService,
            ISectionService sectionService,
            AppDbContext context)
        {
            _dashboardService = dashboardService;
            _sectionService = sectionService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int taId = GetCurrentTAId();

            if (taId <= 0)
                return Unauthorized();

            var viewModel = await _dashboardService.GetTADashboardAsync(taId);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> TakeAttendance(int sectionId)
        {
            int taId = GetCurrentTAId();

            var section = await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .FirstOrDefaultAsync(cs => cs.Id == sectionId && cs.TAId == taId);

            if (section == null)
                return Forbid();

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseSectionId == sectionId)
                .ToListAsync();

            // ? Weekly logic
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var session = await _context.AttendanceSessions
    .Include(s => s.Records)
    .FirstOrDefaultAsync(s =>
        s.CourseSectionId == sectionId &&
        s.Date >= startOfWeek &&
        s.Date < endOfWeek);

            if (session == null)
            {
                session = new AttendanceSession
                {
                    CourseSectionId = sectionId,
                    Date = DateTime.Now,
                    SessionType = AttendanceSessionType.Section,
                    Method = AttendanceMethod.Code,
                    IsClosed = false,
                    AttendanceCode = Guid.NewGuid().ToString().Substring(0, 6)
                };

                _context.AttendanceSessions.Add(session);
                await _context.SaveChangesAsync();
            }

            var model = new SectionAttendanceViewModel
            {
                CourseSectionId = sectionId,
                CourseName = section.Course.Name,
                CourseCode = section.Course.Code,
                DepartmentName = section.DepartmentSection.Department.Name,
                SectionNumber = section.DepartmentSection.Number,
                Date = startOfWeek,
                IsLocked = session?.IsClosed ?? false,
                AttendanceSessionId = session.Id,

                Students = enrollments.Select(e => new SectionAttendanceStudentViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    FullName = e.Student.FullName,
                    UniversityId = e.Student.UniversityId,
                    IsPresent = session != null &&
                                session.Records.Any(r => r.EnrollmentId == e.Id && r.IsPresent)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TakeAttendance(SaveSectionAttendanceViewModel model)
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var session = await _context.AttendanceSessions
                .Include(s => s.Records)
                .FirstOrDefaultAsync(s =>
                    s.CourseSectionId == model.CourseSectionId &&
                    s.Date >= startOfWeek &&
                    s.Date < endOfWeek);

            // ?? ??? ??????? ?? ?????
            if (session != null && session.IsClosed)
            {
                return BadRequest("Attendance already taken this week.");
            }

            // ?? ???? session ? ???? ????
            if (session == null)
            {
                session = new AttendanceSession
                {
                    CourseSectionId = model.CourseSectionId,
                    Date = startOfWeek,
                    IsClosed = false,
                    SessionType = AttendanceSessionType.Section,
                    Method = AttendanceMethod.Manual
                };

                _context.AttendanceSessions.Add(session);
                await _context.SaveChangesAsync();
            }

            // update / insert records
            foreach (var student in model.Students)
            {
                var record = session.Records
                    .FirstOrDefault(r => r.EnrollmentId == student.EnrollmentId);

                if (record != null)
                {
                    record.IsPresent = student.IsPresent;
                }
                else
                {
                    _context.AttendanceRecords.Add(new AttendanceRecord
                    {
                        AttendanceSessionId = session.Id,
                        EnrollmentId = student.EnrollmentId,
                        IsPresent = student.IsPresent
                    });
                }
            }

            await _context.SaveChangesAsync();

            // ?? ???? ??????
            session.IsClosed = true;
            await _context.SaveChangesAsync();

            // ? Activity Log
            var alreadyLogged = await _context.Notifications
                .AnyAsync(n =>
                    n.UserId == GetCurrentTAId() &&
                    n.CreatedAt.Date == DateTime.Today &&
                    n.Message.Contains(model.CourseSectionId.ToString()));

            if (!alreadyLogged)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Attendance Taken",
                    Type = AttendanceSessionType.Section,
                    Message = $"CS381 - Section {model.CourseSectionId}",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    UserId = GetCurrentTAId()
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
        public async Task<IActionResult> ExportAllSectionsReport()
        {
            var userId = GetCurrentTAId();

            var sections = await _context.CourseSections
    .Where(cs => cs.TAId == userId) // ?? ????? ??? ??????
    .Include(cs => cs.Course)
    .Include(cs => cs.Enrollments)
        .ThenInclude(e => e.Student)
    .Include(cs => cs.Quizzes)
        .ThenInclude(q => q.QuizGrades)
    .Include(cs => cs.AttendanceSessions)
        .ThenInclude(s => s.Records)
    .ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();

            foreach (var section in sections)
            {
                var baseName = $"{section.Course.Name}-S{section.SectionNumber}-{section.Id}";
                baseName = baseName.Replace("/", "-").Replace("\\", "-");

                // Excel max length = 31
                if (baseName.Length > 31)
                {
                    baseName = baseName.Substring(0, 31);
                }

                // ???? ??? ???????
                var sheetName = baseName;
                int counter = 1;

                while (package.Workbook.Worksheets.Any(s => s.Name == sheetName))
                {
                    var suffix = $"_{counter}";
                    sheetName = baseName;

                    if (sheetName.Length + suffix.Length > 31)
                    {
                        sheetName = sheetName.Substring(0, 31 - suffix.Length);
                    }

                    sheetName += suffix;
                    counter++;
                }

                var ws = package.Workbook.Worksheets.Add(sheetName);

                // ????? ???????? ??? ???????
                var quizzes = section.Quizzes.OrderBy(q => q.Date).ToList();

                // Headers
                ws.Cells[1, 1].Value = "Name";
                ws.Cells[1, 2].Value = "University ID";

                int col = 3;

                foreach (var quiz in quizzes)
                {
                    ws.Cells[1, col].Value = quiz.Title;
                    col++;
                }

                ws.Cells[1, col++].Value = "Attendance";
                ws.Cells[1, col++].Value = "Total";

                int row = 2;

                foreach (var e in section.Enrollments)
                {
                    col = 3;
                    decimal totalMarks = 0;

                    ws.Cells[row, 1].Value = e.Student.FullName;
                    ws.Cells[row, 2].Value = e.Student.UniversityId;

                    // ????? ??????
                    foreach (var quiz in quizzes)
                    {
                        var grade = quiz.QuizGrades
                            .FirstOrDefault(g => g.EnrollmentId == e.Id)?.Mark ?? 0;

                        ws.Cells[row, col].Value = grade;
                        totalMarks += grade;
                        col++;
                    }

                    // ?? ???? ??????
                    var closedSessions = section.AttendanceSessions
    .Where(s => s.IsClosed)
    .ToList();

                    var totalSessions = closedSessions.Count;

                    var presentCount = closedSessions
                        .SelectMany(s => s.Records)
                        .Count(r => r.EnrollmentId == e.Id && r.IsPresent);

                    const int attendanceMax = 10;

                    decimal attendanceScore = totalSessions == 0
                        ? 0
                        : (decimal)presentCount / totalSessions * attendanceMax;

                    // attendance
                    ws.Cells[row, col].Value = attendanceScore;
                    totalMarks += attendanceScore;
                    col++;

                    // total
                    ws.Cells[row, col].Value = totalMarks;
                    row++;
                }

                ws.Cells.AutoFitColumns();
            }

            var stream = new MemoryStream(package.GetAsByteArray());

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "All_Sections_Report.xlsx");
        }
        private int GetCurrentTAId()
        {
            return User.GetUserId();
        }
        [HttpGet]
        public IActionResult CreateQuiz(int sectionId)
        {
            var model = new CreateQuizViewModel
            {
                CourseSectionId = sectionId
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuiz(CreateQuizViewModel model)
        {

            var section = await _context.CourseSections
    .FirstOrDefaultAsync(cs => cs.Id == model.CourseSectionId && cs.TAId == GetCurrentTAId());

            if (section == null)
                return Forbid();
            if (!ModelState.IsValid)
                return View(model);

            var quiz = new Quiz
            {
                CourseSectionId = model.CourseSectionId,
                Title = model.QuizTitle,
                Date = model.Date,
                MaxMark = model.MaxMark
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public async Task<IActionResult> RecordQuizMarks(int quizId)
        {
            if (quizId == 0)
                return BadRequest("QuizId is missing");

            var quiz = await _context.Quizzes
                .Include(q => q.CourseSection)
                    .ThenInclude(cs => cs.Enrollments)
                        .ThenInclude(e => e.Student)
                .Include(q => q.QuizGrades)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return NotFound();

            if (quiz.CourseSection == null)
                return NotFound();

            // ? ????? ???? crash
            if (quiz.CourseSection.TAId != GetCurrentTAId())
                return Forbid();

            var model = new RecordQuizMarksViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                MaxMark = quiz.MaxMark,
                Students = quiz.CourseSection.Enrollments.Select(e => new QuizStudentMarkViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.Student.Id,
                    FullName = e.Student.FullName,
                    UniversityId = e.Student.UniversityId,
                    Mark = quiz.QuizGrades
                        .FirstOrDefault(g => g.EnrollmentId == e.Id)?.Mark ?? 0
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordQuizMarks(RecordQuizMarksViewModel model)
        {
            var userId = GetCurrentTAId();

            var quiz = await _context.Quizzes
                .Include(q => q.CourseSection)
                .Include(q => q.QuizGrades)
                .FirstOrDefaultAsync(q => q.Id == model.QuizId);

            if (quiz == null)
                return NotFound();

            if (quiz.CourseSection.TAId != userId)
                return Forbid();

            foreach (var student in model.Students)
            {
                var existing = quiz.QuizGrades
                    .FirstOrDefault(g => g.EnrollmentId == student.EnrollmentId);

                if (existing != null)
                {
                    existing.Mark = student.Mark ?? 0;
                }
                else
                {
                    _context.QuizGrades.Add(new QuizGrade
                    {
                        QuizId = quiz.Id,
                        EnrollmentId = student.EnrollmentId,
                        Mark = student.Mark ?? 0
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("RecordQuizMarks", new { quizId = model.QuizId });
        }
        [HttpPost]
        public async Task<IActionResult> CloseQuiz(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return NotFound();

            quiz.IsClosed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}