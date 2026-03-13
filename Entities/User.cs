using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    internal class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public UserRole Role { get; set; }
    }
}
