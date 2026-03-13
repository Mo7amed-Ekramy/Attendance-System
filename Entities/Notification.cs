using System.ComponentModel.DataAnnotations;

namespace EF_LSM.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public bool IsRead { get; set; }
    }
}