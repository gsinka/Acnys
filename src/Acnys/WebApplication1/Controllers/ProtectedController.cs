using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "admin")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok("Success");
        }
    }
}