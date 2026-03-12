using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Section
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public int TeachingAssistantId { get; set; }

        public TA TeachingAssistant { get; set; }

        public int SectionNumber { get; set; }

        public ICollection<Enrollment> Students { get; set; }
    }
}
