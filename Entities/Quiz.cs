using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Quiz
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public decimal MaxMark { get; set; }
    }
}
