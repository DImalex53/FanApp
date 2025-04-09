using Microsoft.EntityFrameworkCore;
using FanApplicationApp.Models;
using FanApplicationApp.Repositories;
using FanApplicationApp.GetDiameterHelpers;
using FanApplicationApp.PdfHelpers;
using ScottPlot;
using System.Collections.Generic;
using Npgsql;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAerodynamicsDataRepository, AerodynamicsDataRepository>();
builder.Services.AddTransient<PaintDiagramsHelper>();

var app = builder.Build();

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Database initialization and report generation
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var repository = services.GetRequiredService<IAerodynamicsDataRepository>();

        // Apply migrations
        await context.Database.MigrateAsync();
        Console.WriteLine("Database migration completed successfully");

        // Get data
        var allData = await repository.GetAllAsync();

        if (!allData.Any())
        {
            Console.WriteLine("Warning: aerodynamics_data table is empty!");
        }

        // Calculation parameters
        var parameters = new CalculationParameters
        {
            FlowRateRequired = 340000,
            SystemResistance = 3000,
            MotorVoltage = 6000,
            Rpm = 740,
            Density = 1.29,
            Type = 3,
            SuctionType = 1,
            NumberOfTask = 66660,
            ConstructScheme = 6,
            RotaitionDirection = "������",
            ExhaustDirection = 90,
            Vibroisolation = 0,
            GuideVane = 1,
            Teploisolation = 1,
            MaterialDesign = 5,
            MaterialOfImpeller = "10����",
            MaterialOfUlita = "09�2�",
            MaterialOfBoltsOfUlita = "������������ �����",
            MaterialOfRama = "09�2�",
            MuftType = 2,
            TypeOfPPO = 4,
            KLimatic = "�1",
            DopTrebovaniyaMotor = "������� ����������� ������� �������",
            ShaftSeal = 2,
            TypeOfCompensatorInlet = "6",
            TypeOfCompensatorOutlet = "6",
            FlangeOutlet = 1,
            FlangeInlet = 1,
            VibroSensorPPO = "DVA 141.214.E3�1 ���.02 �10�1.25 (��� ���) (4-20 ��) 2 �-���",
            TempSensorPPO = "���-044-Pt100.B3,30/5 2 �-���",
            VibroSensorMotor = null,
            MarkOfVzrivMotor = "1ExdII�T4",
            NalichieVFD = 1,
            DopKomplekt = "���-1600/63-025�-92� �1 380� �� ���� IP 54\n������� �� ������� ������� Dn=2500 �� �� ����� ���������� - 1 ��\n" +
            "�������� �������� ������\n" +
            "���� ���������� �� ���� ��\n������� �� ������� ������� Dn=2500 �� �� ����� ���� - 2 ��\n�������� ������� ��� ������ �������� ��������\n" +
            "����������� ������� (�������� - 90) - 2��",
            DopTrebovanyaTDM = "������ ����� ������� ������� ���������\n�� ����������� ����� � �������� ������������� ����\n������ ����� ������� ��������\n" +
            "���������������� ������� �������� �������� �� ���������� 1 � �� ��� �� ����� 80 ���\n���������� ����������� ������������ ����� 80�" +
            "\n��� �������� ���������� ���������\n��� ������������ � ��������� ����\n��� ����� ��� �������� �� ������ C3L\n" +
            "��� ��������� ���������� ��������� ����������",
            ProjectName = "������������� ������� ������������ ������ CO.\n������� ���� �� ������ �� 02-�����-0324-��-��.01.1-��-0001\n" +
            "� �������� �� ����� ����",
            Zip = $"- ������ � ������� 20% - 1 �-��\n- �������� ��������� ������������ ������� - 1 �-��\n" +
            $"- �������� ��� ���������� ������� - 1 �-��\n- ���������������� ������������ - 1 �-��",
            ShefMontage = 1,
            PuskoNaladka = 1,
            StudyOfPersonal = 0
        };

        // Calculate diameter
        double? diameter = null;
        try
        {
            if (allData.Any())
            {
                diameter = CalculationDiameterHelper.GetDiameter(allData.ToList(), parameters);
                Console.WriteLine($"Calculated diameter: {diameter}");
            }
            else
            {
                Console.WriteLine("Cannot calculate diameter - no data available");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Diameter calculation error: {ex.Message}");
        }

        // Generate plots and PDF report
        if (diameter.HasValue && allData.Any())
        {
            try
            {
                var aerodynamicPlot = PaintDiagramsHelper.GenerateAerodynamicPlot(allData.ToList(), parameters);
                var torquePlot = PaintDiagramsHelper.GenerateTorquePlot(allData.ToList(), parameters);

                if (aerodynamicPlot == null)
                {
                    Console.WriteLine("Failed to generate aerodynamic plot");
                }
                else
                {
                    try
                    {
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
                                Title = $"����� � ������� �� ������ {parameters.NumberOfTask}",
                                Orientation = PdfSharp.PageOrientation.Landscape,
                                FontFamily = "Times New Roman"
                            });

                        Console.WriteLine($"Report successfully generated: {reportPath}");
                    }
                    catch (Exception pdfEx)
                    {
                        Console.WriteLine($"PDF generation error: {pdfEx.Message}");
                    }
                }
            }
            catch (Exception plotEx)
            {
                Console.WriteLine($"Plot generation error: {plotEx.Message}");
                if (plotEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {plotEx.InnerException.Message}");
                }
            }
        }
    }

    await app.RunAsync();
}
catch (Exception globalEx)
{
    Console.WriteLine($"Fatal application error: {globalEx.Message}");
    if (globalEx.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {globalEx.InnerException.Message}");
    }
    return -1; // Exit with error code
}

return 0;