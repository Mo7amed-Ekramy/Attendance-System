using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EF_LSM.Entities
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

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public Role Role { get; set; }

        public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}