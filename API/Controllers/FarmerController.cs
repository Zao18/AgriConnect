using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmersController : ControllerBase
    {
        private readonly IFarmerService _service;

        public FarmersController(IFarmerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var farmer = await _service.GetByIdAsync(id);
            return farmer == null ? NotFound() : Ok(farmer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FarmerEntity farmer)
        {
            await _service.AddAsync(farmer);
            return CreatedAtAction(nameof(GetById), new { id = farmer.RowKey }, farmer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] FarmerEntity farmer)
        {
            await _service.UpdateAsync(id, farmer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
