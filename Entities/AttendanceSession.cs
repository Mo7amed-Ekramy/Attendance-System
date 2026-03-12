using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class AttendanceSession
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public AttendanceType Type { get; set; }

        public int CourseId { get; set; }

        public int? SectionId { get; set; }

        public string AttendanceCode { get; set; }

        public bool IsClosed { get; set; }

        public ICollection<AttendanceRecord> Records { get; set; }
    }
}
