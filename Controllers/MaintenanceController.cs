using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Myportal.Models;
using Myportal.Data;
using System.ComponentModel.DataAnnotations;

namespace Myportal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(InventoryDbContext context, ILogger<MaintenanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Maintenance
        [HttpPost]
        public async Task<ActionResult<MaintenanceLog>> LogMaintenance([FromBody] MaintenanceLog maintenanceLog)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if asset exists
                var asset = await _context.Assets.FindAsync(maintenanceLog.AssetId);
                if (asset == null)
                {
                    return NotFound("Asset not found");
                }

                // Update asset status if needed
                if (asset.Status != Asset.AssetStatus.InMaintenance)
                {
                    asset.Status = Asset.AssetStatus.InMaintenance;
                    _context.Entry(asset).State = EntityState.Modified;
                }

                // Set default values
                maintenanceLog.MaintenanceDate = DateTime.UtcNow;
                maintenanceLog.Status = MaintenanceLog.MaintenanceStatus.Pending;

                _context.MaintenanceLogs.Add(maintenanceLog);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMaintenanceLog), 
                    new { id = maintenanceLog.Id }, 
                    maintenanceLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging maintenance");
                return StatusCode(500, "An error occurred while logging maintenance");
            }
        }

        // PUT: api/Maintenance/5/complete
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteMaintenance(int id, 
            [FromBody] MaintenanceStatusDto statusDto)
        {
            try
            {
                var log = await _context.MaintenanceLogs
                    .Include(m => m.Asset)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (log == null)
                {
                    return NotFound("Maintenance log not found");
                }

                // Validate status
                if (!Enum.IsDefined(typeof(MaintenanceLog.MaintenanceStatus), statusDto.Status))
                {
                    return BadRequest("Invalid maintenance status");
                }

                log.Status = statusDto.Status;

                // If maintenance is completed, set asset back to available
                if (statusDto.Status == MaintenanceLog.MaintenanceStatus.Completed)
                {
                    log.Asset.Status = Asset.AssetStatus.Available;
                    _context.Entry(log.Asset).State = EntityState.Modified;
                }

                
                _context.Entry(log).State = EntityState.Modified;
                
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing maintenance log {id}");
                return StatusCode(500, "An error occurred while completing maintenance");
            }
        }

        // GET: api/Maintenance/asset/5
        [HttpGet("asset/{assetId}")]
        public async Task<ActionResult<IEnumerable<MaintenanceLog>>> GetAssetMaintenanceLogs(int assetId)
        {
            try
            {
                var logs = await _context.MaintenanceLogs
                    .Where(m => m.AssetId == assetId)
                    .OrderByDescending(m => m.MaintenanceDate)
                    .ToListAsync();

                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance logs for asset {assetId}");
                return StatusCode(500, "An error occurred while retrieving maintenance logs");
            }
        }

        // GET: api/Maintenance/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceLog>> GetMaintenanceLog(int id)
        {
            try
            {
                var log = await _context.MaintenanceLogs
                    .Include(m => m.Asset)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (log == null)
                {
                    return NotFound();
                }

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance log {id}");
                return StatusCode(500, "An error occurred while retrieving the maintenance log");
            }
        }
    }

    public class MaintenanceStatusDto
    {
        [Required]
        public MaintenanceLog.MaintenanceStatus Status { get; set; }
    }
}