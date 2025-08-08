using CRUDdemo.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace CRUDdemo.Controllers;

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
            TempData["ErrorMessage"] = "Employee not found!";
            return RedirectToAction(nameof(Index));
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

                TempData["SuccessMessage"] = $"Employee '{employee.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Xatolik: " + ex.Message);
            TempData["ErrorMessage"] = "Error creating employee: " + ex.Message;
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
            TempData["ErrorMessage"] = "Employee not found!";
            return RedirectToAction(nameof(Index));
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
                TempData["SuccessMessage"] = $"Employee '{employee.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating employee: " + ex.Message;
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
            var employee = _employeeDAL.GetEmployeeById(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found!";
                return RedirectToAction(nameof(Index));
            }

            string employeeName = employee.Name;
            int childrenCount = employee.Children?.Count ?? 0;

            _employeeDAL.DeleteEmployee(id);

            if (childrenCount > 0)
            {
                TempData["SuccessMessage"] = $"Employee '{employeeName}' and {childrenCount} children deleted successfully!";
            }
            else
            {
                TempData["SuccessMessage"] = $"Employee '{employeeName}' deleted successfully!";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error deleting employee: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult ExportToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var employees = _employeeDAL.GetAllEmployee().ToList();
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "Employee.xlsx");
        using (var package = new ExcelPackage(new FileInfo(templatePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            int startRow = 2;

            for (int i = 0; i < employees.Count; i++)
            {
                int currentRow = startRow + i;
                var emp = employees[i];

                worksheet.Cells[$"A2:H2"].Copy(worksheet.Cells[$"A{currentRow}:H{currentRow}"]);

                worksheet.Cells[currentRow, 1].Value = emp.Id;
                worksheet.Cells[currentRow, 2].Value = emp.Name;
                worksheet.Cells[currentRow, 3].Value = emp.Gender;
                worksheet.Cells[currentRow, 4].Value = emp.Company;
                worksheet.Cells[currentRow, 5].Value = emp.Department;
                worksheet.Cells[currentRow, 6].Value = emp.IsMarried ? "Married" : "Single";
                worksheet.Cells[currentRow, 7].Value = emp.Children.Count;
                worksheet.Cells[currentRow, 8].Value = string.Join(", ", emp.Children.Select(c => c.Name));
                worksheet.Cells[currentRow, 9].Value = string.Join(", ", emp.Children.Select(c => c.Age.ToString()));
                worksheet.Cells[currentRow, 10].Value = string.Join(", ", emp.Children.Select(c => c.Gender));



            }
            var fileName = $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(package.GetAsByteArray(),
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       fileName);
        }
    }

    [HttpPost]
    public IActionResult ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Fayl tanlanmadi!";
            return RedirectToAction("Index");
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        try
        {
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    var employees = new List<Employee>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var isMarriedStr = worksheet.Cells[row, 6].Value?.ToString()?.Trim().ToLower();
                        bool isMarried = isMarriedStr == "married";

                        var employee = new Employee
                        {
                            Name = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                            Gender = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                            Company = worksheet.Cells[row, 4].Value?.ToString() ?? "",
                            Department = worksheet.Cells[row, 5].Value?.ToString() ?? "",
                            IsMarried = isMarried,
                            Children = new List<Child>()
                        };

                        var childrenCountStr = worksheet.Cells[row, 7].Value?.ToString();
                        var childrenNamesStr = worksheet.Cells[row, 8].Value?.ToString();
                        var childrenAgesStr = worksheet.Cells[row, 9].Value?.ToString();
                        var childrenGendersStr = worksheet.Cells[row, 10].Value?.ToString();

                        if (employee.IsMarried &&
                            int.TryParse(childrenCountStr, out int childrenCount) &&
                            childrenCount > 0 &&
                            !string.IsNullOrWhiteSpace(childrenNamesStr))
                        {
                            var names = childrenNamesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            var ages = childrenAgesStr?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
                            var genders = childrenGendersStr?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

                            for (int i = 0; i < names.Length; i++)
                            {
                                var child = new Child
                                {
                                    Name = names[i].Trim(),
                                    Age = (i < ages.Length && int.TryParse(ages[i], out var age)) ? age : 0,
                                    Gender = (i < genders.Length) ? genders[i].Trim() : ""
                                };

                                employee.Children.Add(child);
                            }
                        }

                        employees.Add(employee);
                    }

                    foreach (var emp in employees)
                    {
                        _employeeDAL.AddEmployeeWithChildren(emp);
                    }

                    TempData["SuccessMessage"] = $"{employees.Count} ta xodim (farzandlari bilan) muvaffaqiyatli yuklandi!";
                    return RedirectToAction("Index");
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Xatolik: " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    public IActionResult ExportToPdf()
    {
        try
        {
            var employees = _employeeDAL.GetAllEmployee();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10); 
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("Employee List", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var dateParagraph = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", dateFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(dateParagraph);

                PdfPTable table = new PdfPTable(8); 
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2f, 3f, 3f, 2f, 2f, 4f, 4f });

                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                string[] headers = { "Name", "Gender", "Company", "Department", "Marital Status", "Children Count", "Children Ages", "Children Genders" };
                foreach (var header in headers)
                {
                    table.AddCell(new PdfPCell(new Phrase(header, headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 8 });
                }

                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var employee in employees)
                {
                    string maritalStatus = employee.IsMarried ? "Married" : "Single";
                    string childrenAges = string.Join(", ", employee.Children.Select(c => c.Age.ToString()));
                    string childrenGenders = string.Join(", ", employee.Children.Select(c => c.Gender));

                    table.AddCell(new PdfPCell(new Phrase(employee.Name ?? "", cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(employee.Gender ?? "", cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(employee.Company ?? "", cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(employee.Department ?? "", cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(maritalStatus, cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(employee.Children.Count.ToString(), cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(childrenAges, cellFont)) { Padding = 6 });
                    table.AddCell(new PdfPCell(new Phrase(childrenGenders, cellFont)) { Padding = 6 });
                }

                document.Add(table);

                var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var summary = new Paragraph($"\nTotal Employees: {employees.Count()}", summaryFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 20
                };
                document.Add(summary);

                document.Close();
                writer.Close();

                string fileName = $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error exporting to PDF: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}