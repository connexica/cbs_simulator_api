using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/document")]
    [ApiController]
    public class NidController : ControllerBase
    {
        // Toggle to true to simulate a NO HIT response from /verification/result
        private const bool SimulateNoHit = false;

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
            if (SimulateNoHit)
            {
                return Ok(new
                {
                    error = false,
                    status = "NO HIT",
                    message = "Not matched",
                    data = new
                    {
                        client_request_id = "MOCK-PROCESS-001",
                        requested_at = "2026-06-16T10:00:00",
                        responded_at = "2026-06-16T10:00:10"
                    }
                });
            }

            var json = System.IO.File.ReadAllText("NidVerificationResult.json");
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
    }
}
