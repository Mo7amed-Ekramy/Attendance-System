using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EF_LSM.Entities.Data
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string connectionString = "Server=.\\MSSQLSERVER01;Database=LSM_Database;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePolicy> CoursePolicies { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AttendanceSession> AttendanceSessions { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizGrade> QuizGrades { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.UniversityId)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<CoursePolicy>()
                .HasIndex(cp => cp.CourseId)
                .IsUnique();

            modelBuilder.Entity<Section>()
                .HasIndex(s => new { s.CourseId, s.Number })
                .IsUnique();

            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();

            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(ar => new { ar.EnrollmentId, ar.AttendanceSessionId })
                .IsUnique();

            modelBuilder.Entity<QuizGrade>()
                .HasIndex(qg => new { qg.QuizId, qg.EnrollmentId })
                .IsUnique();
        }
    }
}