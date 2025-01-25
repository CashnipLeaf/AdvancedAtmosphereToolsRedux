using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.SecondStarTemperatureController
{
    [ParserTargetExternal("Atmosphere", "SecondStarTemperatureController", "Kopernicus")]
    public class SecondStarTemperatureControllerLoader : BaseLoader, ITypeParser<SecondStarTemperatureController>, IParserPostApplyEventSubscriber
    {
        public SecondStarTemperatureController Value { get; set; }

        public SecondStarTemperatureControllerLoader()
        {
            Value = new SecondStarTemperatureController();
        }

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            Value.Initialize(generatedBody.celestialBody);

            AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
            data.AddFlatTemperatureModifier(Value);
        }

        [ParserTarget("secondStar")]
        public string SecondStar
        {
            get => Value.secondStarName;
            set => Value.secondStarName = value;
        }

        [ParserTarget("maxTempAngleOffset", Optional = true)]
        public NumericParser<Single> MaxTempAngleOffset
        {
            get => Value.maxTempAngleOffset;
            set => Value.maxTempAngleOffset = value;
        }

        [ParserTargetCollection("temperatureSunMultCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> TemperatureSunMultCurve
        {
            get => Value.temperatureSunMultCurve != null ? Utility.FloatCurveToList(Value.temperatureSunMultCurve) : null;
            set => Value.temperatureSunMultCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("temperatureLatitudeBiasCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> TemperatureLatitudeBiasCurve
        {
            get => Value.temperatureLatitudeBiasCurve != null ? Utility.FloatCurveToList(Value.temperatureLatitudeBiasCurve) : null;
            set => Value.temperatureLatitudeBiasCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("temperatureLatitudeSunMultCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> TemperatureLatitudeSunMultCurve
        {
            get => Value.temperatureLatitudeSunMultCurve != null ? Utility.FloatCurveToList(Value.temperatureLatitudeSunMultCurve) : null;
            set => Value.temperatureLatitudeSunMultCurve = Utility.ListToFloatCurve(value);
        }
    }
}
