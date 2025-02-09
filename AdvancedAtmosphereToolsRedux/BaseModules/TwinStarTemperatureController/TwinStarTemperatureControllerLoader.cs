using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TwinStarTemperatureController
{
    [ParserTargetExternal("Atmosphere", "TwinStarTemperatureController", "Kopernicus")]
    public class TwinStarTemperatureControllerLoader : BaseLoader, ITypeParser<TwinStarTemperatureController>, IParserPostApplyEventSubscriber
    {
        public TwinStarTemperatureController Value { get; set; }

        public TwinStarTemperatureControllerLoader() => Value = new TwinStarTemperatureController();

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            Value.Initialize(generatedBody.celestialBody);

            AtmoToolsRedux_Data.SetBaseTemperature(Value, generatedBody.celestialBody);
        }

        [ParserTarget("secondaryStar")]
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

        [ParserTarget("minDistance", Optional = true)]
        public NumericParser<Double> MinDistance
        {
            get => Value.minDistance;
            set => Value.minDistance = value;
        }

        [ParserTarget("maxDistance", Optional = true)]
        public NumericParser<Double> MaxDistance
        {
            get => Value.maxDistance;
            set => Value.maxDistance = value;
        }

        [ParserTargetCollection("temperatureCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> TemperatureCurve
        {
            get => Value.temperatureCurve != null ? Utility.FloatCurveToList(Value.temperatureCurve) : null;
            set => Value.temperatureCurve = Utility.ListToFloatCurve(value);
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

        [ParserTargetCollection("temperatureAxialSunBiasCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> TemperatureAxialSunBiasCurve
        {
            get => Value.temperatureAxialSunBiasCurve != null ? Utility.FloatCurveToList(Value.temperatureAxialSunBiasCurve) : null;
            set => Value.temperatureAxialSunBiasCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("temperatureAxialSunMultCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> TemperatureAxialSunMultCurve
        {
            get => Value.temperatureAxialSunMultCurve != null ? Utility.FloatCurveToList(Value.temperatureAxialSunMultCurve) : null;
            set => Value.temperatureAxialSunMultCurve = Utility.ListToFloatCurve(value);
        }

        [ParserTargetCollection("temperatureEccentricityBiasCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> TemperatureEccentricityBiasCurve
        {
            get => Value.temperatureEccentricityBiasCurve != null ? Utility.FloatCurveToList(Value.temperatureEccentricityBiasCurve) : null;
            set => Value.temperatureEccentricityBiasCurve = Utility.ListToFloatCurve(value);
        }
    }
}
