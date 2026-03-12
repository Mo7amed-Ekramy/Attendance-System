using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Doctor
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public ICollection<Course> Courses { get; set; }
    }
}
