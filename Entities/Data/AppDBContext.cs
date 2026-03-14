using Microsoft.EntityFrameworkCore;

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
        public DbSet<Department> Departments { get; set; }
        public DbSet<CourseDepartment> CourseDepartments { get; set; }

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

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<CourseDepartment>()
                .HasIndex(cd => new { cd.CourseId, cd.DepartmentId })
                .IsUnique();

            // 1-to-many relationship 
            /*
             * الدكتور الواحد ممكن يدي اكتر من مادة، بس المادة الواحدة ليها دكتور 
             */
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Doctor)
                .WithMany()
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);


            // 1-to-Many Relationship
            /*
             * المادة الواحدة ليها اكتر من سكشن و السكشن الواحد بيكون لمادة واحدة
             */
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-to-Many Relationship
            /*
             * المعيد الواحد ممكن يدي اكتر من سكشن، بس السكشن الواحد ليه معيد واحد
             */
            modelBuilder.Entity<Section>()
                .HasOne(s => s.TA)
                .WithMany(u => u.Sections)
                .HasForeignKey(s => s.TAId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة الاساسية هي بين الطالب والمادة والسكشن بس هي ميني تو ميني ف عملنا بريدج وهو ال Enrollment
            /*
             * كل طالب ليه عملية تسجيل واحدة وعملية التسجيل ممكن يعملها اكتر من طالب
             * يعني مثلا محمود مسجل في مادة كذا وكذا وكذا 
             * يعني هو مسجل ف اكتر من مادة وعملية التسجيل  دي تخص الطالب لوحده
             */
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-M
            /*
             * كل عملية تسجيل فيها اكتر من مادة وكل مادة ليها عملية تسجيل واحدة
            */
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-m نفس كلام المادة مش هنعيد ونزيد
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Section)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-M
            /*
             * كل جلسة حضور تخص مادة واحدة وكل مادة ليها اكتر من جلسة حضور
             */
            modelBuilder.Entity<AttendanceSession>()
                .HasOne(a => a.Course)
                .WithMany(c => c.AttendanceSessions)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-M
            /*
            * كل جلسة حضور تخص سكشن واحدة وكل سكشن ليه اكتر من جلسة حضور
            */
            modelBuilder.Entity<AttendanceSession>()
                .HasOne(a => a.Section)
                .WithMany(s => s.AttendanceSessions)
                .HasForeignKey(a => a.SectionId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-M
            /*
            * كل سجل حضور يخص جلسة حضور واحدة وكل جلسة حضور ليها اكتر من سجل حضور
            */
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.AttendanceSession)
                .WithMany(a => a.Records)
                .HasForeignKey(ar => ar.AttendanceSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-M
            /*
             * كل سجل حضور يخص مادة متسجلة وكل مادة متسجلة ليها اكتر من سجل حضور
             */
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.Enrollment)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(ar => ar.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // العلاقة الاساسية هي بين المادة والقسم
            // المادة ممكن تتدرس في اكتر من قسم
            // والقسم ممكن يكون فيه اكتر من مادة
            // فالعلاقة بينهم Many-To-Many
            // علشان كده عملنا بريدج وهو CourseDepartment

            /*
             * كل سجل في CourseDepartment بيربط مادة واحدة بقسم واحد
             * يعني مثلا مادة DataBase ممكن تتدرس في قسم CS وقسم IS
             * فهنلاقي نفس الكورس متكرر في الجدول لكن مع Departments مختلفة
             */
            modelBuilder.Entity<CourseDepartment>()
                .HasOne(cd => cd.Course)
                .WithMany(c => c.CourseDepartments)
                .HasForeignKey(cd => cd.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-M


            /*
             * كل قسم ممكن يكون فيه مواد كتير
             * لكن كل سجل في CourseDepartment بيربط القسم بمادة واحدة
             * يعني مثلا قسم AI ممكن يدرس
             * Machine Learning
             * DataBase
             * Algorithms
             * فهنلاقي اكتر من سجل لنفس القسم لكن مع كورسات مختلفة
             */
            modelBuilder.Entity<CourseDepartment>()
                .HasOne(cd => cd.Department)
                .WithMany(d => d.CourseDepartments)
                .HasForeignKey(cd => cd.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1-M
        }
    }
}