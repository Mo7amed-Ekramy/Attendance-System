using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;

namespace MVC_PROJECT.Services.Implementation
{
    public class StudentValidationService
    {
        private readonly AppDbContext _context;

        public StudentValidationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates that a Student's DepartmentSection belongs to the specified Department.
        /// Returns true if valid, false otherwise with error message.
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateStudentDepartmentSectionAsync(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            // Load the DepartmentSection with its Department
            var departmentSection = await _context.DepartmentSections
                .Include(ds => ds.Department)
                .FirstOrDefaultAsync(ds => ds.Id == student.DepartmentSectionId);

            // Use the validation method from the Student model
            var errorMessage = student.GetDepartmentSectionValidationError(departmentSection);
            
            return (string.IsNullOrEmpty(errorMessage), errorMessage);
        }

        /// <summary>
        /// Creates a new Student with validation that DepartmentSection belongs to Department.
        /// </summary>
        public async Task<Student> CreateStudentWithValidationAsync(
            string universityId,
            string fullName,
            int level,
            int departmentId,
            int departmentSectionId,
            int userId)
        {
            // First, validate the DepartmentSection belongs to the Department
            var departmentSection = await _context.DepartmentSections
                .Include(ds => ds.Department)
                .FirstOrDefaultAsync(ds => ds.Id == departmentSectionId);

            if (departmentSection == null)
            {
                throw new ArgumentException($"DepartmentSection with ID {departmentSectionId} does not exist.");
            }

            if (departmentSection.DepartmentId != departmentId)
            {
                throw new ArgumentException(
                    $"DepartmentSection with ID {departmentSectionId} belongs to Department ID {departmentSection.DepartmentId}, " +
                    $"but Student is being assigned to Department ID {departmentId}.");
            }

            // Create the student
            var student = new Student
            {
                UniversityId = universityId,
                FullName = fullName,
                Level = level,
                DepartmentId = departmentId,
                DepartmentSectionId = departmentSectionId,
                UserId = userId
            };

            // Validate using the model's method
            if (!student.ValidateDepartmentSectionConsistency(departmentSection))
            {
                throw new InvalidOperationException(student.GetDepartmentSectionValidationError(departmentSection));
            }

            return student;
        }

        /// <summary>
        /// Updates an existing Student with validation that DepartmentSection belongs to Department.
        /// </summary>
        public async Task UpdateStudentWithValidationAsync(
            Student student,
            string universityId,
            string fullName,
            int level,
            int departmentId,
            int departmentSectionId)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            // If DepartmentSection is being changed, validate it
            if (student.DepartmentSectionId != departmentSectionId || student.DepartmentId != departmentId)
            {
                var departmentSection = await _context.DepartmentSections
                    .Include(ds => ds.Department)
                    .FirstOrDefaultAsync(ds => ds.Id == departmentSectionId);

                if (departmentSection == null)
                {
                    throw new ArgumentException($"DepartmentSection with ID {departmentSectionId} does not exist.");
                }

                if (departmentSection.DepartmentId != departmentId)
                {
                    throw new ArgumentException(
                        $"DepartmentSection with ID {departmentSectionId} belongs to Department ID {departmentSection.DepartmentId}, " +
                        $"but Student is being assigned to Department ID {departmentId}.");
                }

                // Validate using the model's method
                if (!student.ValidateDepartmentSectionConsistency(departmentSection))
                {
                    throw new InvalidOperationException(student.GetDepartmentSectionValidationError(departmentSection));
                }
            }

            // Update student properties
            student.UniversityId = universityId;
            student.FullName = fullName;
            student.Level = level;
            student.DepartmentId = departmentId;
            student.DepartmentSectionId = departmentSectionId;
        }
    }
}