using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}
