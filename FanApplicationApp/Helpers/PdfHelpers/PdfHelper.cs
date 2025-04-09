using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using System.Diagnostics;
using SpeedCalc.Models;
using ScottPlot;
using SpeedCalc.Models;
using SpeedCalc.Helpers.GetDiameterHelpers;
using SpeedCalc.Helpers.GetMomentOfInertciaHelper;

namespace SpeedCalc.Helpers.PdfHelpers;

public static class PdfExporter
{
    private static int _pageCounter = 0;
    private const string UpperLogoPath = @"wwwroot\logos\logoUp.png";
    private const string LowerLogoPath = @"wwwroot\logos\logoDown.jpg";

    static PdfExporter()
    {
        GlobalFontSettings.FontResolver = new FailsafeFontResolver();
    }

    private static void DrawHeader(XGraphics gfx, PdfPage page, PdfExportOptions options)
    {
        try
        {
            double pageWidth = page.Width.Point;
            double margin = 25;
            double headerHeight = 70;

            // Логотип (левая часть) - уменьшенный размер
            if (File.Exists(UpperLogoPath))
            {
                using (XImage logo = XImage.FromFile(UpperLogoPath))
                {
                    double logoHeight = headerHeight - 15;
                    double logoWidth = logoHeight * (logo.PixelWidth / (double)logo.PixelHeight);
                    // Ограничиваем ширину логотипа
                    if (logoWidth > pageWidth * 0.4)
                    {
                        logoWidth = pageWidth * 0.4;
                        logoHeight = logoWidth * (logo.PixelHeight / (double)logo.PixelWidth);
                    }
                    gfx.DrawImage(logo, margin, margin, logoWidth, logoHeight);
                }
            }

            // Контакты (правая часть) - 3 строки, выровнены по правому краю
            var contactFont = new XFont(options.FontFamily, 9);
            string[] contacts =
            {
        "193315, г. Санкт-Петербург,",
        "пр. Большевиков, д.52, корп.9",
        "Тел: 8 (812) 331-00-97 | Email: zv@zavodventilator.ru"
    };

            double contactsWidth = pageWidth * 0.5;
            double contactsX = pageWidth - contactsWidth - margin;

            for (int i = 0; i < contacts.Length; i++)
            {
                gfx.DrawString(contacts[i], contactFont, XBrushes.Black,
                    new XRect(contactsX, margin + i * 18, contactsWidth, 18),
                    XStringFormats.TopRight); // Выравнивание по правому краю
            }

            // Разделительная линия
            gfx.DrawLine(XPens.LightGray, margin, margin + headerHeight,
                        pageWidth - margin, margin + headerHeight);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в верхнем колонтитуле: {ex.Message}");
        }
    }

    private static void DrawFooter(XGraphics gfx, PdfPage page, PdfExportOptions options)
    {
        try
        {
            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;
            double margin = 25;
            double footerHeight = 80;

            // Разделительная линия вверху колонтитула
            double lineY = pageHeight - footerHeight;
            gfx.DrawLine(XPens.LightGray, margin, lineY, pageWidth - margin, lineY);

            // Логотип (верхняя часть колонтитула)
            if (File.Exists(LowerLogoPath))
            {
                using XImage footerLogo = XImage.FromFile(LowerLogoPath);
                double logoWidth = pageWidth * 0.75; // 75% ширины страницы
                double logoHeight = footerHeight * 0.4; // 40% высоты колонтитула

                // Сохраняем пропорции
                double aspectRatio = footerLogo.PixelWidth / (double)footerLogo.PixelHeight;
                logoHeight = Math.Min(logoHeight, logoWidth / aspectRatio);

                // Выравниваем по левому краю с отступом и поднимаем выше
                gfx.DrawImage(footerLogo,
                    margin,
                    lineY + 5, // Подняли лого выше
                    logoWidth,
                    logoHeight);
            }

            // Текст реквизитов (одна строка, растянутая по всей ширине)
            var footerFont = new XFont(options.FontFamily, 8);
            string footerText = "ООО «СЗЭМО ЗВТДМ» | ИНН: 7811513824 | ОГРН: 1127847079546 | КПП: 78110100 | " +
                              "р/с 40702810832310002312 в АО «АЛЬФА-БАНК» | БИК: 044030786";

            // Позиционируем текст ниже логотипа (опустили на 25px)
            double textY = lineY + 30;
            gfx.DrawString(footerText, footerFont, XBrushes.Black,
                new XRect(margin, textY, pageWidth - 2 * margin, 15),
                XStringFormats.TopCenter);

            // Номер страницы
            gfx.DrawString($"Страница {_pageCounter}", footerFont, XBrushes.Gray,
                new XRect(0, pageHeight - 15, pageWidth - 15, 15),
                XStringFormats.BottomRight);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в нижнем колонтитуле: {ex.Message}");
        }
    }

    public static void ExportToPdf(List<AerodynamicsData> datas,
        Plot aerodynamicPlot,
        Plot torquePlot,
        string pdfPath,
        CalculationParameters parameters,
        PdfExportOptions? options = null)
    {
        options ??= new PdfExportOptions();

        string tempAeroImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
        string tempTorqueImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

        try
        {
            // Сохраняем графики с явным указанием формата
            aerodynamicPlot.Save(tempAeroImagePath, 800, 600);
            torquePlot.Save(tempTorqueImagePath, 800, 600);

            if (!File.Exists(tempAeroImagePath) || !File.Exists(tempTorqueImagePath))
            {
                throw new Exception("Не удалось сохранить временные файлы графиков");
            }

            using var document = new PdfDocument();

            _pageCounter = 0;

            // 1. Титульная страница (с колонтитулами)
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;
                var projectName = parameters.ProjectName;

                // Шрифты
                var titleFont = new XFont(options.FontFamily, 24, XFontStyleEx.Bold);
                var titleFont1 = new XFont(options.FontFamily, 10, XFontStyleEx.Bold);
                var dateFont = new XFont(options.FontFamily, 12);
                var footerFont = new XFont(options.FontFamily, 10, XFontStyleEx.Italic);

                // Основной заголовок
                gfx.DrawString("Техническое предложение", titleFont, XBrushes.Black,
                    new XRect(0, 120, pageWidth, 50), XStringFormats.TopCenter);

                gfx.DrawString("на Тягодутьевую машину", titleFont, XBrushes.Black,
                    new XRect(0, 170, pageWidth, 50), XStringFormats.TopCenter);

                // Наименование проекта
                string[] lines = projectName.Split('\n');
                float lineHeight = 20;
                float startY = 220;

                gfx.DrawString("Наименование проекта:", titleFont1, XBrushes.Black,
                    new XRect(0, startY, pageWidth, lineHeight), XStringFormats.TopCenter);

                for (int i = 0; i < lines.Length; i++)
                {
                    gfx.DrawString(lines[i], titleFont1, XBrushes.Black,
                        new XRect(0, startY + (i + 1) * lineHeight, pageWidth, lineHeight),
                        XStringFormats.TopCenter);
                }

                // Текст о заводе
                string[] footerLines =
                {
                        "Завод ООО «СЗЭМО ЗВ ТДМ» является одним из лидеров",
                        "по производству тягодутьевых машин в России",
                        "г. Санкт-Петербург"
                };

                for (int i = 0; i < footerLines.Length; i++)
                {
                    gfx.DrawString(footerLines[i], footerFont, XBrushes.Black,
                        new XRect(0, pageHeight - 150 + i * 20, pageWidth, 20),
                        XStringFormats.TopCenter);
                }

                // Дата
                gfx.DrawString($"Дата: {DateTime.Now:dd.MM.yyyy}", dateFont, XBrushes.Black,
                    new XRect(0, pageHeight - 80, pageWidth, 20),
                    XStringFormats.BottomCenter);
            });

            // 2. Технические характеристики
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;

                var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                gfx.DrawString("Общие технические характеристики", titleFont, XBrushes.Black,
                    new XRect(0, 90, pageWidth, 30), XStringFormats.TopCenter);

                string[] specifications =
                {
                        "1. Расчет и подбор ТДМ осуществлялся в соответствии с требованиями ТЗ заказчика,",
                        "стандартов и конструкторской документации на конкретный вид оборудования,",
                        "технического регламента «О безопасности машин и оборудования»;",
                        "2. Конструкция ТДМ обеспечивает безопасность, надежность и удобство работы в ",
                        "период установленного срока эксплуатации;",
                        "3. Проводится динамическая и статическая балансировка рабочего колеса. ",
                        "Класс точности балансировки G 6,3 по ГОСТ ИСО 1940-1-2007;",
                        "4. Материалы изготовления проточной части, лакокрасочных покрытий, ",
                        "прокладок устойчивы к воздействию рабочей среды;",
                        "5. Все детали оборудования проходят расчет на статическую и динамическую прочность. ",
                        "Подбор материалов осуществляется с учетом максимальной надежности и долговечности ТДМ. ",
                        "Конструктивные решения обусловлены обеспечением максимального КПД, ",
                        "удобством эксплуатации и ремонтопригодностью оборудования;",
                        "6. Комплектность сопроводительных документов соответствует требованиям заказчика;",
                        "7. Режим работы ТДМ - непрерывный;",
                        "8. Комплектность поставки указана в главе 3 «Комплектность поставки»;",
                        "9. Транспортировка оборудования осуществляется в разборе (крупноузловая сборка) после ",
                        "контрольной сборки на заводе-изготовителе. Все узлы помечаются этикеткой;",
                        "10. Гарантийный срок службы оборудования – 24 месяца с даты ввода оборудования в эксплуатацию, ",
                        "но не более 36 месяцев с даты поставки."
                };

                float lineHeight = 22;
                float startY = 130;
                float margin = 50;

                for (int i = 0; i < specifications.Length; i++)
                {
                    var textRect = new XRect(margin, startY + i * lineHeight, pageWidth - 2 * margin, lineHeight * 2);
                    gfx.DrawString(specifications[i], new XFont(options.FontFamily, 11),
                        XBrushes.Black, textRect, XStringFormats.TopLeft);

                    if (specifications[i].Length > 100) lineHeight = 24;
                }
            });

            // 3. Аэродинамические характеристики (с защитой от наложения на колонтитул)
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;

                var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                gfx.DrawString("Аэродинамические характеристики", titleFont, XBrushes.Black,
                        new XRect(0, 90, pageWidth, 30), XStringFormats.TopCenter);

                // График
                using var image = XImage.FromFile(tempAeroImagePath);
                double imageWidth = pageWidth * 0.8;
                double imageHeight = image.PixelHeight * (imageWidth / image.PixelWidth);
                double imageY = 130;

                // Проверяем, чтобы график не выходил за пределы
                double maxImageHeight = pageHeight - 250; // Учитываем место под параметры и колонтитулы
                if (imageHeight > maxImageHeight)
                {
                    imageHeight = maxImageHeight;
                    imageWidth = imageHeight * (image.PixelWidth / (double)image.PixelHeight);
                }

                gfx.DrawImage(image, (pageWidth - imageWidth) / 2, imageY, imageWidth, imageHeight);

                // Параметры под графиком

                string vibroisolyators = parameters.Vibroisolation == 1 ? "предусмотрены" : "не предусмотрены";

                string materialDesign = parameters.MaterialDesign switch
                {
                    1 => "Коррозионностойкое",
                    2 => "Взрывозащищенное",
                    3 => "Взрывозащищенное Коррозионностойкое",
                    4 => "Титановое",
                    5 => "Общепромышленное",
                    _ => "Общепромышленное"
                };

                var powerWorkPoint = CalculationDiagramHelper.GetPolinomPower(parameters.FlowRateRequired, datas, parameters);
                var totalPressureWorkPoint = CalculationDiagramHelper.GetPolinomTotalPressure(parameters.FlowRateRequired, datas, parameters);
                var totalEficiencyWorkPoint = CalculationDiagramHelper.GetPolinomEeficiency(parameters.FlowRateRequired, datas, parameters);
                var aerodynamicRow = AerodinamicRowHelper.GetAerodinamicRow(datas, parameters);
                var nomberOfBlades = aerodynamicRow.NumberOfBlades;
                if (parameters.SuctionType == 1)
                { nomberOfBlades = nomberOfBlades * 2; }


                var paramFont = new XFont(options.FontFamily, 10);
                double paramsY = imageY + imageHeight + 50;
                double nameY = paramsY - 30;
                double maxY = pageHeight - 90;
                double columnWidth = (pageWidth - 100) / 2;
                double column1X = 50;
                double column2X = column1X + columnWidth + 10;
                var diameter = CalculationDiameterHelper.GetDiameter(datas, parameters);
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

                gfx.DrawString(
                $"ТДМ {markImpeller}-{diameter * 10:F1}{typeIsp} {parameters.Density} кг/м3 {parameters.Rpm} об/мин",
                paramFont,
                XBrushes.Black,
                new XRect(column1X, nameY, pageWidth, 30),
                XStringFormats.TopLeft);

                // Разбиваем параметры на два столбца
                var leftColumnParams = new List<string>();
                var rightColumnParams = new List<string>();

                // Заполняем столбцы (примерное разделение)
                leftColumnParams.Add($"Рабочая точка:");
                leftColumnParams.Add($"Статическое давление: {parameters.SystemResistance:F1} Па");
                leftColumnParams.Add($"Полное давление: {totalPressureWorkPoint:F1} Па");
                leftColumnParams.Add($"Производительность: {parameters.FlowRateRequired:F1} м3/ч");
                leftColumnParams.Add($"Плотность: {parameters.Density} кг/м3");
                leftColumnParams.Add($"Полный КПД {totalEficiencyWorkPoint:F2}");
                leftColumnParams.Add($"Мощность: {powerWorkPoint:F1} кВт");
                leftColumnParams.Add($"Угол разворота: {parameters.ExhaustDirection?.ToString() ?? ""}");

                rightColumnParams.Add($"Направление вращения: {parameters.RotaitionDirection ?? ""}");
                rightColumnParams.Add($"По ГОСТ Р 55852-2013");
                rightColumnParams.Add($"Исполнение: {materialDesign}");
                rightColumnParams.Add($"Тип лопаток рабочего колеса:");
                rightColumnParams.Add($"{aerodynamicRow.TypeOfBlades}");
                rightColumnParams.Add($"Количество лопаток рабочего колеса:");
                rightColumnParams.Add($"{nomberOfBlades}");

                // Рисуем левый столбец
                double currentY = paramsY;
                foreach (var param in leftColumnParams)
                {
                    if (currentY < maxY)
                    {
                        gfx.DrawString(param, paramFont, XBrushes.Black,
                                new XRect(column1X, currentY, columnWidth, 20),
                                XStringFormats.TopLeft);
                        currentY += 20;
                    }
                }

                // Рисуем правый столбец
                currentY = paramsY;
                foreach (var param in rightColumnParams)
                {
                    if (currentY < maxY)
                    {
                        gfx.DrawString(param, paramFont, XBrushes.Black,
                            new XRect(column2X, currentY, columnWidth, 20),
                            XStringFormats.TopLeft);
                        currentY += 20;
                    }
                }
            });


            // 4. Страница с параметрами двигателя и нагрузочной характеристикой
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;

                // Заголовок
                var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                gfx.DrawString(
                    "Параметры электродвигателя и нагрузочная характеристика",
                    titleFont,
                    XBrushes.Black,
                    new XRect(0, 100, pageWidth, 30),
                    XStringFormats.TopCenter);

                // Технические параметры двигателя
                var paramFont = new XFont(options.FontFamily, 11);
                double paramsYPos = 140;

                var motorVoltage = parameters.MotorVoltage;
                var rpm = parameters.Rpm;
                var klimatic = parameters.KLimatic;
                var markOfVzrivMotor = parameters.MarkOfVzrivMotor;
                var VFD = "не предусмотрен";
                var dopTrebovanyaMotor = parameters.DopTrebovaniyaMotor;
                if (parameters.NalichieVFD == 1)
                {
                    VFD = "предусмотрен";
                }

                var momentOfInertcia = CalculationMomentOfInertciaHelper.GetMomentOfInertcia(datas, parameters);
                var shaftPower = CalculationDiagramHelper.GetPolinomPower(parameters.FlowRateRequired, datas, parameters);

                var parametersOfMotor = new List<string>
                {
                        $"Обороты: {rpm} об/мин",
                        $"Момент инерции: {momentOfInertcia:F2} кг·м²",
                        motorVoltage != null ? $"Напряжение двигателя: {motorVoltage} В" : null,
                        $"Мощность на валу: {shaftPower:F2} кВт",
                        $"Климатическое исполнение: {klimatic}",
                        markOfVzrivMotor != null ? $"Маркировка взрывозащиты: {markOfVzrivMotor}" : null,
                        $"Наличие частотного преобразователя: {VFD}",
                        $"Дополнительные требования: {dopTrebovanyaMotor}"
                }.Where(p => p != null).ToList();

                // Рисуем каждый параметр на новой строке
                foreach (var param in parametersOfMotor)
                {
                    gfx.DrawString(
                        param,
                        paramFont,
                        XBrushes.Black,
                        new XRect(50, paramsYPos, pageWidth - 100, 20),
                        XStringFormats.TopLeft);

                    paramsYPos += 24;
                }


                // График
                using (var image = XImage.FromFile(tempTorqueImagePath))
                {
                    double imageWidth = pageWidth * 0.8;
                    double imageHeight = image.PixelHeight * (imageWidth / image.PixelWidth);
                    gfx.DrawImage(
                        image,
                        (pageWidth - imageWidth) / 2,
                        paramsYPos + 20,
                        imageWidth,
                        imageHeight);
                }
            });

            // 5. Страница комплектности поставки
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;

                // Заголовок
                var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                gfx.DrawString(
                    "Комплектность поставки",
                    titleFont,
                    XBrushes.Black,
                    new XRect(0, 100, pageWidth, 30),
                    XStringFormats.TopCenter);

                // Параметры
                var paramFont = new XFont(options.FontFamily, 11);
                double paramsYPos = 140;

                string muftType = "не предусмотрена";
                switch (parameters.MuftType)
                {
                    case 1: muftType = "не предусмотрена"; break;
                    case 2: muftType = "МУВП"; break;
                    case 3: muftType = "Лепестковая"; break;
                    case 4: muftType = "Пластинчатая"; break;
                }

                string typePPO = "не предусмотрена";
                switch (parameters.TypeOfPPO)
                {
                    case 1: typePPO = "не предусмотрена"; break;
                    case 2: typePPO = "Стандарт - сварная/масляная ванна/жидкое масло"; break;
                    case 3: typePPO = "Литая/масляная ванна/жидкое масло"; break;
                    case 4: typePPO = "типа SKF/разнесенные подшипниковые узлы на консистентной смазке"; break;
                    case 5: typePPO = "типа SKF/разнесенные подшипниковые узлы на жидком масле"; break;
                    case 6: typePPO = "на подшипниках скольжения с принудительной подачей масла через маслостанцию"; break;
                }

                string shaftSeal = "не предусмотрено";
                switch (parameters.ShaftSeal)
                {
                    case 1: shaftSeal = "не предусмотрено"; break;
                    case 2: shaftSeal = "Войлочное"; break;
                    case 3: shaftSeal = "Силиконовое"; break;
                    case 4: shaftSeal = "Сальнико-набивочное"; break;
                    case 5: shaftSeal = "Газоплотное манжетное"; break;
                    case 6: shaftSeal = "Графитное"; break;
                    case 7: shaftSeal = "Торцевое картриджное"; break;
                }

                var flangeInlet = parameters.FlangeInlet == 1 ? "предусмотрен" : "не предусмотрен";
                var flangeOutlet = parameters.FlangeOutlet == 1 ? "предусмотрен" : "не предусмотрен";
                var guideVane = parameters.GuideVane == 1 ? "предусмотрен" : "не предусмотрен";
                var teploisolation = parameters.Teploisolation == 1 ? "предусмотрен" : "не предусмотрен";

                var completenessParameters = new List<string>
                {
                        $"Тип муфты: {muftType}",
                        $"Тип ППО: {typePPO}",
                        $"Уплотнение вала: {shaftSeal}",
                        $"Направляющий аппарат: {guideVane}",
                        $"Компенсатор на входе тип: {parameters.TypeOfCompensatorInlet}",
                        $"Фланец на входе: {flangeInlet}",
                        $"Компенсатор на выходе тип: {parameters.TypeOfCompensatorOutlet}",
                        $"Фланец на выходе: {flangeOutlet}",
                        $"Теплошумоизолирующий кожух: {teploisolation}",
                        $"Датчики вибрации ходовой части: {parameters.VibroSensorPPO}",
                        $"Датчики вибрации электродвигателя: {parameters.VibroSensorMotor}",
                        $"Датчики температуры ходовой части: {parameters.TempSensorPPO}"
                };

                // Рисуем каждый параметр на новой строке
                foreach (var param in completenessParameters)
                {
                    gfx.DrawString(
                        param,
                        paramFont,
                        XBrushes.Black,
                        new XRect(50, paramsYPos, pageWidth - 100, 20),
                        XStringFormats.TopLeft);

                    paramsYPos += 24;
                }
            });

            // 6. Дополнительная комплектация (если есть)
            if (parameters.DopKomplekt != null)
            {
                AddContentPage(document, options, (gfx, page) =>
                {
                    double pageWidth = page.Width.Point;
                    double pageHeight = page.Height.Point;

                    // Заголовок
                    var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                    gfx.DrawString(
                        "Дополнительная комплектация ТДМ",
                        titleFont,
                        XBrushes.Black,
                        new XRect(0, 100, pageWidth, 30),
                        XStringFormats.TopCenter);

                    // Состав ЗиП
                    if (!string.IsNullOrEmpty(parameters.DopKomplekt))
                    {
                        string[] dopKomplektLines = parameters.DopKomplekt.Split('\n');
                        float lineHeight = 20;
                        float startY = 170;
                        var textFont = new XFont(options.FontFamily, 11);

                        gfx.DrawString(
                            "Комплект",
                            textFont,
                            XBrushes.Black,
                            new XRect(50, startY, pageWidth - 100, lineHeight),
                            XStringFormats.TopLeft);

                        for (int i = 0; i < dopKomplektLines.Length; i++)
                        {
                            gfx.DrawString(
                                dopKomplektLines[i].Trim(),
                                textFont,
                                XBrushes.Black,
                                new XRect(50, startY + (i + 1) * lineHeight, pageWidth - 100, lineHeight),
                                XStringFormats.TopLeft);
                        }
                    }
                });
            }
            // 6. Дополнительная требования (если есть)

            AddContentPage(document, options, (gfx, page) =>
        {
            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            // Заголовок
            var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
            gfx.DrawString(
                "Дополнительные требования к ТДМ",
                titleFont,
                XBrushes.Black,
                new XRect(0, 100, pageWidth, 30),
                XStringFormats.TopCenter);

            // Состав ЗиП
            if (!string.IsNullOrEmpty(parameters.DopTrebovanyaTDM))
            {
                string[] dopTrebovanyaTDMLines = parameters.DopTrebovanyaTDM.Split('\n');
                float lineHeight = 20;
                float startY = 170;
                var textFont = new XFont(options.FontFamily, 11);

                for (int i = 0; i < dopTrebovanyaTDMLines.Length; i++)
                {
                    gfx.DrawString(
                        dopTrebovanyaTDMLines[i].Trim(),
                        textFont,
                        XBrushes.Black,
                        new XRect(50, startY + (i + 1) * lineHeight, pageWidth - 100, lineHeight),
                        XStringFormats.TopLeft);
                }
            }
        });


            // 7. Страница ЗиП (Запасные части и инструменты)
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;

                // Заголовок
                var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                gfx.DrawString(
                    "Запасные части и инструменты (ЗиП)",
                    titleFont,
                    XBrushes.Black,
                    new XRect(0, 100, pageWidth, 30),
                    XStringFormats.TopCenter);

                // Основной текст
                var textFont = new XFont(options.FontFamily, 11);
                gfx.DrawString(
                    "Комплект запасных частей и инструментов поставляется согласно спецификации.",
                    textFont,
                    XBrushes.Black,
                    new XRect(50, 140, pageWidth - 100, 20),
                    XStringFormats.TopLeft);

                // Состав ЗиП
                if (!string.IsNullOrEmpty(parameters.Zip))
                {
                    string[] zipLines = parameters.Zip.Split('\n');
                    float lineHeight = 20;
                    float startY = 170;

                    gfx.DrawString(
                        "Состав ЗиП:",
                        textFont,
                        XBrushes.Black,
                        new XRect(50, startY, pageWidth - 100, lineHeight),
                        XStringFormats.TopLeft);

                    for (int i = 0; i < zipLines.Length; i++)
                    {
                        gfx.DrawString(
                            zipLines[i].Trim(),
                            textFont,
                            XBrushes.Black,
                            new XRect(50, startY + (i + 1) * lineHeight, pageWidth - 100, lineHeight),
                            XStringFormats.TopLeft);
                    }
                }
            });

            // 8. Страница документации и дополнительных услуг
            AddContentPage(document, options, (gfx, page) =>
            {
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;

                // Разделительная линия по горизонтали
                double middleY = pageHeight / 2;
                gfx.DrawLine(XPens.LightGray,
                            new XPoint(40, middleY),
                            new XPoint(pageWidth - 40, middleY));

                // Шрифты
                var titleFont = new XFont(options.FontFamily, 16, XFontStyleEx.Bold);
                var sectionFont = new XFont(options.FontFamily, 14, XFontStyleEx.Bold);
                var textFont = new XFont(options.FontFamily, 11);
                float lineHeight = 20;
                float margin = 50;

                // 1. Верхняя половина - Сопроводительная документация
                gfx.DrawString(
                    "Сопроводительная документация",
                    sectionFont,
                    XBrushes.Black,
                    new XRect(0, 100, pageWidth, 30),
                    XStringFormats.TopCenter);

                string[] documentation = new string[]
                {
                        "• Монтажный чертеж с указанием габаритных присоединительных/установочных размеров",
                        "• инструкция по монтажу, пуску и обкатке (ТДМ)",
                        "• инструкция по ремонту и демонтажу (ТДМ)",
                        "• руководство по эксплуатации (ТДМ)",
                        "• паспорт технический (ТДМ)",
                        "• комплектная ведомость",
                        "• декларация или сертификат соотвествия",
                        "• техническая документация на закупаемое оборудование"
                };

                float docStartY = 140;
                for (int i = 0; i < documentation.Length; i++)
                {
                    gfx.DrawString(
                        documentation[i],
                        textFont,
                        XBrushes.Black,
                        new XRect(margin, docStartY + i * lineHeight, pageWidth - 2 * margin, lineHeight),
                        XStringFormats.TopLeft);
                }

                // 2. Нижняя половина - Дополнительные услуги
                gfx.DrawString(
                    "Дополнительные услуги",
                    sectionFont,
                    XBrushes.Black,
                    new XRect(0, middleY + 40, pageWidth, 30),
                    XStringFormats.TopCenter);

                // Статусы услуг
                var shefMontage = parameters.ShefMontage == 1 ? "предусмотрен" : "не предусмотрен";
                var puskoNaladka = parameters.PuskoNaladka == 1 ? "предусмотрены" : "не предусмотрены";
                var studyOfPersonal = parameters.StudyOfPersonal == 1 ? "предусмотрено" : "не предусмотрено";

                string[] services = new string[]
                {
                        $"Шеф-монтаж оборудования: {shefMontage}",
                        $"Пусконаладочные работы: {puskoNaladka}",
                        $"Обучение персонала: {studyOfPersonal}"
                };

                double servicesStartY = middleY + 80;
                for (int i = 0; i < services.Length; i++)
                {
                    gfx.DrawString(
                        services[i],
                        textFont,
                        XBrushes.Black,
                        new XRect(margin, servicesStartY + i * lineHeight, pageWidth - 2 * margin, lineHeight),
                        XStringFormats.TopLeft);
                }
            });

            document.Save(pdfPath);
            Debug.WriteLine($"PDF успешно сохранен: {pdfPath}");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при создании PDF: {ex.Message}");
            throw;
        }
        finally
        {
            try { File.Delete(tempAeroImagePath); } catch { }
            try { File.Delete(tempTorqueImagePath); } catch { }
        }
    }

    private static void AddContentPage(PdfDocument document, PdfExportOptions options,
                                     Action<XGraphics, PdfPage> contentDrawer)
    {
        var page = document.AddPage();
        page.Width = XUnit.FromMillimeter(210);
        page.Height = XUnit.FromMillimeter(297);
        page.Orientation = PdfSharp.PageOrientation.Portrait;
        _pageCounter++;

        using var gfx = XGraphics.FromPdfPage(page);

        // Колонтитулы
        DrawHeader(gfx, page, options);
        DrawFooter(gfx, page, options);

        // Основное содержимое с безопасными отступами
        double contentTop = 90;  // Отступ сверху
        double contentBottom = 90; // Отступ снизу
        gfx.IntersectClip(new XRect(0, contentTop, page.Width,
                                   page.Height - contentTop - contentBottom));

        contentDrawer(gfx, page);
    }
}

public class PdfExportOptions
{
    public string FontFamily { get; set; } = "Times New Roman";
    public PdfSharp.PageOrientation Orientation { get; set; } = PdfSharp.PageOrientation.Portrait;
    public string Title { get; set; } = "";
}

