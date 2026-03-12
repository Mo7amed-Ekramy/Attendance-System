using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class QuizResult
    {
        public int Id { get; set; }

        public int QuizId { get; set; }

        public Quiz Quiz { get; set; }

        public int StudentId { get; set; }

        public double Score { get; set; }
    }
}
