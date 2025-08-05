namespace CRUDdemo.Models
{
    public class EmployeeViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public List<Child> Children { get; set; } = new List<Child>();
        public int ChildrenCount { get; set; } = 0;
    }
}