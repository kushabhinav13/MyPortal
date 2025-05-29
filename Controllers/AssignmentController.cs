using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Myportal.Data;
using Myportal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Myportal.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly InventoryDbContext _context;

        public AssignmentController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Assignment
        public async Task<IActionResult> Index(string searchString)
        {
            var assignments = _context.Assignments
                .Include(a => a.Asset)
                .Include(a => a.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                assignments = assignments.Where(a =>
                    a.Asset.Name.Contains(searchString) ||
                    a.Employee.FullName.Contains(searchString));
            }

            return View(await assignments.ToListAsync());
        }

        // GET: Assignment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Asset)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // GET: Assignment/Create
        public IActionResult Create()
        {
            ViewBag.AssetId = new SelectList(_context.Assets.Where(a => a.Status == Asset.AssetStatus.Available), "Id", "Name");
            ViewBag.EmployeeId = new SelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Assignment/Create
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("AssetId,EmployeeId,AssignedDate,ReturnDate,Notes,Status")] Assignment assignment)
{
    if (ModelState.IsValid)
    {
        var asset = await _context.Assets.FindAsync(assignment.AssetId);
        if (asset == null) return NotFound();

        if (assignment.Status == Assignment.AssignmentStatus.Active)
        {
            asset.Status = Asset.AssetStatus.Assigned;
            _context.Assets.Update(asset);
        }

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    ViewBag.AssetId = new SelectList(_context.Assets.Where(a => a.Status == Asset.AssetStatus.Available), "Id", "Name", assignment.AssetId);
    ViewBag.EmployeeId = new SelectList(_context.Employees, "Id", "FullName", assignment.EmployeeId);
    return View(assignment);
}

        // GET: Assignment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();

            ViewBag.AssetId = new SelectList(_context.Assets, "Id", "Name", assignment.AssetId);
            ViewBag.EmployeeId = new SelectList(_context.Employees, "Id", "FullName", assignment.EmployeeId);
            return View(assignment);
        }

        // POST: Assignment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetId,EmployeeId,AssignedDate,ReturnDate,Notes,Status")] Assignment assignment)
        {
            if (id != assignment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Optionally update asset status on edit
                    var asset = await _context.Assets.FindAsync(assignment.AssetId);
                    if (asset != null && assignment.Status == Assignment.AssignmentStatus.Active)
                    {
                        asset.Status = Asset.AssetStatus.Assigned;
                        _context.Assets.Update(asset);
                    }

                    _context.Assignments.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Assignments.Any(e => e.Id == assignment.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AssetId = new SelectList(_context.Assets, "Id", "Name", assignment.AssetId);
            ViewBag.EmployeeId = new SelectList(_context.Employees, "Id", "FullName", assignment.EmployeeId);
            return View(assignment);
        }

        // GET: Assignment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Asset)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // POST: Assignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment != null)
            {
                // Mark asset as available again
                assignment.Asset.Status = Asset.AssetStatus.Available;
                _context.Assets.Update(assignment.Asset);

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
