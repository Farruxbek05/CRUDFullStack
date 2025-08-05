using CRUDdemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRUDdemo.Controllers
{
    public class ChildController : Controller
    {
        private readonly ChildDAL _childDAL = new ChildDAL();
        private readonly EmployeeDAL _employeeDAL = new EmployeeDAL();

        public IActionResult Index(int employeeId)
        {
            ViewBag.EmployeeId = employeeId;
            ViewBag.Employee = _employeeDAL.GetEmployeeById(employeeId);
            var children = _childDAL.GetChildrenByEmployeeId(employeeId);
            return View(children);
        }

        public IActionResult Create(int employeeId)
        {
            ViewBag.EmployeeId = employeeId;
            ViewBag.Employee = _employeeDAL.GetEmployeeById(employeeId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Child child)
        {
            if (ModelState.IsValid)
            {
                _childDAL.AddChild(child);
                return RedirectToAction("Index", new { employeeId = child.EmployeeId });
            }

            ViewBag.EmployeeId = child.EmployeeId;
            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }


        public IActionResult Edit(int id)
        {
            var child = _childDAL.GetChildById(id);
            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Child child)
        {
            if (ModelState.IsValid)
            {
                _childDAL.UpdateChild(child);
                return RedirectToAction("Index", new { employeeId = child.EmployeeId });
            }
            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }

        public IActionResult Delete(int id)
        {
            var child = _childDAL.GetChildById(id);
            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var child = _childDAL.GetChildById(id);
            int employeeId = child.EmployeeId;
            _childDAL.DeleteChild(id);
            return RedirectToAction("Index", new { employeeId = employeeId });
        }

        public IActionResult Details(int id)
        {
            var child = _childDAL.GetChildById(id);
            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }
    }
}