using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Semester { get; set; }
        public int DoctorId { get; set; }
    }
}
