using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.FractionalPressureModifierMap
{
    [ParserTargetExternal("Atmosphere", "FractionalPressureModifierMap", "Kopernicus")]
    public class FractionalPressureModifierMapLoader : BaseLoader, ITypeParser<FractionalPressureModifierMap>, IParserPostApplyEventSubscriber
    {
        public FractionalPressureModifierMap Value { get; set; }

        public FractionalPressureModifierMapLoader() => Value = new FractionalPressureModifierMap();

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node) => AtmoToolsRedux_Data.AddFractionalPressureModifier(Value, generatedBody.celestialBody);

        [ParserTarget("pressureModifierRange")]
        public NumericParser<Double> PressureModifierRange
        {
            get => Value.PressureModifierRange;
            set => Value.PressureModifierRange = value;
        }

        [ParserTarget("pressureModifierOffset")]
        public NumericParser<Double> PressureModifierOffset
        {
            get => Value.PressureModifierOffset;
            set => Value.PressureModifierOffset = value;
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
