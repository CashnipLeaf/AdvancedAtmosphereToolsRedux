using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TestModule
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class TestModuleLoader : BaseLoader, ITypeParser<TestModule>
    {
        public TestModule Value { get; set; }
        public TestModuleLoader()
        {
            Value = new TestModule();
        }

        [ParserTarget("garbage", Optional = true)]
        public NumericParser<Double> Garbage
        {
            get => 0.0;
            set
            {
                AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
                data.AddFlatTemperatureModifier(Value);
            }
        }
    }

    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class TestModule2Loader : BaseLoader, ITypeParser<TestModule2>
    {
        public TestModule2 Value { get; set; }
        public TestModule2Loader()
        {
            Value = new TestModule2();
        }

        [ParserTarget("garbage2", Optional = true)]
        public NumericParser<Double> Garbage
        {
            get => 0.0;
            set
            {
                AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
                data.AddFlatTemperatureModifier(Value);
            }
        }
    }
}
