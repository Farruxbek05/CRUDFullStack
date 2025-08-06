using CRUDdemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRUDdemo.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeDAL _employeeDAL;

        public EmployeeController()
        {
            _employeeDAL = new EmployeeDAL();
        }

        public IActionResult Index()
        {
            var employees = _employeeDAL.GetAllEmployee();
            return View(employees);
        }

        public IActionResult Details(int id)
        {
            var employee = _employeeDAL.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        public IActionResult Create()
        {
            var model = new Employee();
            model.Children = new List<Child>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (employee.IsMarried && employee.Children != null && employee.Children.Any())
                    {
                        _employeeDAL.AddEmployeeWithChildren(employee);
                    }
                    else
                    {
                        _employeeDAL.AddEmployee(employee);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Xatolik: " + ex.Message);
                ModelState.AddModelError("", "Xatolik yuz berdi: " + ex.Message);
            }

            if (employee.Children == null)
                employee.Children = new List<Child>();

            return View(employee);
        }


        public IActionResult Edit(int id)
        {
            var employee = _employeeDAL.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _employeeDAL.UpdateEmployee(employee);
                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the employee: " + ex.Message);
                }
            }
            return View(employee);
        }

        public IActionResult Delete(int id)
        {
            var employee = _employeeDAL.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _employeeDAL.DeleteEmployee(id);
                TempData["SuccessMessage"] = "Employee deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the employee: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public JsonResult ValidateEmployee(Employee employee)
        {
            var isValid = true;
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                isValid = false;
                errors.Add("Name is required");
            }

            if (string.IsNullOrWhiteSpace(employee.Gender))
            {
                isValid = false;
                errors.Add("Gender is required");
            }

            if (string.IsNullOrWhiteSpace(employee.Company))
            {
                isValid = false;
                errors.Add("Company is required");
            }

            if (string.IsNullOrWhiteSpace(employee.Department))
            {
                isValid = false;
                errors.Add("Department is required");
            }

            return Json(new { isValid = isValid, errors = errors });
        }
    }
}