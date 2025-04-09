using FanApplicationApp.GetDiameterHelpers;
using FanApplicationApp.Models;

namespace FanApplicationApp.GetMomentOfInertciaHelper;

public static class CalculationMomentOfInertciaHelper
{
    private readonly static double Stock = 1.1;
    private readonly static double StockOfHub = 1.1;
    public static double GetMomentOfInertcia (List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);

        var bladeLength = aerodynamicRow.BladeLength;
        var bladeWidth = aerodynamicRow.BladeWidth;
        var numberOfBlades = aerodynamicRow.NumberOfBlades;
        var impellerWidth = aerodynamicRow.ImpellerWidth;
        var impellerInletDiameter = aerodynamicRow.ImpellerInletDiameter;

        double Coefficient = 8;

        double bladeThickness;
        double coverDiskThickness;
        double mainDiskThickness;

        GetThicknesses(diameter, out bladeThickness, out mainDiskThickness, out coverDiskThickness, parameters);

        double momentOfInertcia = ((GetWeightOfMainDisk(diameter,
        mainDiskThickness, parameters) * Math.Pow(diameter, 2) / Coefficient * StockOfHub) + (((GetWeightOfBlade( 
        bladeLength, bladeWidth, diameter, bladeThickness, parameters) * numberOfBlades) + GetWeightOfCoverDisk( 
        diameter, impellerWidth, coverDiskThickness, impellerInletDiameter, parameters)) * (Math.Pow(diameter, 2) + 
        Math.Pow((impellerInletDiameter * diameter), 2)) / Coefficient)) * Stock;

        if (parameters.SuctionType == 1)
        {
            momentOfInertcia = momentOfInertcia * 2;
        }
        return momentOfInertcia;
    }
    public static double GetWeightOfImpeller (double bladeLength, double bladeWidth, double diameter,
        double bladeThickness, int numberOfBlades, double mainDiskThickness, double impellerWidth,
        double coverDiskThickness, double impellerInletDiameter, CalculationParameters parameters)
    {
        double weightOfImpeller = ((GetWeightOfBlade(bladeLength, bladeWidth, diameter,
        bladeThickness, parameters) * numberOfBlades) + (GetWeightOfMainDisk(diameter,
        mainDiskThickness, parameters) * StockOfHub) + GetWeightOfCoverDisk(diameter, impellerWidth,
        coverDiskThickness, impellerInletDiameter, parameters)) * Stock;

        return weightOfImpeller;
    }
    private static double GetWeightOfBlade (double bladeLength, double bladeWidth, double diameter, 
        double bladeThickness, CalculationParameters parameters)
    {
        double weightOfBlade = parameters.MaterialDensyti * bladeLength * bladeWidth * Math.Pow(diameter, 2) * bladeThickness;

        return weightOfBlade;
    }
    private static double GetWeightOfMainDisk(double diameter,
        double mainDiskThickness, CalculationParameters parameters)
    {
        int Coefficient = 4;
        
        double weightOfMainDisk = parameters.MaterialDensyti * double.Pi * Math.Pow(diameter, 2) * mainDiskThickness / Coefficient;

        return weightOfMainDisk;
    }
    private static double GetWeightOfCoverDisk(double diameter, double impellerWidth,
        double coverDiskThickness, double impellerInletDiameter, CalculationParameters parameters)
    {
        int Coefficient = 2;
        double coverDiskGeneratrix = GetCoverDiskGeneratrix(impellerWidth, diameter, impellerInletDiameter);

        double weightOfCoverDisk = parameters.MaterialDensyti * double.Pi * (diameter + impellerInletDiameter) *
            coverDiskGeneratrix * coverDiskThickness / Coefficient;

        return weightOfCoverDisk;
    }
    private static double GetCoverDiskGeneratrix (double impellerWidth, double diameter, double impellerInletDiameter)
    {
        double coverDiskGeneratrix = Math.Pow((Math.Pow(impellerWidth, 2) + Math.Pow((diameter * (1 - impellerInletDiameter)),2)),
            0.5);

        return coverDiskGeneratrix;
    }
    private static void GetThicknesses(
        double diameter,
        out double bladeThickness,
        out double mainDiskThickness,
        out double coverDiskThickness,
        CalculationParameters parameters)
    {
        bladeThickness = 0;
        mainDiskThickness = 0;
        coverDiskThickness = 0;

        if (diameter <= 1.00 && parameters.Rpm <= 1500)
        {
            bladeThickness = 0.003;
            mainDiskThickness = 0.005;
            coverDiskThickness = 0.003;
        }
        if (1.00 < diameter && diameter <= 1.50 && parameters.Rpm <= 1500)
        {
            bladeThickness = 0.005;
            mainDiskThickness = 0.008;
            coverDiskThickness = 0.005;
        }
        if (1.00 < diameter && diameter <= 1.50 && parameters.Rpm > 1500)
        {
            bladeThickness = 0.01;
            mainDiskThickness = 0.014;
            coverDiskThickness = 0.01;
        }
        if (1.50 < diameter && diameter <= 2.00 && 750 < parameters.Rpm && parameters.Rpm <= 1500)
        {
            bladeThickness = 0.008;
            mainDiskThickness = 0.01;
            coverDiskThickness = 0.08;
        }
        if (2.00 < diameter && parameters.Rpm <= 1500)
        {
            bladeThickness = 0.012;
            mainDiskThickness = 0.02;
            coverDiskThickness = 0.012;
        }
        if (diameter < 1.00 && parameters.Rpm > 1500)
        {
            bladeThickness = 0.005;
            mainDiskThickness = 0.008;
            coverDiskThickness = 0.005;
        }
        if (1.50 < diameter && diameter <= 2.00 && parameters.Rpm > 1500)
        {
            bladeThickness = 0.012;
            mainDiskThickness = 0.02;
            coverDiskThickness = 0.012;
        }
        if (1.50 < diameter && diameter <= 2.00 && parameters.Rpm <= 750)
        {
            bladeThickness = 0.005;
            mainDiskThickness = 0.005;
            coverDiskThickness = 0.005;
        }

    }
}
