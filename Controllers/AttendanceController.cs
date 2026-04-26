using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Models;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Attendance;
using System.Threading.Tasks;
using QRCoder;

namespace MVC_PROJECT.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ISectionService _sectionService;
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;

        public AttendanceController(
            IAttendanceService attendanceService,
            ISectionService sectionService,
            ICourseService courseService,
            IStudentService studentService)
        {
            _attendanceService = attendanceService;
            _sectionService = sectionService;
            _courseService = courseService;
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> Section(int sectionId)
        {
            if (sectionId <= 0)
                return BadRequest("Invalid section ID.");

            int taId = User.GetUserId();

            if (taId <= 0)
                return Unauthorized("Unable to identify TA.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, sectionId);

            if (!isAssigned)
                return Forbid();

            var viewModel = await _attendanceService.GetSectionAttendanceViewModelAsync(sectionId);

            if (viewModel == null || viewModel.CourseSectionId == 0)
                return NotFound("Section attendance data was not found.");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TA")]
        public async Task<IActionResult> SaveSectionAttendance(SaveSectionAttendanceViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int taId = User.GetUserId();

            if (taId <= 0)
                return Unauthorized("Unable to identify TA.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, viewModel.CourseSectionId);

            if (!isAssigned)
                return Forbid();

            var session = await _attendanceService.GetSessionByIdAsync(viewModel.AttendanceSessionId);

            if (session == null)
                return NotFound("Attendance session not found.");

            if (session.IsClosed)
                return BadRequest("Cannot save attendance for a closed session.");

            if (session.Method != AttendanceMethod.Manual)
                return BadRequest("This session does not use manual attendance.");

            var records = new System.Collections.Generic.List<AttendanceRecord>();

            foreach (var student in viewModel.Students)
            {
                records.Add(new AttendanceRecord
                {
                    EnrollmentId = student.EnrollmentId,
                    IsPresent = student.IsPresent
                });
            }

            await _attendanceService.SaveAttendanceRecordsAsync(viewModel.AttendanceSessionId, records);

            return RedirectToAction("SectionDetails", "TA", new { sectionId = viewModel.CourseSectionId });
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Lecture(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");

            int doctorId = User.GetUserId();

            if (doctorId <= 0)
                return Unauthorized("Unable to identify doctor.");

            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
                return NotFound($"Course with ID {courseId} not found.");

            if (course.DoctorId != doctorId)
                return Forbid();

            var viewModel = await _attendanceService.GetLectureAttendanceViewModelAsync(courseId);

            if (viewModel == null || viewModel.CourseId == 0)
                return NotFound("Lecture attendance data was not found.");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> SaveLectureAttendance(SaveLectureAttendanceViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int doctorId = User.GetUserId();

            if (doctorId <= 0)
                return Unauthorized("Unable to identify doctor.");

            var session = await _attendanceService.GetSessionByIdAsync(viewModel.AttendanceSessionId);

            if (session == null)
                return NotFound("Attendance session not found.");

            if (session.IsClosed)
                return BadRequest("Cannot save attendance for a closed session.");

            if (session.Method != AttendanceMethod.Manual)
                return BadRequest("This session does not use manual attendance.");

            var course = await _courseService.GetCourseByIdAsync(session.CourseSection.CourseId);

            if (course == null || course.DoctorId != doctorId)
                return Forbid();

            var records = new System.Collections.Generic.List<AttendanceRecord>();

            foreach (var student in viewModel.Students)
            {
                records.Add(new AttendanceRecord
                {
                    EnrollmentId = student.EnrollmentId,
                    IsPresent = student.IsPresent
                });
            }

            await _attendanceService.SaveAttendanceRecordsAsync(viewModel.AttendanceSessionId, records);

            return RedirectToAction("CourseDetails", "Doctor", new { courseId = session.CourseSection.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,TA")]
        public async Task<IActionResult> GenerateCode(int attendanceSessionId)
        {
            if (attendanceSessionId <= 0)
                return BadRequest("Invalid attendance session ID.");

            var session = await _attendanceService.GetSessionByIdAsync(attendanceSessionId);

            if (session == null)
                return NotFound("Attendance session not found.");

            if (session.IsClosed)
                return BadRequest("Cannot generate code for a closed session.");

            if (session.Method != AttendanceMethod.Code)
                return BadRequest("This session does not use attendance code.");

            var authResult = await CheckSessionOwnerAsync(session);

            if (authResult != null)
                return authResult;

            string newCode = await _attendanceService.GenerateAttendanceCodeAsync(attendanceSessionId);

            return Json(new { success = true, code = newCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,TA")]
        public async Task<IActionResult> GenerateQr(int attendanceSessionId)
        {
            if (attendanceSessionId <= 0)
                return BadRequest("Invalid attendance session ID.");

            var session = await _attendanceService.GetSessionByIdAsync(attendanceSessionId);

            if (session == null)
                return NotFound("Attendance session not found.");

            if (session.IsClosed)
                return BadRequest("Cannot generate QR for a closed session.");

            if (session.Method != AttendanceMethod.QR)
                return BadRequest("This session does not use QR attendance.");

            var authResult = await CheckSessionOwnerAsync(session);

            if (authResult != null)
                return authResult;

            string qrCode = await _attendanceService.GenerateAttendanceCodeAsync(attendanceSessionId);

            string qrPayload = Url.Action(
                "SubmitQr",
                "Attendance",
                new { attendanceSessionId = attendanceSessionId, code = qrCode },
                Request.Scheme
            ) ?? string.Empty;

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q);
            using var qrPng = new PngByteQRCode(qrData);

            byte[] qrBytes = qrPng.GetGraphic(20);
            string qrBase64 = Convert.ToBase64String(qrBytes);
            string qrImage = $"data:image/png;base64,{qrBase64}";

            return Json(new
            {
                success = true,
                code = qrCode,
                qrPayload = qrPayload,
                qrImage = qrImage
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitCode(SubmitAttendanceCodeViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await SubmitAttendanceByCodeAsync(
                viewModel.AttendanceSessionId,
                viewModel.Code,
                AttendanceMethod.Code);

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitQr(int attendanceSessionId, string code)
        {
            if (attendanceSessionId <= 0)
                return BadRequest("Invalid attendance session ID.");

            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Invalid QR code.");

            var result = await SubmitAttendanceByCodeAsync(
                attendanceSessionId,
                code,
                AttendanceMethod.QR);

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,TA")]
        public async Task<IActionResult> CloseSession(int attendanceSessionId)
        {
            if (attendanceSessionId <= 0)
                return BadRequest("Invalid attendance session ID.");

            var session = await _attendanceService.GetSessionByIdAsync(attendanceSessionId);

            if (session == null)
                return NotFound("Attendance session not found.");

            if (session.IsClosed)
                return BadRequest("Session is already closed.");

            var authResult = await CheckSessionOwnerAsync(session);

            if (authResult != null)
                return authResult;

            await _attendanceService.CloseSessionAsync(attendanceSessionId);

            return Json(new { success = true, message = "Attendance session closed successfully." });
        }

        private async Task<IActionResult?> CheckSessionOwnerAsync(AttendanceSession session)
        {
            int userId = User.GetUserId();

            if (userId <= 0)
                return Unauthorized("Unable to identify user.");

            if (session.SessionType == AttendanceSessionType.Section)
            {
                bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(userId, session.CourseSectionId);

                if (!isAssigned)
                    return Forbid();
            }
            else if (session.SessionType == AttendanceSessionType.Lecture)
            {
                var course = await _courseService.GetCourseByIdAsync(session.CourseSection.CourseId);

                if (course == null || course.DoctorId != userId)
                    return Forbid();
            }

            return null;
        }

        private async Task<IActionResult> SubmitAttendanceByCodeAsync(
            int attendanceSessionId,
            string code,
            AttendanceMethod expectedMethod)
        {
            int userId = User.GetUserId();

            if (userId <= 0)
                return Unauthorized("Unable to identify student.");

            var student = await _studentService.GetStudentByUserIdAsync(userId);

            if (student == null)
                return NotFound("Student not found.");

            var session = await _attendanceService.GetSessionByIdAsync(attendanceSessionId);

            if (session == null)
                return NotFound("Attendance session not found.");

            if (session.IsClosed)
                return BadRequest("Attendance session is closed.");

            if (session.Method != expectedMethod)
                return BadRequest("This attendance method is not valid for this session.");

            var enrollment = await _sectionService.GetEnrollmentByStudentAndSectionAsync(
                student.Id,
                session.CourseSectionId);

            if (enrollment == null)
                return BadRequest("You are not enrolled in this course section.");

            bool success = await _attendanceService.SubmitAttendanceCodeAsync(
                attendanceSessionId,
                code,
                enrollment.Id);

            if (!success)
                return BadRequest("Invalid code or you have already submitted attendance for this session.");

            return Json(new { success = true, message = "Attendance submitted successfully." });
        }
    }
}