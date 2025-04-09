namespace FanApplicationApp.Models;

public class CalculationParameters
{
    public double MotorVoltage { get; set; } = 380;
    public double FlowRateRequired { get; set; } = 100000;
    public double SystemResistance { get; set; } = 6000;
    public double Density { get; set; } = 1.204;
    public int Rpm { get; set; } = 990;
    public int Type { get; set; } = 2;
    public double MaterialDensyti { get; set; } = 8000;
    public int SuctionType { get; set; } = 0;
    public int? NumberOfTask { get; set; } = null;
    public int? ConstructScheme { get; set; } = null;
    public string? RotaitionDirection { get; set; } = null;
    public int? ExhaustDirection { get; set; } = null;
    public int Vibroisolation { get; set; } = 0;
    public int GuideVane { get; set; } = 0;
    public int Teploisolation { get; set; } = 0;
    public int MaterialDesign { get; set; } = 5;
    public string MaterialOfImpeller { get; set; } = "09Г2С";
    public string MaterialOfUlita { get; set; } = "09Г2С";
    public string? MaterialOfBoltsOfUlita { get; set; } = null;
    public string MaterialOfRama { get; set; } = "09Г2С";
    public int MuftType { get; set; } = 1;
    public int TypeOfPPO { get; set; } = 1;
    public string KLimatic { get; set; } = "У1";
    public string? DopTrebovaniyaMotor { get; set; } = null;
    public int ShaftSeal { get; set; } = 1;
    public string? TypeOfCompensatorInlet { get; set; } = null;
    public string? TypeOfCompensatorOutlet { get; set; } = null;
    public int FlangeOutlet { get; set; } = 0;
    public int FlangeInlet { get; set; } = 0;
    public string? VibroSensorPPO { get; set; } = null;
    public string? TempSensorPPO { get; set; } = null;
    public string? VibroSensorMotor { get; set; } = null;
    public string? MarkOfVzrivMotor { get; set; } = null;
    public int NalichieVFD { get; set; } = 0;
    public string ProjectName { get; set; } = "No name project";
    public string? DopKomplekt { get; set; } = null;
    public string? DopTrebovanyaTDM { get; set; } = null;
    public string? Zip { get; set; }
    public int ShefMontage { get; set; } = 0;
    public int PuskoNaladka { get; set; } = 0;
    public int StudyOfPersonal { get; set; } = 0;
}