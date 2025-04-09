using SpeedCalc.Models;

namespace SpeedCalc.Services;

public interface IAerodynamicService
{
    public Task DownloadFileAsync(CalculationParameters parameters);
}
