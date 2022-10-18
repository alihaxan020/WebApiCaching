using Microsoft.AspNetCore.Mvc;
using CachingWebApi.Services;
using CachingWebApi.Data;
using CachingWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CachingWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly ILogger<DriverController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _context;
        public DriverController(ILogger<DriverController> logger, ICacheService cacheService, AppDbContext context)
        {
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
        }
        [HttpGet("drivers")]
        public async Task<IActionResult> Get()
        {
            // check cache data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if (cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }
            cacheData = await _context.Drivers.ToListAsync();
            // Set expiry time
            var expiryTime = DateTimeOffset.Now.AddMinutes(5);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);
            return Ok(cacheData);
        }
        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post(Driver value)
        {
            var addedObj = await _context.Drivers.AddAsync(value);
            // Set expiry time
            var expiryTime = DateTimeOffset.Now.AddMinutes(5);
            _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity, expiryTime);
            await _context.SaveChangesAsync();
            return Ok(addedObj.Entity);
        }

        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = _context.Drivers.FirstOrDefault(x => x.Id == id);
            if (exist != null)
            {
                _context.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return NotFound();
        }

    }
}