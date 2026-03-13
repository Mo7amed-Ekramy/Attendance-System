using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 100)]
        public decimal MaxMark { get; set; }
    }
}