using System.ComponentModel.DataAnnotations;

namespace CRUDdemo.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        [StringLength(100)]
        public string Company { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        [Required]
        public bool IsMarried { get; set; }

        public List<Child> Children { get; set; } = new List<Child>();
    }
}