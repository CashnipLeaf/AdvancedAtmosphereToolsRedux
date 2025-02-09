using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.BinaryTemperatureData
{
    [ParserTargetExternal("Atmosphere", "BinaryTemperatureData", "Kopernicus")]
    public class BinaryTemperatureDataLoader : BaseLoader, ITypeParser<BinaryTemperatureData>, IParserPostApplyEventSubscriber
    {
        public BinaryTemperatureData Value { get; set; }

        public BinaryTemperatureDataLoader() => Value = new BinaryTemperatureData();

        //initialize the stuff
        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            Value.Initialize(generatedBody.celestialBody);

            AtmoToolsRedux_Data.SetBaseTemperature(Value, generatedBody.celestialBody);
        }

        [ParserTarget("sizeLon")]
        public NumericParser<Int32> SizeLon
        {
            get => Value.sizeLon;
            set => Value.sizeLon = value;
        }

        [ParserTarget("sizeLat")]
        public NumericParser<Int32> SizeLat
        {
            get => Value.sizeLat;
            set => Value.sizeLat = value;
        }

        [ParserTarget("sizeAlt")]
        public NumericParser<Int32> SizeAlt
        {
            get => Value.sizeAlt;
            set => Value.sizeAlt = value;
        }

        [ParserTarget("timestamps")]
        public NumericParser<Int32> Timestamps
        {
            get => Value.timestamps;
            set => Value.timestamps = value;
        }

        [ParserTarget("modelTop")]
        public NumericParser<Double> ModelTop
        {
            get => Value.modeltop;
            set => Value.modeltop = value;
        }

        [ParserTarget("timeStep")]
        public NumericParser<Double> TimeStep
        {
            get => Value.TimeStep;
            set => Value.TimeStep = value;
        }

        [ParserTarget("scaleFactor")]
        public NumericParser<Double> ScaleFactor
        {
            get => Value.ScaleFactor;
            set => Value.ScaleFactor = value;
        }

        [ParserTarget("path")]
        public string Path
        {
            get => Value.Path;
            set => Value.Path = value;
        }

        //optional values below
        [ParserTarget("invertAltitude", Optional = true)]
        public NumericParser<Boolean> InvertAlt
        {
            get => Value.invertalt;
            set => Value.invertalt = value;
        }

        [ParserTarget("initialOffset", Optional = true)]
        public NumericParser<Int32> InitialOffset
        {
            get => Value.initialoffset;
            set => Value.initialoffset = value;
        }

        [ParserTarget("longitudeOffset", Optional = true)]
        public NumericParser<Double> LonOffset
        {
            get => Value.LonOffset;
            set => Value.LonOffset = value;
        }

        [ParserTarget("timeOffset", Optional = true)]
        public NumericParser<Double> TimeOffset
        {
            get => Value.TimeOffset;
            set => Value.TimeOffset = value;
        }

        [ParserTarget("disableLatitudeBias", Optional = true)]
        public NumericParser<Boolean> DisableLatitudeBias
        {
            get => Value.DisableLatitudeBias;
            set => Value.DisableLatitudeBias = value;
        }

        [ParserTarget("disableLatitudeSunMult", Optional = true)]
        public NumericParser<Boolean> DisableLatitudeSunMult
        {
            get => Value.DisableLatitudeSunMult;
            set => Value.DisableLatitudeSunMult = value;
        }

        [ParserTarget("disableAxialSunBias", Optional = true)]
        public NumericParser<Boolean> DisableAxialSunBias
        {
            get => Value.DisableAxialSunBias;
            set => Value.DisableAxialSunBias = value;
        }

        [ParserTarget("disableEccentricityBias", Optional = true)]
        public NumericParser<Boolean> DisableEccentricityBias
        {
            get => Value.DisableEccentricityBias;
            set => Value.DisableEccentricityBias = value;
        }
    }
}
