using ScottPlot;
using SpeedCalc.Models;
using SpeedCalc.Helpers.GetDiameterHelpers;

namespace SpeedCalc.Helpers.PdfHelpers;
public class PaintDiagramsHelper
{
    public static Plot? GenerateAerodynamicPlot(List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        int pointsCount = 100;

        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);

        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        var (flowRates, staticPressures) = CalculationDiagramHelper.GetStaticPressureMassive(pointsCount, parameters, datas);

        var (_, totalPressures) = CalculationDiagramHelper.GetTotalPressureMassive(pointsCount, parameters, datas);

        var (_, powers) = CalculationDiagramHelper.GetPowerMassive(pointsCount, parameters, datas);
        var (flowRates1, pressureResistances) = CalculationDiagramHelper.GetPressureResistanceMassive(
            pointsCount, parameters);

        var totalPressureWorkPoint = CalculationDiagramHelper.GetPolinomTotalPressure(parameters.FlowRateRequired,
        datas, parameters);

        var totalEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomEeficiency(parameters.FlowRateRequired,
        datas, parameters);

        var powerWorkPoint = CalculationDiagramHelper.GetPolinomPower(parameters.FlowRateRequired, datas, parameters);

        var staticEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomStaticEficiency(parameters.FlowRateRequired,
          datas, parameters);

        var markImpeller = aerodynamicRow.NewMarkOfFan;
        if (parameters.SuctionType == 1)
        {
            markImpeller = aerodynamicRow.NewMarkOfFanD;
        }
        string? typeIsp = null;
        switch (parameters.MaterialDesign)
        {
            case 1: typeIsp = "К1"; break;
            case 2: typeIsp = "В"; break;
            case 3: typeIsp = "ВК1"; break;
            case 4: typeIsp = "Ti"; break;
            case 5: typeIsp = null; break;
        }


        var aerodynamicPlot = new Plot();
        aerodynamicPlot.Title($"ТДМ {markImpeller}-{diameter * 10:F1}{typeIsp} {parameters.Density} кг/м3 {parameters.Rpm} об/мин");
        aerodynamicPlot.XLabel("Расход воздуха, м³/ч");
        aerodynamicPlot.Axes.Left.Label.Text = "Давление, Па";
        aerodynamicPlot.Axes.Right.Label.Text = "Мощность, кВт";

        var staticPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, staticPressures, Colors.Grey);
        staticPressurePlot.LegendText = "Статическое давление";

        var totalPressurePlot = aerodynamicPlot.Add.Scatter(flowRates, totalPressures, Colors.Black);
        totalPressurePlot.LegendText = "Полное давление";

        var resistancePlot = aerodynamicPlot.Add.Scatter(flowRates1, pressureResistances, Colors.Blue);
        resistancePlot.LegendText = "Характеристика сети";

        var powerPlot = aerodynamicPlot.Add.Scatter(flowRates, powers, Colors.Orange);
        powerPlot.LegendText = "Потребляемая мощность";
        powerPlot.Axes.YAxis = aerodynamicPlot.Axes.Right;

        AddWorkPointMarker(aerodynamicPlot, parameters.FlowRateRequired, parameters.SystemResistance,
            $"Q = {parameters.FlowRateRequired:F1} м³/ч\nPs = {parameters.SystemResistance:F1} Па\n" +
            $"КПДs = {staticEficiencyWorkPoint:F2}\nМощность = {powerWorkPoint:F1} кВт",
            "Статическое давление требуемое", 0, 0, Alignment.UpperLeft);

        AddWorkPointMarker(aerodynamicPlot, parameters.FlowRateRequired, totalPressureWorkPoint,
            $"Q = {parameters.FlowRateRequired:F1} м³/ч\nPv = {totalPressureWorkPoint:F1} Па\n" +
            $"КПД = {totalEficiencyWorkPoint:F2}\nМощность = {powerWorkPoint:F1} кВт",
            "Полное давление", 0, 1000000, Alignment.MiddleLeft);

        aerodynamicPlot.ShowLegend();
        aerodynamicPlot.Legend.Alignment = Alignment.LowerRight;

        return aerodynamicPlot;
    }
    public static Plot? GenerateTorquePlot(List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        int pointsCount = 100;

        var (rpmValues, nominalTorques) = CalculationDiagramHelper.GetNominalTorqueMassive(pointsCount, datas, parameters);

        var (_, torqueWithGates) = CalculationDiagramHelper.GetTorqueWithGateMassive(pointsCount, datas, parameters);

        var torquePlot = new Plot();
        torquePlot.Title($"Нагрузочная характеристика электродвигателя");
        torquePlot.XLabel("Обороты рабочего колеса, об/мин");
        torquePlot.Axes.Left.Label.Text = "Момент силы, кН*м";

        var staticPressurePlot = torquePlot.Add.Scatter(rpmValues, nominalTorques, Colors.Grey);
        staticPressurePlot.LegendText = "Момент при открытой заслонке";

        var totalPressurePlot = torquePlot.Add.Scatter(rpmValues, torqueWithGates, Colors.Black);
        totalPressurePlot.LegendText = "Момент при закрытой заслонке на входе";

        torquePlot.ShowLegend();
        torquePlot.Legend.Alignment = Alignment.LowerRight;

        return torquePlot;
    }
    private static void AddWorkPointMarker(Plot aerodynamicPlot, double x, double y, string annotationText, string legendText,
        int offSetX, int offSetY, Alignment alignment)
    {
        var marker = aerodynamicPlot.Add.Marker(x, y, MarkerShape.FilledCircle, 15, Colors.Red);
        marker.LegendText = legendText;

        var annotation = aerodynamicPlot.Add.Annotation(annotationText);
        annotation.Alignment = alignment;
        annotation.OffsetX = offSetX;
        annotation.OffsetY = offSetY;
        annotation.LabelFontColor = Colors.Red;
        annotation.LabelBorderColor = Colors.LightGray;
        annotation.LabelBorderWidth = 1;
    }
}




