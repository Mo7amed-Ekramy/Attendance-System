using MVC_PROJECT.Models;
using MVC_PROJECT.ViewModels.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IQuizService
    {
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        Task<List<Quiz>> GetQuizzesBySectionAsync(int courseSectionId);
        Task<List<QuizGrade>> GetGradesByQuizAsync(int quizId);
        Task<List<QuizGrade>> GetGradesByStudentCourseAsync(int studentId, int courseId);
        Task<Quiz> CreateQuizAsync(int courseSectionId, string title, decimal maxMark);
        Task SaveQuizGradesAsync(int quizId, List<QuizGrade> grades);

        // Student-specific methods
        Task<StudentQuizMarksViewModel> GetStudentQuizGradesAsync(int courseId, int studentId);
    }
}
