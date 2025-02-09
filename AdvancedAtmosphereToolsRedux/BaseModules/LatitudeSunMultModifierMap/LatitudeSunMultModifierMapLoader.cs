using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.LatitudeSunMultModifierMap
{
    [ParserTargetExternal("Atmosphere", "LatitudeSunMultModifierMap", "Kopernicus")]
    public class LatitudeSunMultModifierMapLoader : BaseLoader, ITypeParser<LatitudeSunMultModifierMap>, IParserPostApplyEventSubscriber
    {
        public LatitudeSunMultModifierMap Value { get; set; }

        public LatitudeSunMultModifierMapLoader() => Value = new LatitudeSunMultModifierMap();

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node) => AtmoToolsRedux_Data.AddFractionalLatitudeSunMultModifier(Value, generatedBody.celestialBody);

        [ParserTarget("latitudeSunMultModifierRange")]
        public NumericParser<Double> LatitudeSunMultModifierRange
        {
            get => Value.LatitudeSunMultModifierRange;
            set => Value.LatitudeSunMultModifierRange = value;
        }

        [ParserTarget("latitudeSunMultModifierOffset")]
        public NumericParser<Double> LatitudeSunMultModifierOffset
        {
            get => Value.LatitudeSunMultModifierOffset;
            set => Value.LatitudeSunMultModifierOffset = value;
        }

        [ParserTarget("map")]
        public string FilePath
        {
            get => Value.FilePath;
            set => Value.SetTexture(value);
        }

        [ParserTarget("scrollPeriod", Optional = true)]
        public NumericParser<Double> ScrollPeriod
        {
            get => Value.ScrollPeriod;
            set => Value.ScrollPeriod = value;
        }

        [ParserTargetCollection("AltitudeMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> AltitudeMultiplierCurve
        {
            get => Value.AltitudeMultiplierCurve != null ? Utility.FloatCurveToList(Value.AltitudeMultiplierCurve) : null;
            set => Value.AltitudeMultiplierCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("TimeMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> TimeMultiplierCurve
        {
            get => Value.TimeMultiplierCurve != null ? Utility.FloatCurveToList(Value.TimeMultiplierCurve) : null;
            set => Value.TimeMultiplierCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("EccentricityMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> EccentricityMultiplierCurve
        {
            get => Value.EccentricityMultiplierCurve != null ? Utility.FloatCurveToList(Value.EccentricityMultiplierCurve) : null;
            set => Value.EccentricityMultiplierCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("TrueAnomalyMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> TrueAnomalyMultiplierCurve
        {
            get => Value.TrueAnomalyMultiplierCurve != null ? Utility.FloatCurveToList(Value.TrueAnomalyMultiplierCurve) : null;
            set => Value.TrueAnomalyMultiplierCurve = Utility.ListToFloatCurve(value);
        }
    }
}
