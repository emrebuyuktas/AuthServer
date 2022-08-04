using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniApp3.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ClientCtroller : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(200);
    }
}