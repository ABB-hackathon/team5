using IntelliInspect.API.Models;
using IntelliInspect.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelliInspect.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DateRangesController : ControllerBase
    {
        private readonly DateRangeService _dateRangeService;

        public DateRangesController(DateRangeService dateRangeService)
        {
            _dateRangeService = dateRangeService;
        }

        [HttpPost("validate")]
        public ActionResult<DateRangeResponse> ValidateRanges([FromBody] DateRangeRequest request)
        {
            var response = _dateRangeService.ValidateRanges(request);
            if (response.Status == "Valid")
                return Ok(response);

            return BadRequest(response);
        }
    }
}
