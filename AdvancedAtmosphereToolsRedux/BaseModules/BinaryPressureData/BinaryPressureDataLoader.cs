using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.BinaryPressureData
{
    [ParserTargetExternal("Atmosphere", "BinaryPressureData", "Kopernicus")]
    public class BinaryPressureDataLoader : BaseLoader, ITypeParser<BinaryPressureData>, IParserPostApplyEventSubscriber
    {
        public BinaryPressureData Value { get; set; }

        public BinaryPressureDataLoader()
        {
            Value = new BinaryPressureData();
        }

        //initialize the stuff
        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            Value.Initialize(generatedBody.celestialBody);

            AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
            data.SetBasePressure(Value);
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
    }
}
