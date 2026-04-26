using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MVC_PROJECT.Models;
using MVC_PROJECT.Models.Data;

namespace MVC_PROJECT.Data
{
    public static class DbSeeder
    {
        private class SeedMaps
        {
            public Dictionary<int, int> Departments { get; set; } = new();
            public Dictionary<int, int> DepartmentSections { get; set; } = new();
            public Dictionary<int, int> Users { get; set; } = new();
            public Dictionary<int, int> Courses { get; set; } = new();
        }

        public static async Task SeedAsync(AppDbContext context, IWebHostEnvironment env)
        {
            await context.Database.MigrateAsync();

            if (await context.Departments.AnyAsync())
                return;

            var path = Path.Combine(env.ContentRootPath, "Seeding");
            var maps = new SeedMaps();

            await SeedDepartments(context, path, maps);
            await SeedDepartmentSections(context, path, maps);
            await SeedUsers(context, path, maps);
            await SeedCourses(context, path, maps);
            await SeedCourseDepartments(context, path, maps);
            await SeedCourseSections(context, path, maps);
            await SeedStudents(context, path, maps);
            await GenerateEnrollments(context);
        }

        private static async Task<List<T>> ReadJsonAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Seed file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<T>();
        }

        private static async Task SeedDepartments(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<DepartmentSeedDto>(Path.Combine(path, "Department.json"));

            foreach (var item in items)
            {
                var department = new Department
                {
                    Code = item.Code,
                    Name = item.Name
                };

                context.Departments.Add(department);
                await context.SaveChangesAsync();

                maps.Departments[item.Id] = department.Id;
            }
        }

        private static async Task SeedDepartmentSections(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<DepartmentSectionSeedDto>(Path.Combine(path, "DepartmentSection.json"));

            foreach (var item in items)
            {
                if (!maps.Departments.ContainsKey(item.DepartmentId))
                    throw new Exception($"Invalid DepartmentId {item.DepartmentId} in DepartmentSection.json");

                var section = new DepartmentSection
                {
                    DepartmentId = maps.Departments[item.DepartmentId],
                    Number = item.Number
                };

                context.DepartmentSections.Add(section);
                await context.SaveChangesAsync();

                maps.DepartmentSections[item.Id] = section.Id;
            }
        }

        private static async Task SeedUsers(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<UserSeedDto>(Path.Combine(path, "User.json"));

            foreach (var item in items)
            {
                var role = Enum.Parse<Role>(item.Role, true);

                var user = new User
                {
                    UserName = item.UserName,
                    FullName = item.FullName,
                    Email = item.Email,
                    Role = role,
                    Password = GetDefaultPassword(role)
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                if (item.Id.HasValue)
                    maps.Users[item.Id.Value] = user.Id;
            }
        }

        private static async Task SeedCourses(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<CourseSeedDto>(Path.Combine(path, "Course.json"));

            foreach (var item in items)
            {
                if (!maps.Users.ContainsKey(item.DoctorId))
                    throw new Exception($"Invalid DoctorId {item.DoctorId} in Course.json");

                var realDoctorId = maps.Users[item.DoctorId];

                var doctor = await context.Users.FirstOrDefaultAsync(u =>
                    u.Id == realDoctorId &&
                    u.Role == Role.Doctor);

                if (doctor == null)
                    throw new Exception($"User mapped from DoctorId {item.DoctorId} is not a Doctor.");

                var course = new Course
                {
                    Code = item.Code,
                    Name = item.Name,
                    Semester = item.Semester,
                    DoctorId = realDoctorId,
                    Level = item.Level
                };

                context.Courses.Add(course);
                await context.SaveChangesAsync();

                maps.Courses[item.Id] = course.Id;
            }
        }

        private static async Task SeedCourseDepartments(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<CourseDepartmentSeedDto>(Path.Combine(path, "CourseDepartment.json"));

            foreach (var item in items)
            {
                if (!maps.Courses.ContainsKey(item.CourseId))
                    throw new Exception($"Invalid CourseId {item.CourseId} in CourseDepartment.json");

                if (!maps.Departments.ContainsKey(item.DepartmentId))
                    throw new Exception($"Invalid DepartmentId {item.DepartmentId} in CourseDepartment.json");

                context.CourseDepartments.Add(new CourseDepartment
                {
                    CourseId = maps.Courses[item.CourseId],
                    DepartmentId = maps.Departments[item.DepartmentId]
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCourseSections(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<CourseSectionSeedDto>(Path.Combine(path, "CourseSection.json"));

            foreach (var item in items)
            {
                if (!maps.Courses.ContainsKey(item.CourseId))
                    throw new Exception($"Invalid CourseId {item.CourseId} in CourseSection.json");

                if (!maps.DepartmentSections.ContainsKey(item.DepartmentSectionId))
                    throw new Exception($"Invalid DepartmentSectionId {item.DepartmentSectionId} in CourseSection.json");

                if (!maps.Users.ContainsKey(item.TAId))
                    throw new Exception($"Invalid TAId {item.TAId} in CourseSection.json");

                var realTAId = maps.Users[item.TAId];

                var ta = await context.Users.FirstOrDefaultAsync(u =>
                    u.Id == realTAId &&
                    u.Role == Role.TA);

                if (ta == null)
                    throw new Exception($"User mapped from TAId {item.TAId} is not a TA.");

                context.CourseSections.Add(new CourseSection
                {
                    CourseId = maps.Courses[item.CourseId],
                    DepartmentSectionId = maps.DepartmentSections[item.DepartmentSectionId],
                    TAId = realTAId
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedStudents(AppDbContext context, string path, SeedMaps maps)
        {
            var items = await ReadJsonAsync<StudentSeedDto>(Path.Combine(path, "Student.json"));

            foreach (var item in items)
            {
                var universityId = item.UniversityId.ToString();

                if (string.IsNullOrWhiteSpace(item.UserName))
                    throw new Exception($"Student {universityId} has no username");

                if (string.IsNullOrWhiteSpace(universityId))
                    throw new Exception($"Student with name {item.FullName} has no UniversityId");

                if (!maps.Departments.ContainsKey(item.DepartmentId))
                    throw new Exception($"Invalid DepartmentId {item.DepartmentId} for student {universityId}");

                if (!maps.DepartmentSections.ContainsKey(item.DepartmentSectionId))
                    throw new Exception($"Invalid DepartmentSectionId {item.DepartmentSectionId} for student {universityId}");

                var existingStudent = await context.Students
                    .FirstOrDefaultAsync(s => s.UniversityId == universityId);

                if (existingStudent != null)
                    continue;

                var existingUser = await context.Users
                    .FirstOrDefaultAsync(u => u.UserName == item.UserName);

                if (existingUser != null)
                    continue;

                var realDepartmentId = maps.Departments[item.DepartmentId];
                var realDepartmentSectionId = maps.DepartmentSections[item.DepartmentSectionId];

                var section = await context.DepartmentSections
                    .FirstOrDefaultAsync(ds => ds.Id == realDepartmentSectionId);

                if (section == null)
                    throw new Exception($"Section not found for student {universityId}");

                if (section.DepartmentId != realDepartmentId)
                    throw new Exception($"Student {universityId}: Section does not belong to Department.");

                var user = new User
                {
                    UserName = item.UserName,
                    FullName = item.FullName,
                    Email = null,
                    Role = Role.Student,
                    Password = universityId
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                context.Students.Add(new Student
                {
                    UserName = item.UserName,
                    UniversityId = universityId,
                    FullName = item.FullName,
                    Level = item.Level,
                    DepartmentId = realDepartmentId,
                    DepartmentSectionId = realDepartmentSectionId,
                    UserId = user.Id
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task GenerateEnrollments(AppDbContext context)
        {
            if (await context.Enrollments.AnyAsync())
                return;

            var students = await context.Students.ToListAsync();

            foreach (var student in students)
            {
                var courseIds = await context.CourseDepartments
                    .Where(cd => cd.DepartmentId == student.DepartmentId)
                    .Select(cd => cd.CourseId)
                    .ToListAsync();

                foreach (var courseId in courseIds)
                {
                    var courseSection = await context.CourseSections
                        .FirstOrDefaultAsync(cs =>
                            cs.CourseId == courseId &&
                            cs.DepartmentSectionId == student.DepartmentSectionId);

                    if (courseSection == null)
                        continue;

                    var exists = await context.Enrollments.AnyAsync(e =>
                        e.StudentId == student.Id &&
                        e.CourseSectionId == courseSection.Id);

                    if (!exists)
                    {
                        context.Enrollments.Add(new Enrollment
                        {
                            StudentId = student.Id,
                            CourseSectionId = courseSection.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static string GetDefaultPassword(Role role)
        {
            return role switch
            {
                Role.Admin => "Admin@123",
                Role.Doctor => "Doctor@123",
                Role.TA => "TA@123",
                _ => "User@123"
            };
        }

        private class DepartmentSeedDto
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        private class DepartmentSectionSeedDto
        {
            public int Id { get; set; }
            public int DepartmentId { get; set; }
            public int Number { get; set; }
        }

        private class UserSeedDto
        {
            public int? Id { get; set; }
            public string UserName { get; set; }
            public string FullName { get; set; }
            public string? Email { get; set; }
            public string Role { get; set; }
        }

        private class CourseSeedDto
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Semester { get; set; }
            public int DoctorId { get; set; }
            public int Level { get; set; }
        }

        private class CourseDepartmentSeedDto
        {
            public int CourseId { get; set; }
            public int DepartmentId { get; set; }
        }

        private class CourseSectionSeedDto
        {
            public int CourseId { get; set; }
            public int DepartmentSectionId { get; set; }
            public int TAId { get; set; }
        }

        private class StudentSeedDto
        {
            public long UniversityId { get; set; }
            public string FullName { get; set; }
            public string UserName { get; set; }
            public int Level { get; set; }
            public int DepartmentId { get; set; }
            public int DepartmentSectionId { get; set; }
        }
    }
}