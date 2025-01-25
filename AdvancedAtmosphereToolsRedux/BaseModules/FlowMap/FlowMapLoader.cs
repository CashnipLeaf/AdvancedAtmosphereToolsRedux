using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.FlowMap
{
    [ParserTargetExternal("Atmosphere", "FlowMap", "Kopernicus")]
    public class FlowMapLoader : BaseLoader, ITypeParser<FlowMap>, IParserPostApplyEventSubscriber
    {
        public FlowMap Value { get; set; }
        
        public FlowMapLoader()
        {
            Value = new FlowMap();
        }

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            Value.Initialize();

            AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
            data.AddWindProvider(Value);
        }

        [ParserTarget("path")]
        public string Path
        {
            get => Value.Path;
            set => Value.Path = value;
        }

        [ParserTarget("northSouthWindSpeed")] //X direction
        public NumericParser<Single> X_WindSpeed
        {
            get => Value.x_WindSpeed;
            set => Value.x_WindSpeed = value;
        }

        [ParserTarget("verticalWindSpeed")] //Y direction
        public NumericParser<Single> Y_WindSpeed
        {
            get => Value.y_WindSpeed;
            set => Value.y_WindSpeed = value;
        }

        [ParserTarget("eastWestWindSpeed")] //Z direction
        public NumericParser<Single> Z_WindSpeed
        {
            get => Value.z_WindSpeed;
            set => Value.z_WindSpeed = value;
        }

        [ParserTarget("useThirdChannel", Optional = true)]
        public NumericParser<Boolean> UseThirdChannel
        {
            get => Value.useThirdChannel;
            set => Value.useThirdChannel = value;
        }

        [ParserTarget("canScroll", Optional = true)]
        public NumericParser<Boolean> CanScroll
        {
            get => Value.canscroll;
            set => Value.canscroll = value;
        }

        [ParserTarget("scrollPeriod", Optional = true)]
        public NumericParser<Double> ScrollPeriod
        {
            get => Value.scrollperiod;
            set => Value.scrollperiod = value;
        }

        [ParserTarget("timeOffset", Optional = true)]
        public NumericParser<Single> TimeOffset
        {
            get => Value.timeoffset;
            set => Value.timeoffset = value;
        }

        [ParserTargetCollection("AltitudeSpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> AltitudeSpeedMultCurve
        {
            get => Value.AltitudeSpeedMultCurve != null ? Utility.FloatCurveToList(Value.AltitudeSpeedMultCurve) : null;
            set => Value.AltitudeSpeedMultCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTarget("minAlt", Optional = true)]
        public NumericParser<Single> MinAlt
        {
            get => Value.minalt;
            set
            {
                Value.minalt = value;
                Value.minentered = true;
            }
        }

        [ParserTarget("maxAlt", Optional = true)]
        public NumericParser<Single> MaxAlt
        {
            get => Value.maxalt;
            set
            {
                Value.maxalt = value;
                Value.maxentered = true;
            }
        }

        [ParserTarget("lowerFade", Optional = true)]
        public NumericParser<Single> LowerFade
        {
            get => Value.lowerfade;
            set => Value.lowerfade = value;
        }

        [ParserTarget("upperFade", Optional = true)]
        public NumericParser<Single> UpperFade
        {
            get => Value.upperfade;
            set => Value.upperfade = value;
        }

        [ParserTargetCollection("NorthSouthAltitudeSpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> X_AltitudeSpeedMultCurve
        {
            get => Value.X_AltitudeSpeedMultCurve != null ? Utility.FloatCurveToList(Value.X_AltitudeSpeedMultCurve) : null;
            set => Value.X_AltitudeSpeedMultCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("VerticalAltitudeSpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> Y_AltitudeSpeedMultCurve
        {
            get => Value.Y_AltitudeSpeedMultCurve != null ? Utility.FloatCurveToList(Value.Y_AltitudeSpeedMultCurve) : null;
            set => Value.Y_AltitudeSpeedMultCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("EastWestAltitudeSpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> Z_AltitudeSpeedMultCurve
        {
            get => Value.Z_AltitudeSpeedMultCurve != null ? Utility.FloatCurveToList(Value.Z_AltitudeSpeedMultCurve) : null;
            set => Value.Z_AltitudeSpeedMultCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("TimeSpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> WindSpeedMultiplierTimeCurve
        {
            get => Value.WindSpeedMultiplierTimeCurve != null ? Utility.FloatCurveToList(Value.WindSpeedMultiplierTimeCurve) : null;
            set => Value.WindSpeedMultiplierTimeCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("TrueAnomalySpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> TrueAnomalyMultiplierCurve
        {
            get => Value.TrueAnomalyMultiplierCurve != null ? Utility.FloatCurveToList(Value.TrueAnomalyMultiplierCurve) : null;
            set => Value.TrueAnomalyMultiplierCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("EccentricitySpeedMultiplierCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> EccentricityMultiplierCurve
        {
            get => Value.EccentricityMultiplierCurve != null ? Utility.FloatCurveToList(Value.EccentricityMultiplierCurve) : null;
            set => Value.EccentricityMultiplierCurve = Utility.ListToFloatCurve(value);
        }
    }
}
