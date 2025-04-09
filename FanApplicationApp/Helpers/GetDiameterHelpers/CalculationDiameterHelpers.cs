using System;
using SpeedCalc.Models;

namespace SpeedCalc.Helpers.GetDiameterHelpers;

public static class CalculationDiameterHelper
{
    private readonly static double Accuracy = 0.01;
    private readonly static double MaxDiameter = 3;
    public static double GetDiameter(List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        double polinomCoefficientI1 = GetPolinomCoefficientI1(datas, parameters);
        double polinomCoefficientI2 = GetPolinomCoefficientI2(datas, parameters);
        double polinomCoefficientI3 = GetPolinomCoefficientI3(datas, parameters);

        double calculationError = MaxDiameter;
        double diameterStart = MaxDiameter;
        double diameter = 0;

        while (calculationError > Accuracy)
        {
            double polinomPressure = GetPolinomValue(diameterStart, polinomCoefficientI3, parameters, polinomCoefficientI1,
                polinomCoefficientI2);
            double diffPolinomPressure = GetPolinomDiffValue(diameterStart, polinomCoefficientI3, parameters,
                polinomCoefficientI2);
            double diameterNextItt1 = GetNextIteration(diameterStart, polinomPressure, diffPolinomPressure);
            double polinomPressureNextItt = GetPolinomValue(diameterNextItt1, polinomCoefficientI3, parameters,
                polinomCoefficientI1,
                polinomCoefficientI2);
            double diffPolinomPressureNextItt = GetPolinomDiffValue(diameterNextItt1, polinomCoefficientI3, parameters,
                polinomCoefficientI2);
            double diameterNextItt2 = GetNextIteration(diameterNextItt1, polinomPressureNextItt, diffPolinomPressureNextItt);
            calculationError = GetCalculationError(diameterNextItt2, diameterNextItt1, diameterStart);
            diameterStart = diameterNextItt1;
            diameter = diameterNextItt2;
        }

        return diameter;
    }
    public static double GetSpeed(CalculationParameters parameters)
    {
        double PressureReserve = 1.05;
        double DensityNormal = 1.204;
        double AccelerationOfGravity = 9.81;
        double SecondsInHour = 3600;
        double flowRateRequired = parameters.FlowRateRequired;

        if (parameters.SuctionType == 1)
        {
            flowRateRequired = flowRateRequired / 2;
        }

        double speed = Math.Pow(flowRateRequired / SecondsInHour, 0.5) / Math.Pow(parameters.SystemResistance *
            DensityNormal * PressureReserve / parameters.Density / AccelerationOfGravity, 0.75) * parameters.Rpm;
        return speed;
    }

    private static double GetPolinomValue(double diameter, double polinomCoefficientI3, CalculationParameters parameters,
        double polinomCoefficientI1, double polinomCoefficientI2)
    {
        double polinomPressure = polinomCoefficientI1 + polinomCoefficientI2 * Math.Pow(diameter, 3) + polinomCoefficientI3 *
            Math.Pow(diameter, 6) - parameters.SystemResistance * Math.Pow(diameter, 4);
        return polinomPressure;
    }

    private static double GetPolinomDiffValue(double diameter, double polinomCoefficientI3, CalculationParameters parameters,
        double polinomCoefficientI2)
    {
        double diffPolinomPressure = 3 * polinomCoefficientI2 * Math.Pow(diameter, 2) + polinomCoefficientI3 * 6 *
            Math.Pow(diameter, 5) - parameters.SystemResistance * 4 * Math.Pow(diameter, 3);
        return diffPolinomPressure;
    }

    private static double GetNextIteration(double diameter, double polinomPressure, double diffPolinomPressure)
    {
        double diametrNextItt = diameter - polinomPressure / diffPolinomPressure;
        return diametrNextItt;
    }

    private static double GetCalculationError(double diameterNextItt2, double diameterNextItt1, double diameterStart)
    {
        double calculationError = Math.Abs((diameterNextItt2 - diameterNextItt1) / (1 - (diameterNextItt2 - diameterNextItt1) /
            (diameterNextItt1 - diameterStart)));
        return calculationError;
    }

    private static double GetPolinomCoefficientI2(List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var flowRateRequired = parameters.FlowRateRequired;

        if (parameters.SuctionType == 1)
        {
            flowRateRequired = flowRateRequired / 2;
        }

        double Coefficient = 108000;
        double polinomCoefficientI2 = aerodynamicRow.StaticPressure2 * parameters.Density * parameters.Rpm *
            flowRateRequired / Coefficient;
        return polinomCoefficientI2;
    }

    private static double GetPolinomCoefficientI1(List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
        var flowRateRequired = parameters.FlowRateRequired;

        if (parameters.SuctionType == 1)
        {
            flowRateRequired = flowRateRequired / 2;
        }

        double Coefficient = 1620000;
        double polinomCoefficientI1 = aerodynamicRow.StaticPressure1 * parameters.Density * Math.Pow(flowRateRequired, 2)
            / (double.Pi * double.Pi * Coefficient);
        return polinomCoefficientI1;
    }

    private static double GetPolinomCoefficientI3(List<AerodynamicsData> datas, CalculationParameters parameters)
    {
        var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);

        double Coefficient = 7200;
        double polinomCoefficientI3 = aerodynamicRow.StaticPressure3 * parameters.Density * parameters.Rpm * parameters.Rpm *
            double.Pi * double.Pi / Coefficient;
        return polinomCoefficientI3;
    }
}
