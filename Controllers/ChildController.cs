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
                try
                {
                    _childDAL.AddChild(child);
                    TempData["SuccessMessage"] = $"Child '{child.Name}' added successfully!";
                    return RedirectToAction("Index", new { employeeId = child.EmployeeId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error adding child: " + ex.Message;
                }
            }

            ViewBag.EmployeeId = child.EmployeeId;
            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }

        public IActionResult Edit(int id)
        {
            var child = _childDAL.GetChildById(id);
            if (child == null)
            {
                TempData["ErrorMessage"] = "Child not found!";
                return RedirectToAction("Index");
            }

            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Child child)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _childDAL.UpdateChild(child);
                    TempData["SuccessMessage"] = $"Child '{child.Name}' updated successfully!";
                    return RedirectToAction("Index", new { employeeId = child.EmployeeId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error updating child: " + ex.Message;
                }
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
            try
            {
                var child = _childDAL.GetChildById(id);
                if (child == null)
                {
                    TempData["ErrorMessage"] = "Child not found!";
                    return RedirectToAction("Index");
                }

                int employeeId = child.EmployeeId;
                string childName = child.Name;

                _childDAL.DeleteChild(id);
                TempData["SuccessMessage"] = $"Child '{childName}' deleted successfully!";

                return RedirectToAction("Index", new { employeeId = employeeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting child: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult Details(int id)
        {
            var child = _childDAL.GetChildById(id);
            if (child == null)
            {
                TempData["ErrorMessage"] = "Child not found!";
                return RedirectToAction("Index");
            }

            ViewBag.Employee = _employeeDAL.GetEmployeeById(child.EmployeeId);
            return View(child);
        }
    }
}