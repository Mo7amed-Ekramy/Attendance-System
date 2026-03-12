using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace EF_LSM.Entities
{
    public class TA
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public ICollection<Section> Sections { get; set; }
    }
}
