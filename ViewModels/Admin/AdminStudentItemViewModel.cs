namespace MVC_PROJECT.ViewModels.Admin
{
    public class AdminStudentItemViewModel
    {
        public int Id { get; set; }
        public string UniversityId { get; set; }
        public string FullName { get; set; }
        public int Level { get; set; }
        public string DepartmentName { get; set; }
        public int? SectionNumber { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
