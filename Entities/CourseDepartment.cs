using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    using System.ComponentModel.DataAnnotations;

    namespace EF_LSM.Entities
    {
        public class CourseDepartment
        {
            [Key]
            public int Id { get; set; }

            public int CourseId { get; set; }

            public int DepartmentId { get; set; }

            public Course Course { get; set; }

            public Department Department { get; set; }
        }
    }
}
