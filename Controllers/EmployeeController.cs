using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Myportal.Data;
using Myportal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Myportal.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly InventoryDbContext _context;

        public EmployeeController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: /Employee
        public async Task<IActionResult> Index(string searchString)
        {
            var employees = from e in _context.Employees
                            select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e =>
                    e.FirstName.Contains(searchString) ||
                    e.LastName.Contains(searchString) ||
                    e.Email.Contains(searchString) ||
                    e.Department.Contains(searchString));
            }

            return View(await employees.ToListAsync());
        }

        // GET: /Employee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: /Employee/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,Department,HireDate,TerminationDate,IsActive")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);

                if (employee.TerminationDate.HasValue)
                    employee.TerminationDate = DateTime.SpecifyKind(employee.TerminationDate.Value, DateTimeKind.Utc);

                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: /Employee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: /Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Department,HireDate,TerminationDate,IsActive")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);

                    if (employee.TerminationDate.HasValue)
                        employee.TerminationDate = DateTime.SpecifyKind(employee.TerminationDate.Value, DateTimeKind.Utc);

                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // GET: /Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: /Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}