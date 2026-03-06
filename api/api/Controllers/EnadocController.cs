using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v3")]
    [ApiController]
    public class EnadocController : ControllerBase
    {
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token()
        {
            var json = System.IO.File.ReadAllText("enadocauthtoken.json");
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }

        [HttpPost]
        [Route("documents")]
        public async Task<IActionResult> Documents()
        {
            var json = System.IO.File.ReadAllText("enadocdocuments.json");
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
    }
}
