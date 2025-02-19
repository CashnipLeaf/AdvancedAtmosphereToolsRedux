using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GlobalIntakeChokeFactor
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    internal class GlobalIntakeChokeFactorLoader : BaseLoader, ITypeParser<GlobalIntakeChokeFactor>
    {
        public GlobalIntakeChokeFactor Value { get; set; }

        public GlobalIntakeChokeFactorLoader() => Value = new GlobalIntakeChokeFactor();

        [ParserTarget("GlobalAirIntakeChokeFactor", Optional = true)]
        public NumericParser<Double> ChokeFactor
        {
            get => Value.ChokeFactor;
            set
            {
                Value.ChokeFactor = value;
                AtmoToolsRedux_Data.SetAirIntakeChokeFactor(Value, generatedBody.celestialBody);
            }
        }
    }
}
