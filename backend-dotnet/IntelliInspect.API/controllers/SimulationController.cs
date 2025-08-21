using IntelliInspect.API.Models;
using IntelliInspect.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelliInspect.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly SimulationService _service;

        public SimulationController(SimulationService service)
        {
            _service = service;
        }

        [HttpPost("run")]
        public async Task<ActionResult<SimulationResponse>> Run([FromBody] SimulationRequest request)
        {
            var response = await _service.RunAsync(request);
            if (response.Status == "Success") return Ok(response);
            return BadRequest(response);
        }
    }
}



