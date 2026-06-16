using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/document")]
    [ApiController]
    public class NidController : ControllerBase
    {
        [HttpPost]
        [Route("verification")]
        public async Task<IActionResult> Verification()
        {
            var json = System.IO.File.ReadAllText("NidVerification.json");
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }

        [HttpPost]
        [Route("verification/result")]
        public async Task<IActionResult> VerificationResult()
        {
            var json = System.IO.File.ReadAllText("NidVerificationResult.json");
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
    }
}
