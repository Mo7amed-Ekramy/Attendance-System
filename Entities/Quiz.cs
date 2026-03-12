using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Quiz
    {
        public int Id { get; set; }

        public int SectionId { get; set; }

        public Section Section { get; set; }

        public DateTime Date { get; set; }

        public int MaxScore { get; set; }

        public ICollection<QuizResult> Results { get; set; }
    }
}
