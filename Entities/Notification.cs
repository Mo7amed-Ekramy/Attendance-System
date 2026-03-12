using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
     public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public bool IsRead { get; set; }
    }
}
