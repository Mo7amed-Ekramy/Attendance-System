using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int AttendanceSessionId { get; set; }

        public AttendanceSession AttendanceSession { get; set; }

        public int StudentId { get; set; }

        public bool IsPresent { get; set; }
    }
}
