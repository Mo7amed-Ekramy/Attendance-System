using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;
using MVC_PROJECT.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC_PROJECT.Services.Interfaces;

namespace MVC_PROJECT.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Users.CountAsync(u => u.Role == Role.Student);
            var totalDoctors = await _context.Users.CountAsync(u => u.Role == Role.Doctor);
            var totalTAs = await _context.Users.CountAsync(u => u.Role == Role.TA);
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == Role.Admin);

            var totalCourses = await _context.Courses.CountAsync();
            var totalDepartments = await _context.Departments.CountAsync();
            var totalSections = await _context.CourseSections.CountAsync();
            var totalEnrollments = await _context.Enrollments.CountAsync();

            // Get recent users (last 10)
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.Id)
                .Take(10)
                .Select(u => new AdminUserItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .ToListAsync();

            // Get recent courses (last 10)
            var recentCourses = await _context.Courses
                .Include(c => c.Doctor)
                .OrderByDescending(c => c.Id)
                .Take(10)
                .Select(c => new AdminCourseItemViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Semester = c.Semester,
                    DoctorName = c.Doctor != null ? c.Doctor.FullName : string.Empty,
                    Level = c.Level,
                    SectionCount = c.CourseSections != null ? c.CourseSections.Count : 0
                })
                .ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalDoctors = totalDoctors,
                TotalTAs = totalTAs,
                TotalAdmins = totalAdmins,
                TotalCourses = totalCourses,
                TotalDepartments = totalDepartments,
                TotalSections = totalSections,
                TotalEnrollments = totalEnrollments,
                RecentUsers = recentUsers,
                RecentCourses = recentCourses
            };
        }

        public async Task<List<AdminUserItemViewModel>> GetUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Id)
                .Select(u => new AdminUserItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<AdminStudentItemViewModel>> GetStudentsAsync()
        {
            return await _context.Students
                .Include(s => s.Department)
                .Include(s => s.DepartmentSection)
                .Include(s => s.User)
                .OrderBy(s => s.Id)
                .Select(s => new AdminStudentItemViewModel
                {
                    Id = s.Id,
                    UniversityId = s.UniversityId,
                    FullName = s.FullName,
                    Level = s.Level,
                    DepartmentName = s.Department != null ? s.Department.Name : string.Empty,
                    SectionNumber = s.DepartmentSection != null ? s.DepartmentSection.Number : null,
                    UserName = s.User != null ? s.User.UserName : string.Empty,
                    Email = s.User != null ? s.User.Email : string.Empty
                })
                .ToListAsync();
        }

        public async Task<List<AdminDoctorItemViewModel>> GetDoctorsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == Role.Doctor)
                .Include(u => u.CourseSections)
                .OrderBy(u => u.Id)
                .Select(u => new AdminDoctorItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    CourseCount = u.CourseSections != null ? u.CourseSections.Count : 0
                })
                .ToListAsync();
        }

        public async Task<List<AdminTAItemViewModel>> GetTAsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == Role.TA)
                .Include(u => u.CourseSections)
                .OrderBy(u => u.Id)
                .Select(u => new AdminTAItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    SectionCount = u.CourseSections != null ? u.CourseSections.Count : 0
                })
                .ToListAsync();
        }

        public async Task<List<AdminCourseItemViewModel>> GetCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Doctor)
                .Include(c => c.CourseSections)
                .OrderBy(c => c.Id)
                .Select(c => new AdminCourseItemViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Semester = c.Semester,
                    DoctorName = c.Doctor != null ? c.Doctor.FullName : string.Empty,
                    Level = c.Level,
                    SectionCount = c.CourseSections != null ? c.CourseSections.Count : 0
                })
                .ToListAsync();
        }

        public async Task<List<AdminSectionItemViewModel>> GetSectionsAsync()
        {
            return await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.TA)
                .OrderBy(cs => cs.Id)
                .Select(cs => new AdminSectionItemViewModel
                {
                    Id = cs.Id,
                    CourseCode = cs.Course != null ? cs.Course.Code : string.Empty,
                    CourseName = cs.Course != null ? cs.Course.Name : string.Empty,
                    DepartmentName = cs.DepartmentSection != null && cs.DepartmentSection.Department != null
                        ? cs.DepartmentSection.Department.Name
                        : string.Empty,
                    SectionNumber = cs.DepartmentSection != null ? cs.DepartmentSection.Number : 0,
                    TAName = cs.TA != null ? cs.TA.FullName : string.Empty,
                    StudentCount = _context.Enrollments
                        .Count(e => e.CourseSectionId == cs.Id)
                })
                .ToListAsync();
        }

        public async Task<AdminUserItemViewModel?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new AdminUserItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();
        }

        #region User CRUD

        public async Task CreateUserAsync(CreateUserDto dto)
        {
            // Check for duplicate username
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate email
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists.");
                }
            }

            var user = new User
            {
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                Role = Enum.Parse<Role>(dto.Role)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {dto.Id} not found.");
            }

            // Check for duplicate username (excluding current user)
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.Id != dto.Id);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate email (excluding current user)
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists.");
                }
            }

            user.UserName = dto.UserName;
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = Enum.Parse<Role>(dto.Role);

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = dto.Password;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {id} not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Student CRUD

        public async Task<AdminStudentItemViewModel?> GetStudentByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Department)
                .Include(s => s.DepartmentSection)
                .Include(s => s.User)
                .Where(s => s.Id == id)
                .Select(s => new AdminStudentItemViewModel
                {
                    Id = s.Id,
                    UniversityId = s.UniversityId,
                    FullName = s.FullName,
                    Level = s.Level,
                    DepartmentName = s.Department != null ? s.Department.Name : string.Empty,
                    SectionNumber = s.DepartmentSection != null ? s.DepartmentSection.Number : null,
                    UserName = s.User != null ? s.User.UserName : string.Empty,
                    Email = s.User != null ? s.User.Email : string.Empty
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateStudentAsync(CreateStudentDto dto)
        {
            // Check for duplicate username
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate university ID
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UniversityId == dto.UniversityId);
            if (existingStudent != null)
            {
                throw new InvalidOperationException("University ID already exists.");
            }

            // Validate department section belongs to department
            var departmentSection = await _context.DepartmentSections
                .Include(ds => ds.Department)
                .FirstOrDefaultAsync(ds => ds.Id == dto.DepartmentSectionId);
            if (departmentSection == null)
            {
                throw new InvalidOperationException($"DepartmentSection with ID {dto.DepartmentSectionId} not found.");
            }

            if (departmentSection.DepartmentId != dto.DepartmentId)
            {
                throw new InvalidOperationException($"DepartmentSection with ID {dto.DepartmentSectionId} does not belong to Department with ID {dto.DepartmentId}.");
            }

            var user = new User
            {
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.UserName,
                Password = dto.Password,
                Role = Role.Student
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var student = new Student
            {
                UserName = dto.UserName,
                UniversityId = dto.UniversityId,
                FullName = dto.FullName,
                Level = dto.Level,
                DepartmentId = dto.DepartmentId,
                DepartmentSectionId = dto.DepartmentSectionId,
                UserId = user.Id
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStudentAsync(UpdateStudentDto dto)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (student == null)
            {
                throw new InvalidOperationException($"Student with ID {dto.Id} not found.");
            }

            // Check for duplicate username (excluding current student)
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.Id != student.UserId);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate university ID (excluding current student)
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UniversityId == dto.UniversityId && s.Id != dto.Id);
            if (existingStudent != null)
            {
                throw new InvalidOperationException("University ID already exists.");
            }

            // Validate department section belongs to department
            var departmentSection = await _context.DepartmentSections
                .Include(ds => ds.Department)
                .FirstOrDefaultAsync(ds => ds.Id == dto.DepartmentSectionId);
            if (departmentSection == null)
            {
                throw new InvalidOperationException($"DepartmentSection with ID {dto.DepartmentSectionId} not found.");
            }

            if (departmentSection.DepartmentId != dto.DepartmentId)
            {
                throw new InvalidOperationException($"DepartmentSection with ID {dto.DepartmentSectionId} does not belong to Department with ID {dto.DepartmentId}.");
            }

            student.UserName = dto.UserName;
            student.UniversityId = dto.UniversityId;
            student.FullName = dto.FullName;
            student.Level = dto.Level;
            student.DepartmentId = dto.DepartmentId;
            student.DepartmentSectionId = dto.DepartmentSectionId;

            if (student.User != null)
            {
                student.User.UserName = dto.UserName;
                student.User.FullName = dto.FullName;
                student.User.Email = dto.UserName;

                if (!string.IsNullOrEmpty(dto.Password))
                {
                    student.User.Password = dto.Password;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                throw new InvalidOperationException($"Student with ID {id} not found.");
            }

            if (student.User != null)
            {
                _context.Users.Remove(student.User);
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Doctor CRUD

        public async Task<AdminDoctorItemViewModel?> GetDoctorByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Role == Role.Doctor && u.Id == id)
                .Include(u => u.CourseSections)
                .Select(u => new AdminDoctorItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    CourseCount = u.CourseSections != null ? u.CourseSections.Count : 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateDoctorAsync(CreateUserDto dto)
        {
            // Check for duplicate username
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate email
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists.");
                }
            }

            var user = new User
            {
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                Role = Role.Doctor
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctorAsync(UpdateUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.Id && u.Role == Role.Doctor);
            if (user == null)
            {
                throw new InvalidOperationException($"Doctor with ID {dto.Id} not found.");
            }

            // Check for duplicate username (excluding current doctor)
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.Id != dto.Id);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate email (excluding current doctor)
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists.");
                }
            }

            user.UserName = dto.UserName;
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = Enum.Parse<Role>(dto.Role);

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = dto.Password;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == Role.Doctor);
            if (user == null)
            {
                throw new InvalidOperationException($"Doctor with ID {id} not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region TA CRUD

        public async Task<AdminTAItemViewModel?> GetTAByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Role == Role.TA && u.Id == id)
                .Include(u => u.CourseSections)
                .Select(u => new AdminTAItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    SectionCount = u.CourseSections != null ? u.CourseSections.Count : 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateTAAsync(CreateUserDto dto)
        {
            // Check for duplicate username
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate email
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists.");
                }
            }

            var user = new User
            {
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                Role = Role.TA
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTAAsync(UpdateUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.Id && u.Role == Role.TA);
            if (user == null)
            {
                throw new InvalidOperationException($"TA with ID {dto.Id} not found.");
            }

            // Check for duplicate username (excluding current TA)
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.Id != dto.Id);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Check for duplicate email (excluding current TA)
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException("Email already exists.");
                }
            }

            user.UserName = dto.UserName;
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = Enum.Parse<Role>(dto.Role);

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = dto.Password;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTAAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == Role.TA);
            if (user == null)
            {
                throw new InvalidOperationException($"TA with ID {id} not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Course CRUD

        public async Task<AdminCourseItemViewModel?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Doctor)
                .Include(c => c.CourseSections)
                .Where(c => c.Id == id)
                .Select(c => new AdminCourseItemViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Semester = c.Semester,
                    DoctorName = c.Doctor != null ? c.Doctor.FullName : string.Empty,
                    Level = c.Level,
                    SectionCount = c.CourseSections != null ? c.CourseSections.Count : 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateCourseAsync(CreateCourseDto dto)
        {
            // Check for duplicate course code
            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code == dto.Code);
            if (existingCourse != null)
            {
                throw new InvalidOperationException("Course code already exists.");
            }

            // Verify doctor exists
            var doctor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.DoctorId && u.Role == Role.Doctor);
            if (doctor == null)
            {
                throw new InvalidOperationException($"Doctor with ID {dto.DoctorId} not found.");
            }

            var course = new Course
            {
                Code = dto.Code,
                Name = dto.Name,
                Semester = dto.Semester,
                Level = dto.Level,
                DoctorId = dto.DoctorId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCourseAsync(UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(dto.Id);
            if (course == null)
            {
                throw new InvalidOperationException($"Course with ID {dto.Id} not found.");
            }

            // Check for duplicate course code (excluding current course)
            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code == dto.Code && c.Id != dto.Id);
            if (existingCourse != null)
            {
                throw new InvalidOperationException("Course code already exists.");
            }

            // Verify doctor exists
            var doctor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.DoctorId && u.Role == Role.Doctor);
            if (doctor == null)
            {
                throw new InvalidOperationException($"Doctor with ID {dto.DoctorId} not found.");
            }

            course.Code = dto.Code;
            course.Name = dto.Name;
            course.Semester = dto.Semester;
            course.Level = dto.Level;
            course.DoctorId = dto.DoctorId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                throw new InvalidOperationException($"Course with ID {id} not found.");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Section CRUD

        public async Task<AdminSectionItemViewModel?> GetSectionByIdAsync(int id)
        {
            return await _context.CourseSections
                .Include(cs => cs.Course)
                .Include(cs => cs.DepartmentSection)
                    .ThenInclude(ds => ds.Department)
                .Include(cs => cs.TA)
                .Where(cs => cs.Id == id)
                .Select(cs => new AdminSectionItemViewModel
                {
                    Id = cs.Id,
                    CourseCode = cs.Course != null ? cs.Course.Code : string.Empty,
                    CourseName = cs.Course != null ? cs.Course.Name : string.Empty,
                    DepartmentName = cs.DepartmentSection != null && cs.DepartmentSection.Department != null
                        ? cs.DepartmentSection.Department.Name
                        : string.Empty,
                    SectionNumber = cs.DepartmentSection != null ? cs.DepartmentSection.Number : 0,
                    TAName = cs.TA != null ? cs.TA.FullName : string.Empty,
                    StudentCount = 0 // TODO: Add actual student count
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateSectionAsync(CreateSectionDto dto)
        {
            // Verify course exists
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
            {
                throw new InvalidOperationException($"Course with ID {dto.CourseId} not found.");
            }

            // Verify department section exists
            var departmentSection = await _context.DepartmentSections.FindAsync(dto.DepartmentSectionId);
            if (departmentSection == null)
            {
                throw new InvalidOperationException($"DepartmentSection with ID {dto.DepartmentSectionId} not found.");
            }

            // Verify TA exists
            var ta = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.TAId && u.Role == Role.TA);
            if (ta == null)
            {
                throw new InvalidOperationException($"TA with ID {dto.TAId} not found.");
            }

            // Check for duplicate section
            var existingSection = await _context.CourseSections
                .FirstOrDefaultAsync(cs => cs.CourseId == dto.CourseId
                    && cs.DepartmentSectionId == dto.DepartmentSectionId);
            if (existingSection != null)
            {
                throw new InvalidOperationException("Section already exists for this course and department section.");
            }

            var section = new CourseSection
            {
                CourseId = dto.CourseId,
                DepartmentSectionId = dto.DepartmentSectionId,
                TAId = dto.TAId
            };

            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSectionAsync(UpdateSectionDto dto)
        {
            // Validation Core
            var section = await _context.CourseSections.FindAsync(dto.Id);
            if (section == null)
            {
                throw new InvalidOperationException($"Section with ID {dto.Id} not found.");
            }

            // Verify course exists
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
            {
                throw new InvalidOperationException($"Course with ID {dto.CourseId} not found.");
            }

            // Verify department section exists
            var departmentSection = await _context.DepartmentSections.FindAsync(dto.DepartmentSectionId);
            if (departmentSection == null)
            {
                throw new InvalidOperationException($"DepartmentSection with ID {dto.DepartmentSectionId} not found.");
            }

            // Verify TA exists
            var ta = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.TAId && u.Role == Role.TA);
            if (ta == null)
            {
                throw new InvalidOperationException($"TA with ID {dto.TAId} not found.");
            }

            // Check for duplicate section (excluding current section)
            var existingSection = await _context.CourseSections
                .FirstOrDefaultAsync(cs => cs.CourseId == dto.CourseId
                    && cs.DepartmentSectionId == dto.DepartmentSectionId
                    && cs.Id != dto.Id);
            if (existingSection != null)
            {
                throw new InvalidOperationException("Section already exists for this course and department section.");
            }

            section.CourseId = dto.CourseId;
            section.DepartmentSectionId = dto.DepartmentSectionId;
            section.TAId = dto.TAId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteSectionAsync(int id)
        {
            var section = await _context.CourseSections.FindAsync(id);
            if (section == null)
            {
                throw new InvalidOperationException($"Section with ID {id} not found.");
            }

            _context.CourseSections.Remove(section);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
