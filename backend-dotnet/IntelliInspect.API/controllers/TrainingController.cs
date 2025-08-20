using IntelliInspect.API.Models;
using IntelliInspect.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelliInspect.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingController : ControllerBase
    {
        private readonly TrainingService _trainingService;

        public TrainingController(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpPost("train-model")]
        public async Task<ActionResult<TrainModelResponse>> TrainModel([FromBody] TrainModelRequest request)
        {
            var response = await _trainingService.TrainAsync(request);

            if (response.Status == "Success")
                return Ok(response);

            return BadRequest(response);
        }
    }
}
