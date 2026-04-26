namespace MVC_PROJECT.ViewModels.Admin
{
    public class AdminCourseItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Semester { get; set; }
        public string DoctorName { get; set; }
        public int Level { get; set; }
        public int SectionCount { get; set; }
    }
}
