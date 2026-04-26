using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Implementation
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;

        public QuizService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Include(q => q.QuizGrades)
                    .ThenInclude(qg => qg.Enrollment)
                        .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }
        public async Task<StudentQuizMarksViewModel> GetStudentQuizGradesAsync(int courseId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.CoursePolicy)
                .Include(e => e.CourseSection)
                    .ThenInclude(cs => cs.DepartmentSection)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseSection.CourseId == courseId);

            if (enrollment == null)
                return new StudentQuizMarksViewModel();

            var grades = await _context.QuizGrades
                .Include(g => g.Quiz)
                .Where(g => g.EnrollmentId == enrollment.Id)
                .OrderBy(g => g.Quiz.Date)
                .ToListAsync();

            return new StudentQuizMarksViewModel
            {
                CourseId = enrollment.CourseSection.Course.Id,
                CourseCode = enrollment.CourseSection.Course.Code,
                CourseName = enrollment.CourseSection.Course.Name,

                SectionNumber = enrollment.CourseSection.DepartmentSection.Number,

                QuizMarks = enrollment.CourseSection.Course.CoursePolicy?.QuizMarks ?? 0,
                BestQuizzesCount = enrollment.CourseSection.Course.CoursePolicy?.BestQuizzesCount ?? 0,

                AverageQuizMark = grades.Count == 0
                    ? 0
                    : grades.Average(g => g.Mark),

                Quizzes = grades.Select(g => new StudentQuizMarkItemViewModel
                {
                    QuizId = g.QuizId,
                    QuizTitle = g.Quiz.Title,
                    Date = g.Quiz.Date,
                    MaxMark = g.Quiz.MaxMark,
                    Mark = g.Mark,
                    PercentageScore = g.Quiz.MaxMark == 0
                        ? 0
                        : (g.Mark / g.Quiz.MaxMark) * 100
                }).ToList()
            };
        }

        public async Task<List<Quiz>> GetQuizzesBySectionAsync(int courseSectionId)
        {
            return await _context.Quizzes
                .Include(q => q.QuizGrades)
                .Where(q => q.CourseSectionId == courseSectionId)
                .OrderBy(q => q.Date)
                .ToListAsync();
        }

        public async Task<List<QuizGrade>> GetGradesByQuizAsync(int quizId)
        {
            return await _context.QuizGrades
                .Include(qg => qg.Quiz)
                .Include(qg => qg.Enrollment)
                    .ThenInclude(e => e.Student)
                .Where(qg => qg.QuizId == quizId)
                .ToListAsync();
        }

        public async Task<List<QuizGrade>> GetGradesByStudentCourseAsync(int studentId, int courseId)
        {
            var enrollmentIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Join(_context.CourseSections.Where(cs => cs.CourseId == courseId),
                    e => e.CourseSectionId,
                    cs => cs.Id,
                    (e, cs) => e.Id)
                .ToListAsync();

            var quizIds = await _context.Quizzes
                .Where(q => enrollmentIds.Contains(q.CourseSectionId))
                .Select(q => q.Id)
                .ToListAsync();

            return await _context.QuizGrades
                .Include(qg => qg.Quiz)
                .Include(qg => qg.Enrollment)
                .Where(qg => enrollmentIds.Contains(qg.EnrollmentId) && quizIds.Contains(qg.QuizId))
                .ToListAsync();
        }

        public async Task<Quiz> CreateQuizAsync(int courseSectionId, string title, decimal maxMark)
        {
            var courseSection = await _context.CourseSections.FindAsync(courseSectionId);
            if (courseSection == null)
            {
                throw new ArgumentException($"CourseSection with ID {courseSectionId} not found.");
            }

            var quiz = new Quiz
            {
                CourseSectionId = courseSectionId,
                Title = title,
                Date = DateTime.UtcNow,
                MaxMark = maxMark
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return quiz;
        }

        public async Task SaveQuizGradesAsync(int quizId, List<QuizGrade> grades)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizGrades)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new ArgumentException($"Quiz with ID {quizId} not found.");
            }

            var enrollmentIds = grades.Select(g => g.EnrollmentId).Distinct().ToList();
            var enrollments = await _context.Enrollments
                .Where(e => enrollmentIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id);

            foreach (var grade in grades)
            {
                if (!enrollments.ContainsKey(grade.EnrollmentId))
                {
                    throw new ArgumentException($"Enrollment with ID {grade.EnrollmentId} not found.");
                }

                if (grade.Mark < 0 || grade.Mark > quiz.MaxMark)
                {
                    throw new ArgumentException($"Mark {grade.Mark} is out of range for Quiz {quizId}. Max mark is {quiz.MaxMark}.");
                }

                grade.PercentageScore = quiz.MaxMark > 0 ? (grade.Mark / quiz.MaxMark) * 100 : 0;
            }

            _context.QuizGrades.RemoveRange(quiz.QuizGrades);

            foreach (var grade in grades)
            {
                grade.QuizId = quizId;
                _context.QuizGrades.Add(grade);
            }

            await _context.SaveChangesAsync();
        }
    }
}
