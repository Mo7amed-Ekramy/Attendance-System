using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace MVC_PROJECT.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePolicy> CoursePolicies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentSection> DepartmentSections { get; set; }
        public DbSet<CourseDepartment> CourseDepartments { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AttendanceSession> AttendanceSessions { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizGrade> QuizGrades { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Quiz>()
                .Property(q => q.MaxMark)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuizGrade>()
                .Property(qg => qg.Mark)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuizGrade>()
                .Property(qg => qg.PercentageScore)
                .HasPrecision(18, 2);
            // =========================
            // Unique Indexes
            // =========================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.UniversityId)
                .IsUnique();

            // Unique index for UserId (one Student per User)
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<CoursePolicy>()
                .HasIndex(cp => cp.CourseId)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<DepartmentSection>()
                .HasIndex(ds => new { ds.DepartmentId, ds.Number })
                .IsUnique();

            modelBuilder.Entity<CourseDepartment>()
                .HasIndex(cd => new { cd.CourseId, cd.DepartmentId })
                .IsUnique();

            modelBuilder.Entity<CourseSection>()
                .HasIndex(cs => new { cs.CourseId, cs.DepartmentSectionId })
                .IsUnique();

            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseSectionId })
                .IsUnique();

            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(ar => new { ar.EnrollmentId, ar.AttendanceSessionId })
                .IsUnique();

            modelBuilder.Entity<QuizGrade>()
                .HasIndex(qg => new { qg.QuizId, qg.EnrollmentId })
                .IsUnique();

            // =========================
            // Relationships
            // =========================

            // Course -> Doctor
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Doctor)
                .WithMany()
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student -> Department
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student -> DepartmentSection
            modelBuilder.Entity<Student>()
                .HasOne(s => s.DepartmentSection)
                .WithMany(ds => ds.Students)
                .HasForeignKey(s => s.DepartmentSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student -> User (one-to-one relationship)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // DepartmentSection -> Department
            modelBuilder.Entity<DepartmentSection>()
                .HasOne(ds => ds.Department)
                .WithMany(d => d.Sections)
                .HasForeignKey(ds => ds.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CoursePolicy -> Course
            modelBuilder.Entity<CoursePolicy>()
                .HasOne(cp => cp.Course)
                .WithOne(c => c.CoursePolicy)
                .HasForeignKey<CoursePolicy>(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseDepartment -> Course
            modelBuilder.Entity<CourseDepartment>()
                .HasOne(cd => cd.Course)
                .WithMany(c => c.CourseDepartments)
                .HasForeignKey(cd => cd.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseDepartment -> Department
            modelBuilder.Entity<CourseDepartment>()
                .HasOne(cd => cd.Department)
                .WithMany(d => d.CourseDepartments)
                .HasForeignKey(cd => cd.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseSection -> Course
            modelBuilder.Entity<CourseSection>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSections)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseSection -> DepartmentSection
            modelBuilder.Entity<CourseSection>()
                .HasOne(cs => cs.DepartmentSection)
                .WithMany(ds => ds.CourseSections)
                .HasForeignKey(cs => cs.DepartmentSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseSection -> TA
            modelBuilder.Entity<CourseSection>()
                .HasOne(cs => cs.TA)
                .WithMany(u => u.CourseSections)
                .HasForeignKey(cs => cs.TAId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment -> Student
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment -> CourseSection
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.CourseSection)
                .WithMany(cs => cs.Enrollments)
                .HasForeignKey(e => e.CourseSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceSession -> CourseSection
            modelBuilder.Entity<AttendanceSession>()
                .HasOne(a => a.CourseSection)
                .WithMany(cs => cs.AttendanceSessions)
                .HasForeignKey(a => a.CourseSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceRecord -> AttendanceSession
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.AttendanceSession)
                .WithMany(a => a.Records)
                .HasForeignKey(ar => ar.AttendanceSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceRecord -> Enrollment
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.Enrollment)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(ar => ar.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quiz -> CourseSection
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.CourseSection)
                .WithMany(cs => cs.Quizzes)
                .HasForeignKey(q => q.CourseSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizGrade -> Quiz
            modelBuilder.Entity<QuizGrade>()
                .HasOne(qg => qg.Quiz)
                .WithMany(q => q.QuizGrades)
                .HasForeignKey(qg => qg.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizGrade -> Enrollment
            modelBuilder.Entity<QuizGrade>()
                .HasOne(qg => qg.Enrollment)
                .WithMany(e => e.QuizGrades)
                .HasForeignKey(qg => qg.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges()
        {
            ValidateDepartmentSectionConsistency();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ValidateDepartmentSectionConsistency();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ValidateDepartmentSectionConsistency()
        {
            var studentEntries = ChangeTracker.Entries<Student>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in studentEntries)
            {
                var student = entry.Entity;
                
                // Check if DepartmentSectionId or DepartmentId properties have changed
                bool departmentSectionChanged = false;
                bool departmentChanged = false;
                
                if (entry.State == EntityState.Modified)
                {
                    departmentSectionChanged = entry.Property(nameof(Student.DepartmentSectionId)).IsModified;
                    departmentChanged = entry.Property(nameof(Student.DepartmentId)).IsModified;
                }
                else // EntityState.Added
                {
                    departmentSectionChanged = true;
                    departmentChanged = true;
                }

                // Only validate if either property has changed
                if (departmentSectionChanged || departmentChanged)
                {
                    // Load the DepartmentSection to check its DepartmentId
                    var departmentSection = DepartmentSections
                        .Include(ds => ds.Department)
                        .FirstOrDefault(ds => ds.Id == student.DepartmentSectionId);

                    if (departmentSection == null)
                    {
                        throw new InvalidOperationException(
                            $"Cannot save Student {student.Id}: DepartmentSection with ID {student.DepartmentSectionId} does not exist.");
                    }

                    if (departmentSection.DepartmentId != student.DepartmentId)
                    {
                        throw new InvalidOperationException(
                            $"Cannot save Student {student.Id}: DepartmentSection with ID {student.DepartmentSectionId} " +
                            $"(belongs to Department ID: {departmentSection.DepartmentId}) " +
                            $"does not match Student's Department ID: {student.DepartmentId}.");
                    }
                }
            }
        }
    }
}