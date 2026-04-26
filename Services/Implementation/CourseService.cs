using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Course;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC_PROJECT.ViewModels.Student;
using MVC_PROJECT.ViewModels.Doctor;

namespace MVC_PROJECT.Services.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;

        public CourseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentCourseItemViewModel>> GetStudentCoursesAsync(int studentId)
        {
            return await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.Doctor)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.CoursePolicy)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.DepartmentSection)
                .Where(e => e.StudentId == studentId)
                .Select(e => new StudentCourseItemViewModel
                {
                    CourseId = e.CourseSection.Course.Id,
                    CourseCode = e.CourseSection.Course.Code,
                    CourseName = e.CourseSection.Course.Name,
                    DoctorName = e.CourseSection.Course.Doctor.FullName,
                    SectionNumber = e.CourseSection.DepartmentSection.Number,
                    AllowedAbsences = e.CourseSection.Course.CoursePolicy != null
                        ? e.CourseSection.Course.CoursePolicy.AllowedAbsences
                        : 0,
                    AbsenceCount = 0,
                    AbsenceStatus = "Safe",
                    CourseworkMark = 0
                })
                .ToListAsync();
        }

        public async Task<StudentCourseDetailsViewModel> GetCourseDetailsAsync(int courseId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.Doctor)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.CoursePolicy)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.DepartmentSection)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseSection.CourseId == courseId);

            if (enrollment == null)
                return new StudentCourseDetailsViewModel();

            var policy = enrollment.CourseSection.Course.CoursePolicy;

            return new StudentCourseDetailsViewModel
            {
                CourseId = enrollment.CourseSection.Course.Id,
                CourseCode = enrollment.CourseSection.Course.Code,
                CourseName = enrollment.CourseSection.Course.Name,
                DoctorName = enrollment.CourseSection.Course.Doctor.FullName,
                SectionNumber = enrollment.CourseSection.DepartmentSection.Number,
                AllowedAbsences = policy?.AllowedAbsences ?? 0,
                AbsenceCount = 0,
                AbsenceStatus = "Safe",
                TotalCourseworkMarks =
                    (policy?.SectionAttendanceMarks ?? 0) +
                    (policy?.QuizMarks ?? 0) +
                    (policy?.LectureAttendanceMarks ?? 0),
                SectionAttendanceMarks = policy?.SectionAttendanceMarks ?? 0,
                QuizMarks = policy?.QuizMarks ?? 0,
                LectureAttendanceMarks = policy?.LectureAttendanceMarks ?? 0,
                BestQuizzesCount = policy?.BestQuizzesCount ?? 0
            };
        }
        public async Task<DoctorCourseDetailsViewModel> GetCourseDetailsForDoctorAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Doctor)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.DepartmentSection)
                        .ThenInclude(ds => ds.Department)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.TA)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Enrollments)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.AttendanceSessions)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return new DoctorCourseDetailsViewModel();

            var sections = course.CourseSections.Select(cs => new DoctorSectionItemViewModel
            {
                CourseSectionId = cs.Id,

                DepartmentName = cs.DepartmentSection.Department.Name,

                SectionNumber = cs.DepartmentSection.Number,

                TAName = cs.TA.FullName,

                StudentCount = cs.Enrollments.Count,

                TotalAttendanceSessions = cs.AttendanceSessions.Count,

                TotalQuizzes = cs.Quizzes.Count

            }).ToList();

            return new DoctorCourseDetailsViewModel
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                Semester = course.Semester,

                TotalSections = sections.Count,

                TotalStudents = course.CourseSections
                    .SelectMany(cs => cs.Enrollments)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .Count(),

                Sections = sections
            };
        }

        public async Task<int> GetSectionCountByCourseAsync(int courseId)
        {
            return await _context.CourseSections
                .CountAsync(cs => cs.CourseId == courseId);
        }

        public async Task<int> GetStudentCountByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.CourseSection)
                .CountAsync(e => e.CourseSection.CourseId == courseId);
        }

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Doctor)
                .Include(c => c.CoursePolicy)
                .Include(c => c.CourseDepartments)
                    .ThenInclude(cd => cd.Department)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.DepartmentSection)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.TA)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<List<Course>> GetCoursesByDoctorAsync(int doctorId)
        {
            return await _context.Courses
                .Include(c => c.Doctor)
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByDepartmentAsync(int departmentId)
        {
            return await _context.Courses
                .Include(c => c.Doctor)
                .Include(c => c.CourseDepartments)
                .Where(c => c.CourseDepartments.Any(cd => cd.DepartmentId == departmentId))
                .ToListAsync();
        }

        public async Task<List<CourseDepartment>> GetCourseDepartmentsAsync(int courseId)
        {
            return await _context.CourseDepartments
                .Include(cd => cd.Department)
                .Where(cd => cd.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<List<CourseSection>> GetCourseSectionsAsync(int courseId)
        {
            return await _context.CourseSections
                .Include(cs => cs.DepartmentSection)
                .Include(cs => cs.TA)
                .Where(cs => cs.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<CourseConfigurationViewModel> GetCourseConfigurationAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CoursePolicy)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return new CourseConfigurationViewModel();
            }

            var policy = course.CoursePolicy;

            if (policy == null)
            {
                return new CourseConfigurationViewModel
                {
                    CourseId = course.Id,
                    CourseName = course.Name
                };
            }

            return new CourseConfigurationViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                SectionAttendanceMarks = policy.SectionAttendanceMarks,
                QuizMarks = policy.QuizMarks,
                LectureAttendanceMarks = policy.LectureAttendanceMarks,
                AllowedAbsences = policy.AllowedAbsences,
                BestQuizzesCount = policy.BestQuizzesCount
            };
        }

        public async Task UpdateCourseConfigurationAsync(SaveCourseConfigurationViewModel viewModel)
        {
            var course = await _context.Courses
                .Include(c => c.CoursePolicy)
                .FirstOrDefaultAsync(c => c.Id == viewModel.CourseId);

            if (course == null)
            {
                throw new ArgumentException($"Course with ID {viewModel.CourseId} not found.");
            }

            ValidateCourseConfiguration(viewModel);

            var policy = course.CoursePolicy;

            if (policy == null)
            {
                policy = new CoursePolicy
                {
                    CourseId = viewModel.CourseId,
                    SectionAttendanceMarks = viewModel.SectionAttendanceMarks,
                    QuizMarks = viewModel.QuizMarks,
                    LectureAttendanceMarks = viewModel.LectureAttendanceMarks,
                    AllowedAbsences = viewModel.AllowedAbsences,
                    BestQuizzesCount = viewModel.BestQuizzesCount
                };

                _context.CoursePolicies.Add(policy);
            }
            else
            {
                policy.SectionAttendanceMarks = viewModel.SectionAttendanceMarks;
                policy.QuizMarks = viewModel.QuizMarks;
                policy.LectureAttendanceMarks = viewModel.LectureAttendanceMarks;
                policy.AllowedAbsences = viewModel.AllowedAbsences;
                policy.BestQuizzesCount = viewModel.BestQuizzesCount;
            }

            await _context.SaveChangesAsync();
        }

        private void ValidateCourseConfiguration(SaveCourseConfigurationViewModel viewModel)
        {
            var totalDistributedMarks =
                viewModel.SectionAttendanceMarks +
                viewModel.QuizMarks +
                viewModel.LectureAttendanceMarks;

            if (totalDistributedMarks > 100)
            {
                throw new ArgumentException(
                    "The sum of section attendance marks, quiz marks, and lecture attendance marks cannot exceed 100.");
            }

            if (viewModel.BestQuizzesCount < 0)
            {
                throw new ArgumentException("Best quizzes count cannot be negative.");
            }
        }
    }
}