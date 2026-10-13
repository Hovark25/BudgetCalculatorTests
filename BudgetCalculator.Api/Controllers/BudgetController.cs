using BudgetCalculator.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetCalculator.Api.Controllers;

[ApiController]
[Route("api/budget")]
public class BudgetController : ControllerBase
{
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] BudgetRequest request)
    {
        if (request.Days <= 0)
        {
            return BadRequest("Количество дней должно быть больше нуля");
        }

        var total = request.DailyCost * request.Days;

        if (request.IncludeInsurance)
        {
            total *= 1.05m;
        }

        total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

        return Ok(new { TotalAmount = total });
    }
}
