using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace EF_LSM.Entities
{
    public class Course
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public int DoctorId { get; set; }

        public Doctor Doctor { get; set; }

        public ICollection<Section> Sections { get; set; }

        public CoursePolicy Policy { get; set; }
    }
}
