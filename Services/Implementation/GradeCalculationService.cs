using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using MVC_PROJECT.Services.Interfaces;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Implementation
{
    public class GradeCalculationService : IGradeCalculationService
    {
        private readonly AppDbContext _context;

        public GradeCalculationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CoursePolicy?> GetCoursePolicyAsync(int courseId)
        {
            return await _context.CoursePolicies
                .FirstOrDefaultAsync(cp => cp.CourseId == courseId);
        }

        public async Task<decimal> CalculateSectionAttendanceGradeAsync(int enrollmentId, int courseId)
        {
            var policy = await GetCoursePolicyAsync(courseId);
            if (policy == null || policy.SectionAttendanceMarks == 0)
            {
                return 0;
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return 0;
            }

            // Get all section attendance sessions for this course section
            var sectionSessions = await _context.AttendanceSessions
                .Where(s => s.CourseSectionId == enrollment.CourseSectionId && s.SessionType == AttendanceSessionType.Section)
                .ToListAsync();

            if (!sectionSessions.Any())
            {
                return policy.SectionAttendanceMarks; // Full marks if no sessions
            }

            var sessionIds = sectionSessions.Select(s => s.Id).ToList();

            // Get student's attendance records for these sessions
            var records = await _context.AttendanceRecords
                .Where(r => r.EnrollmentId == enrollmentId && sessionIds.Contains(r.AttendanceSessionId))
                .ToListAsync();

            var presentCount = records.Count(r => r.IsPresent);
            var totalSessions = sessionIds.Count;

            if (totalSessions == 0)
            {
                return policy.SectionAttendanceMarks;
            }

            // Calculate attendance percentage
            var attendancePercentage = (decimal)presentCount / totalSessions;

            // Apply to section attendance marks
            return attendancePercentage * policy.SectionAttendanceMarks;
        }

        public async Task<decimal> CalculateLectureAttendanceGradeAsync(int enrollmentId, int courseId)
        {
            var policy = await GetCoursePolicyAsync(courseId);
            if (policy == null || policy.LectureAttendanceMarks == 0)
            {
                return 0;
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return 0;
            }

            // Get all lecture sessions for this course
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            var lectureSessions = await _context.AttendanceSessions
                .Where(s => sectionIds.Contains(s.CourseSectionId) && s.SessionType == AttendanceSessionType.Lecture)
                .ToListAsync();

            if (!lectureSessions.Any())
            {
                return policy.LectureAttendanceMarks; // Full marks if no lectures
            }

            var sessionIds = lectureSessions.Select(s => s.Id).ToList();

            // Get student's attendance records for these sessions
            var records = await _context.AttendanceRecords
                .Where(r => r.EnrollmentId == enrollmentId && sessionIds.Contains(r.AttendanceSessionId))
                .ToListAsync();

            var presentCount = records.Count(r => r.IsPresent);
            var totalSessions = sessionIds.Count;

            if (totalSessions == 0)
            {
                return policy.LectureAttendanceMarks;
            }

            // Calculate attendance percentage
            var attendancePercentage = (decimal)presentCount / totalSessions;

            // Apply to lecture attendance marks
            return attendancePercentage * policy.LectureAttendanceMarks;
        }

        public async Task<decimal> CalculateQuizGradeAsync(int enrollmentId, int courseId)
        {
            var policy = await GetCoursePolicyAsync(courseId);
            if (policy == null || policy.QuizMarks == 0)
            {
                return 0;
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return 0;
            }

            // Get all quizzes for this course section
            var quizzes = await _context.Quizzes
                .Include(q => q.QuizGrades)
                .Where(q => q.CourseSectionId == enrollment.CourseSectionId)
                .ToListAsync();

            if (!quizzes.Any())
            {
                return policy.QuizMarks; // Full marks if no quizzes
            }

            // Get student's quiz grades
            var quizGrades = quizzes
                .Select(q => new
                {
                    QuizId = q.Id,
                    MaxMark = q.MaxMark,
                    Grade = q.QuizGrades.FirstOrDefault(g => g.EnrollmentId == enrollmentId)
                })
                .ToList();

            // Calculate percentage scores for all quizzes
            var percentages = quizGrades
                .Select(q => new
                {
                    q.QuizId,
                    PercentageScore = q.Grade != null && q.MaxMark > 0
                        ? (q.Grade.Mark / q.MaxMark) * 100
                        : 0
                })
                .ToList();

            // If student missed any quiz, they get 0 for that quiz
            foreach (var quiz in quizzes)
            {
                if (!quizGrades.Any(qg => qg.QuizId == quiz.Id))
                {
                    percentages.Add(new { QuizId = quiz.Id, PercentageScore = 0m });
                }
            }

            // Select best N quizzes
            var bestQuizzes = percentages
                .OrderByDescending(p => p.PercentageScore)
                .Take(policy.BestQuizzesCount > 0 ? policy.BestQuizzesCount : percentages.Count)
                .ToList();

            if (!bestQuizzes.Any())
            {
                return 0;
            }

            // Calculate average of best quizzes
            var averagePercentage = bestQuizzes.Average(q => q.PercentageScore);

            // Normalize to quiz marks (should not exceed policy.QuizMarks)
            var normalizedGrade = (averagePercentage / 100) * policy.QuizMarks;

            return Math.Min(normalizedGrade, policy.QuizMarks);
        }

        public async Task<decimal> CalculateTotalCourseworkGradeAsync(int enrollmentId, int courseId)
        {
            var policy = await GetCoursePolicyAsync(courseId);
            if (policy == null)
            {
                return 0;
            }

            var sectionAttendanceGrade = await CalculateSectionAttendanceGradeAsync(enrollmentId, courseId);
            var lectureAttendanceGrade = await CalculateLectureAttendanceGradeAsync(enrollmentId, courseId);
            var quizGrade = await CalculateQuizGradeAsync(enrollmentId, courseId);

            // Total coursework is the sum of all components
            var total = sectionAttendanceGrade + lectureAttendanceGrade + quizGrade;

            // The maximum possible coursework mark is the sum of policy marks
            var maximumPossibleMarks = policy.SectionAttendanceMarks + policy.QuizMarks + policy.LectureAttendanceMarks;

            // If maximum is 0, return 0
            if (maximumPossibleMarks == 0)
            {
                return 0;
            }

            // Ensure total doesn't exceed the maximum possible
            return Math.Min(total, maximumPossibleMarks);
        }

        public async Task<bool> IsAbsentLimitExceededAsync(int enrollmentId, int courseId)
        {
            var policy = await GetCoursePolicyAsync(courseId);
            if (policy == null)
            {
                return false;
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return false;
            }

            // Get all attendance sessions for this course (both section and lecture)
            var sectionIds = await _context.CourseSections
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Id)
                .ToListAsync();

            var allSessions = await _context.AttendanceSessions
                .Where(s => sectionIds.Contains(s.CourseSectionId))
                .ToListAsync();

            if (!allSessions.Any())
            {
                return false;
            }

            var sessionIds = allSessions.Select(s => s.Id).ToList();

            // Get student's attendance records
            var records = await _context.AttendanceRecords
                .Where(r => r.EnrollmentId == enrollmentId && sessionIds.Contains(r.AttendanceSessionId))
                .ToListAsync();

            // Count absences
            var totalSessions = sessionIds.Count;
            var presentCount = records.Count(r => r.IsPresent);
            var absentCount = totalSessions - presentCount;

            // Also count sessions where student has no record (considered absent)
            var sessionsWithRecords = records.Select(r => r.AttendanceSessionId).Distinct().ToList();
            var missedSessions = sessionIds.Where(sid => !sessionsWithRecords.Contains(sid)).Count();

            var totalAbsences = absentCount + missedSessions;

            return totalAbsences > policy.AllowedAbsences;
        }
    }
}
