using SpeedCalc.Helpers.PdfHelpers;
using SpeedCalc.Models;
using SpeedCalc.Repositories;

namespace SpeedCalc.Services
{
    public class AerodynamicService(IAerodynamicsDataRepository aerodynamicsDataRepository) : IAerodynamicService
    {
        private readonly IAerodynamicsDataRepository _aerodynamicsDataRepository = aerodynamicsDataRepository;

        public async Task DownloadFileAsync(CalculationParameters parameters)
        {
            var allData = (await _aerodynamicsDataRepository.GetAllAsync()).ToList();

            var aerodynamicPlot = PaintDiagramsHelper.GenerateAerodynamicPlot(allData, parameters);
            var torquePlot = PaintDiagramsHelper.GenerateTorquePlot(allData, parameters);

            if (aerodynamicPlot == null)
            {
                return;
            }

            string reportPath = Path.Combine("wwwroot", "reports", "report.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

            PdfExporter.ExportToPdf(
                allData.ToList(),
                aerodynamicPlot,
                torquePlot,
                reportPath,
                parameters,
                new PdfExportOptions
                {
                    Title = $"Отчет о подборе по задаче {parameters.NumberOfTask}",
                    Orientation = PdfSharp.PageOrientation.Landscape,
                    FontFamily = "Times New Roman"
                });
        }
    }
}
