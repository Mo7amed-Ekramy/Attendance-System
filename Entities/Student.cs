using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Student
    {
        public int Id { get; set; }

        public string UniversityId { get; set; }

        public string FullName { get; set; }

        public int Level { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
