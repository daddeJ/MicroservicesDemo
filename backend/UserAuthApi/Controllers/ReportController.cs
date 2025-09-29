using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpGet("admin")]
    public IActionResult AdminReport()
    {
        return Ok("Access Granted: Executives only");
    }
    
    [Authorize(Policy = "ExecutivesOnly")]
    [HttpGet("executives")]
    public IActionResult ExecutiveReport()
    {
        return Ok("Access Granted: Executives only");
    }
    
    [Authorize(Policy = "ManagerAndAbove")]
    [HttpGet("manager")]
    public IActionResult ManagerReport()
    {
        return Ok("Access Granted: Managers and above");
    }
    
    [Authorize(Policy = "LeaderAndAbove")]
    [HttpGet("leader")]
    public IActionResult TeamReport()
    {
        return Ok("Access Granted: Leader and above");
    }
    
    [Authorize(Policy = "RegularAndAbove")]
    [HttpGet("general")]
    public IActionResult GeneralReport()
    {
        return Ok("Access granted: All employees except superadmin");
    }
}