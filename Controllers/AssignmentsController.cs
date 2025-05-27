using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Myportal.Models;
using Myportal.Data;
using System.ComponentModel.DataAnnotations;

namespace Myportal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(InventoryDbContext context, ILogger<AssignmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignments()
        {
            try
            {
                return await _context.Assignments
                    .Include(a => a.Asset)
                    .Include(a => a.Employee)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assignments");
                return StatusCode(500, "An error occurred while retrieving assignments");
            }
        }

        // POST: api/Assignments
        [HttpPost]
        public async Task<ActionResult<Assignment>> PostAssignment([FromBody] Assignment assignment)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if asset exists and is available
                var asset = await _context.Assets.FindAsync(assignment.AssetId);
                if (asset == null)
                {
                    return NotFound("Asset not found");
                }

                if (asset.Status != Asset.AssetStatus.Available)
                {
                    return BadRequest($"Asset is not available for assignment (current status: {asset.Status})");
                }

                // Check if employee exists
                if (!await _context.Employees.AnyAsync(e => e.Id == assignment.EmployeeId))
                {
                    return NotFound("Employee not found");
                }

                // Update asset status
                asset.Status = Asset.AssetStatus.Assigned;
                _context.Entry(asset).State = EntityState.Modified;

                // Create assignment
                assignment.AssignedDate = DateTime.UtcNow;
                assignment.Status = Assignment.AssignmentStatus.Active;
                _context.Assignments.Add(assignment);

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment");
                return StatusCode(500, "An error occurred while creating the assignment");
            }
        }

        // PUT: api/Assignments/5/return
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnAssignment(int id)
        {
            try
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Asset)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (assignment == null)
                {
                    return NotFound("Assignment not found");
                }

                if (assignment.ReturnDate.HasValue)
                {
                    return BadRequest("Assignment is already returned");
                }

                // Update assignment
                assignment.ReturnDate = DateTime.UtcNow;
                assignment.Status = Assignment.AssignmentStatus.Returned;

                // Update asset status
                assignment.Asset.Status = Asset.AssetStatus.Available;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error returning assignment with id {id}");
                return StatusCode(500, "An error occurred while processing the return");
            }
        }

        // GET: api/Assignments/asset/5
        [HttpGet("asset/{assetId}")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssetAssignments(int assetId)
        {
            try
            {
                var assignments = await _context.Assignments
                    .Where(a => a.AssetId == assetId)
                    .Include(a => a.Employee)
                    .ToListAsync();

                if (!assignments.Any())
                {
                    return NotFound("No assignments found for this asset");
                }

                return assignments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving assignments for asset {assetId}");
                return StatusCode(500, "An error occurred while retrieving assignments");
            }
        }

        // GET: api/Assignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int id)
        {
            try
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Asset)
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (assignment == null)
                {
                    return NotFound();
                }

                return assignment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving assignment with id {id}");
                return StatusCode(500, "An error occurred while retrieving the assignment");
            }
        }
    }
}