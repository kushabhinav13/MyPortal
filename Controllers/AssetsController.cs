using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Myportal.Models;
using Myportal.Data;
using System.ComponentModel.DataAnnotations;

namespace Myportal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(InventoryDbContext context, ILogger<AssetsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Assets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets(
            [FromQuery] Asset.AssetStatus? status = null)
        {
            try
            {
                IQueryable<Asset> query = _context.Assets;

                if (status.HasValue)
                {
                    query = query.Where(a => a.Status == status.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assets");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        // GET: api/Assets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asset>> GetAsset(int id)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(id);

                if (asset == null)
                {
                    return NotFound();
                }

                return asset;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting asset with id {id}");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        // PUT: api/Assets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsset(int id, Asset asset)
        {
            if (id != asset.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!Enum.IsDefined(typeof(Asset.AssetStatus), asset.Status))
            {
                return BadRequest("Invalid status value");
            }

            _context.Entry(asset).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!AssetExists(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, $"Concurrency error updating asset with id {id}");
                return StatusCode(500, "A concurrency error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating asset with id {id}");
                return StatusCode(500, "An error occurred while updating the asset");
            }
        }

        // POST: api/Assets
        [HttpPost]
        public async Task<ActionResult<Asset>> PostAsset(Asset asset)
        {
            if (!Enum.IsDefined(typeof(Asset.AssetStatus), asset.Status))
            {
                return BadRequest("Invalid status value");
            }

            try
            {
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetAsset", new { id = asset.Id }, asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new asset");
                return StatusCode(500, "An error occurred while creating the asset");
            }
        }

        // DELETE: api/Assets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(id);
                if (asset == null)
                {
                    return NotFound();
                }

                // Prevent deletion if asset is assigned
                if (asset.Status == Asset.AssetStatus.Assigned)
                {
                    return BadRequest("Cannot delete an assigned asset");
                }

                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting asset with id {id}");
                return StatusCode(500, "An error occurred while deleting the asset");
            }
        }

        private bool AssetExists(int id)
        {
            return _context.Assets.Any(e => e.Id == id);
        }
    }
}