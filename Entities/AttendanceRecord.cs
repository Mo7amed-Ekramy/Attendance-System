using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AttendanceSessionId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        [ForeignKey(nameof(AttendanceSessionId))]
        public AttendanceSession AttendanceSession { get; set; }

        [ForeignKey(nameof(EnrollmentId))]
        public Enrollment Enrollment { get; set; }
    }
}