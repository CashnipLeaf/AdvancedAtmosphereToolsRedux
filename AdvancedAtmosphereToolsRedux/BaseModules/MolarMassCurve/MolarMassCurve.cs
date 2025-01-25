using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.MolarMassCurve
{
    public class MolarMassCurve : IBaseMolarMass
    {
        public FloatCurve BaseMolarMassCurve;

        public MolarMassCurve() { }

        public double GetBaseMolarMass(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity) => (double)BaseMolarMassCurve.Evaluate((float)alt);
    }
}
