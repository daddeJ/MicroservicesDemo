using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    [Authorize(Policy = "ExecutivesOnly")]
    [HttpGet("financial")]
    public IActionResult FinancialReport()
    {
        return Ok("Access Granted: Executives only");
    }
    
    [Authorize(Policy = "ManagerAndAbove")]
    [HttpGet("team")]
    public IActionResult TeamReport()
    {
        return Ok("Access Granted: Managers and above");
    }
    
    [Authorize(Policy = "RegularAndAbove")]
    [HttpGet("general")]
    public IActionResult GeneralReport()
    {
        return Ok("Access granted: All employees except superadmin");
    }
}