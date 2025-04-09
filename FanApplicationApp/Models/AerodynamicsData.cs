namespace SpeedCalc.Models;

public class AerodynamicsData
{
    public Guid Id { get; set; }
    public AerodynamicsType Type { get; set; }
    public string? Name { get; set; }
    public double StaticPressure1 { get; set; }
    public double StaticPressure2 { get; set; }
    public double StaticPressure3 { get; set; }
    public double Efficiency1 { get; set; }
    public double Efficiency2 { get; set; }
    public double Efficiency3 { get; set; }
    public double Efficiency4 { get; set; }
    public double MinSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double OutletLength { get; set; }
    public double OutletWidth { get; set; }
    public double MinDeltaEfficiency { get; set; }
    public double MaxDeltaEfficiency { get; set; }
    public string? NewMarkOfFan { get; set; }
    public double BladeLength { get; set; }
    public double BladeWidth { get; set; }
    public double ImpellerWidth { get; set; }
    public double ImpellerInletDiameter { get; set; }
    public int NumberOfBlades { get; set; }
    public string? TypeOfBlades { get; set; }
    public string? NewMarkOfFanD { get; set; }
}

