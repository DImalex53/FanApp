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
            RotaitionDirection = "правое",
            ExhaustDirection = 90,
            Vibroisolation = 0,
            GuideVane = 1,
            Teploisolation = 1,
            MaterialDesign = 5,
            MaterialOfImpeller = "10ХСНД",
            MaterialOfUlita = "09Г2С",
            MaterialOfBoltsOfUlita = "углеродистая сталь",
            MaterialOfRama = "09Г2С",
            MuftType = 2,
            TypeOfPPO = 4,
            KLimatic = "У1",
            DopTrebovaniyaMotor = "датчики температуры обмоток статора",
            ShaftSeal = 2,
            TypeOfCompensatorInlet = "6",
            TypeOfCompensatorOutlet = "6",
            FlangeOutlet = 1,
            FlangeInlet = 1,
            VibroSensorPPO = "DVA 141.214.E3Х1 исп.02 М10х1.25 (НПП ТИК) (4-20 мА) 2 к-кта",
            TempSensorPPO = "ДТС-044-Pt100.B3,30/5 2 к-кта",
            VibroSensorMotor = null,
            MarkOfVzrivMotor = "1ExdIIСT4",
            NalichieVFD = 1,
            DopKomplekt = "МЭО-1600/63-025У-92К У1 380В не ниже IP 54\nПереход на круглое сечение Dn=2500 мм на линии НАГНЕТАНИЕ - 1 шт\n" +
            "Комплект анкерных болтов\n" +
            "Шкаф управления на базе ПЧ\nПереход на круглое сечение Dn=2500 мм на линии ВХОД - 2 шт\nКлеммная коробка для вывода сигналов датчиков\n" +
            "Всасывающие карманы (разворот - 90) - 2шт",
            DopTrebovanyaTDM = "Корпус улиты оснащен съемным сегментом\nНа поверхности улиты и карманов предусмотрены люки\nКорпус улиты оснащен дренажом\n" +
            "Корректированный уровень звуквого давления на расстоянии 1 м от ТДМ не более 80 дБА\nПредельная температура перемещаемой среды 80С" +
            "\nГПР согласно требований заказчика\nТДМ поставляется в разборном виде\nВсе части ТДМ окрашены до уровня C3L\n" +
            "Все фланцевые соединения уплотнены герметиком",
            ProjectName = "Строительство системы термического дожига CO.\nДымосос МГОУ на основе ОЛ 02-ЕНТМК-0324-ПД-ТХ.01.1-ОЛ-0001\n" +
            "в редакции АО Евраз НТМК",
            Zip = $"- крепеж с запасом 20% - 1 к-кт\n- комплект монтажных прокладочных пластин - 1 к-кт\n" +
            $"- герметик для уплотнения фланцев - 1 к-кт\n- сопроводительная документация - 1 к-кт",
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
                                Title = $"Отчет о подборе по задаче {parameters.NumberOfTask}",
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