using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Enrollment
    {
        public int StudentId { get; set; }

        public Student Student { get; set; }

        public int SectionId { get; set; }

        public Section Section { get; set; }
    }
}
