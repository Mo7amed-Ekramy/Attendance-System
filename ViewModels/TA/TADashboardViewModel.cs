using System.Collections.Generic;

namespace MVC_PROJECT.ViewModels.TA
{
    public class TADashboardViewModel
    {
        public int TAId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
        public int PendingAttendanceSessions { get; set; }
        public List<TASectionItemViewModel> Sections { get; set; } = new List<TASectionItemViewModel>();
    }
}
