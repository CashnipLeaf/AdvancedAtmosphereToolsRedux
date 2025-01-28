using System;
using System.Threading.Tasks;
using AdvancedAtmosphereToolsRedux.Interfaces;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TestModule
{
    public class TestModule : IFlatTemperatureModifier, IRequiresFinalSetup
    {
        public TestModule() { }

        public double garbage;

        public double GetFlatTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            return 0.0;
        }

        public void FinalSetup()
        {
            throw new Exception("THIS IS EXPECTED.");
        }
    }

    public class TestModule2 : IFlatTemperatureModifier, IRequiresFinalSetup
    {
        public TestModule2() { }

        public double garbage;

        public double GetFlatTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            return 0.0;
        }

        public void FinalSetup()
        {
            Task.Delay(3000).Wait();

            Utils.LogInfo("Module TestModule2 ran asynchronously");
        }
    }
}
