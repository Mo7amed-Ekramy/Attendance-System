using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class CoursePolicy
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public int SectionAttendanceMarks { get; set; }

        public int QuizMarks { get; set; }

        public int LectureAttendanceMarks { get; set; }

        public int AllowedAbsences { get; set; }

        public int BestQuizzesCount { get; set; }
    }
}
