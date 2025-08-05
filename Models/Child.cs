using System.ComponentModel.DataAnnotations;

namespace CRUDdemo.Models
{
    public class Child
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Age must be between 0 and 100")]
        public int Age { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        public Employee Employee { get; set; }
    }
}