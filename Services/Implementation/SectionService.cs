using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.TA;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Implementation
{
    public class SectionService : ISectionService
    {
        private readonly AppDbContext _context;

        public SectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CourseSection?> GetSectionByIdAsync(int sectionId)
        {
            return await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.TA)
                .FirstOrDefaultAsync(cs => cs.Id == sectionId);
        }

        public async Task<List<CourseSection>> GetSectionsByTAAsync(int taId)
        {
            return await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Where(cs => cs.TAId == taId)
                .ToListAsync();
        }

        public async Task<List<CourseSection>> GetSectionsByCourseAsync(int courseId)
        {
            return await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.TA)
                .Where(cs => cs.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentByStudentAndSectionAsync(int studentId, int courseSectionId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.CourseSection)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseSectionId == courseSectionId);
        }

        public async Task<List<Enrollment>> GetEnrollmentsBySectionAsync(int sectionId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.CourseSection)
                .Where(e => e.CourseSectionId == sectionId)
                .ToListAsync();
        }

        public async Task<bool> IsTAAssignedToSectionAsync(int taId, int sectionId)
        {
            var section = await _context.CourseSections.FindAsync(sectionId);
            return section != null && section.TAId == taId;
        }

        public async Task<List<TASectionItemViewModel>> GetTASectionsAsync(int taId)
        {
            return await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.TAId == taId)
                .Select(cs => new TASectionItemViewModel
                {
                    CourseSectionId = cs.Id,
                    CourseCode = cs.Course.Code,
                    CourseName = cs.Course.Name,
                    DepartmentName = cs.DepartmentSection.Department.Name,
                    SectionNumber = cs.DepartmentSection.Number,
                    StudentCount = cs.Enrollments.Count
                })
                .ToListAsync();
        }

        public async Task<TASectionDetailsViewModel> GetSectionDetailsAsync(int sectionId)
        {
            var section = await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(cs => cs.Id == sectionId);

            if (section == null)
                return new TASectionDetailsViewModel();

            return new TASectionDetailsViewModel
            {
                CourseSectionId = section.Id,
                CourseCode = section.Course.Code,
                CourseName = section.Course.Name,
                DepartmentName = section.DepartmentSection.Department.Name,
                SectionNumber = section.DepartmentSection.Number,
                StudentCount = section.Enrollments.Count
            };
        }
    }
}