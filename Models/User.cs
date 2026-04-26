using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        [StringLength(200)]
        public string Password { get; set; }

        public Student? Student { get; set; }

        public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}