using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public AttendanceSessionType Type { get; set; }

        [Required]
        public bool IsRead { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}