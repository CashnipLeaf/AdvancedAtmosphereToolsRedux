using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedAtmosphereToolsRedux.Interfaces;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GlobalIntakeChokeFactor
{
    public class GlobalIntakeChokeFactor : IAirIntakeChokeFactor
    {
        private double chokefactor = 0.0;
        public double ChokeFactor
        {
            get => chokefactor;
            set => chokefactor = UtilMath.Clamp01(value);
        }

        public GlobalIntakeChokeFactor() { }

        public void Initialize() { }

        public double GetAirIntakeChokeFactor(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity) => chokefactor;
    }
}
