using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_PROJECT.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(30)]
        public string UniversityId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Range(1, 10)]
        public int Level { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int DepartmentSectionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; }

        [ForeignKey(nameof(DepartmentSectionId))]
        public DepartmentSection DepartmentSection { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        
        /// Validates that the DepartmentSection belongs to the specified Department.
        /// This should be called before saving the entity.
        
        public bool ValidateDepartmentSectionConsistency(DepartmentSection departmentSection)
        {
            return departmentSection != null && departmentSection.DepartmentId == this.DepartmentId;
        }

        /// <summary>
        /// Gets a validation error message if DepartmentSection doesn't belong to Department.
        /// </summary>
        public string GetDepartmentSectionValidationError(DepartmentSection departmentSection)
        {
            if (departmentSection == null)
            {
                return $"DepartmentSection with ID {DepartmentSectionId} does not exist.";
            }
            
            if (departmentSection.DepartmentId != DepartmentId)
            {
                return $"DepartmentSection with ID {DepartmentSectionId} (Department ID: {departmentSection.DepartmentId}) " +
                       $"does not belong to Department with ID {DepartmentId}.";
            }
            
            return null;
        }
    }
}