using CRUDdemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRUDdemo.Controllers;
[Route("[controller]/[action]")]
public class EmployeeController : Controller
{
    EmployeeDAL employeeDAL = new EmployeeDAL();
    public IActionResult Index()
    {
        List<Employee> employees = new List<Employee>();
        employees = employeeDAL.GetAllEmployee().ToList();
        return View(employees);

    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind] Employee employee)
    {
        if (ModelState.IsValid)
        {
            employeeDAL.AddEmployee(employee);
            return RedirectToAction("Index");
        }
        return View(employee);
    }

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Employee employee = employeeDAL.GetEmployeeById(id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind] Employee employee)
    {
        if (id != employee.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            employeeDAL.UpdateEmployee(employee);
            return RedirectToAction("Index");
        }

        return View(employee); 
    }

    [HttpGet]
    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        Employee emp = employeeDAL.GetEmployeeById(id);
        if (emp == null)
        {
            return NotFound();
        }
        return View(emp);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        Employee emp = employeeDAL.GetEmployeeById(id);
        if (emp == null)
        {
            return NotFound();
        }
        return View(emp);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteEmp(int? id)
    {
        employeeDAL.DeleteEmployee(id);
        return RedirectToAction("Index");
    }
}