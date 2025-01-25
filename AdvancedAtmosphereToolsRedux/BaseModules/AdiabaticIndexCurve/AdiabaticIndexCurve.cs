using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.AdiabaticIndexCurve
{
    public class AdiabaticIndexCurve : IBaseAdiabaticIndex
    {
        public FloatCurve BaseAdiabaticIndexCurve;
        
        public AdiabaticIndexCurve() { }

        public double GetBaseAdiabaticIndex(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity) => (double)BaseAdiabaticIndexCurve.Evaluate((float)alt);
    }
}
