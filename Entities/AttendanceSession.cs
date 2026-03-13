using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class AttendanceSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceSessionType SessionType { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? SectionId { get; set; }

        [StringLength(50)]
        public string? AttendanceCode { get; set; }

        [Required]
        public bool IsClosed { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }

        [ForeignKey(nameof(SectionId))]
        public Section? Section { get; set; }

        public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
    }
}