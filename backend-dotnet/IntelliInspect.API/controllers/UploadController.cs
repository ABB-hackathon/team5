using Microsoft.AspNetCore.Mvc;
using IntelliInspect.API.Services;
using IntelliInspect.API.Models;

namespace IntelliInspect.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly DatasetService _datasetService;

        public UploadController()
        {
            _datasetService = new DatasetService();
        }

        [HttpPost]
        public IActionResult Upload([FromForm] IFormFile file)
        {
            if (file == null || Path.GetExtension(file.FileName).ToLower() != ".csv")
                return BadRequest("Only CSV files are allowed.");

            using var stream = file.OpenReadStream();
            try
            {
                DatasetMetadata metadata = _datasetService.ProcessCsv(stream, file.FileName);
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
