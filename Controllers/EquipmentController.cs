using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        //get

        
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
                var equipmentList = await _context.Equipments.ToListAsync();


            if (equipmentList is not null)
            {
                return Ok(equipmentList);
            }
            return BadRequest();
        }
    }
}
