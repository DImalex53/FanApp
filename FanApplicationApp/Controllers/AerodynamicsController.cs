using Microsoft.AspNetCore.Mvc;
using SpeedCalc.Models;
using SpeedCalc.Services;

[ApiController]
[Route("api/[controller]")]
public class AerodynamicsController : ControllerBase
{
    private readonly IAerodynamicService _service;

    public AerodynamicsController(IAerodynamicService calculationService)
    {
        _service = calculationService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CalculationParameters parameters)
    {
        if (parameters == null)
            return BadRequest("Параметры не могут быть пустыми");

        if (parameters.FlowRateRequired <= 0)
            return BadRequest("Расход должен быть положительным числом");

        await _service.DownloadFileAsync(parameters);

        return Ok();
    }
}