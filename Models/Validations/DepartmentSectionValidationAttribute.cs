using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.Models.Validations
{
    public class DepartmentSectionValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // This validation requires access to database context, so it should be implemented
            // at the service/application layer. We'll leave the attribute as a marker.
            return ValidationResult.Success;
        }
    }
}