using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Section
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int CourseId { get; set; }
        public int TAId { get; set; }
        public int StudentCount { get; set; }
    }
}
