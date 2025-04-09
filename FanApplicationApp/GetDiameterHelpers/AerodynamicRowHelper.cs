using SpeedCalc.Models;
using System.Collections.Generic;
using System.Linq;

namespace SpeedCalc.GetDiameterHelpers
{
    public class AerodinamicRowHelper
    {
        public static AerodynamicsData? GetAerodinamicRow(List<AerodynamicsData> datas, CalculationParameters parameters)
        {
            double speed = CalculationDiameterHelper.GetSpeed(parameters);
            var aerodynamicsByType = datas.Where(d => d.Type == (AerodynamicsType)parameters.Type);
            var aerodynamicRow = aerodynamicsByType.FirstOrDefault(d => d.MinSpeed <= speed && d.MaxSpeed > speed);

            if (aerodynamicRow == null)
            {
                return null;
            }

            return aerodynamicRow;
        }
    }
}
