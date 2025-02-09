using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.SubstellarPressureGradient
{
    [ParserTargetExternal("Atmosphere", "SubstellarPressureGradient", "Kopernicus")]
    public class SubStellarPressureGradientLoader : BaseLoader, ITypeParser<SubstellarPressureGradient>, IParserPostApplyEventSubscriber
    {
        public SubstellarPressureGradient Value { get; set; }

        public SubStellarPressureGradientLoader() => Value = new SubstellarPressureGradient(generatedBody.celestialBody);

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node) => AtmoToolsRedux_Data.AddFractionalPressureModifier(Value, generatedBody.celestialBody);

        [ParserTarget("angleOffset", Optional = true)]
        public NumericParser<Single> AngleOffset
        {
            get => Value.angleOffset;
            set => Value.angleOffset = value;
        }

        [ParserTargetCollection("gradientCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> GradientCurve
        {
            get => Value.GradientCurve != null ? Utility.FloatCurveToList(Value.GradientCurve) : null;
            set => Value.GradientCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("altitudeCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> AltitudeCurve
        {
            get => Value.AltitudeCurve != null ? Utility.FloatCurveToList(Value.AltitudeCurve) : null;
            set => Value.AltitudeCurve = Utility.ListToFloatCurve(value);
        }
    }
}
