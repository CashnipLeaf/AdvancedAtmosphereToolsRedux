using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.BinaryWindData
{
    [ParserTargetExternal("Atmosphere", "BinaryWindData", "Kopernicus")]
    public class BinaryWindDataLoader : BaseLoader, IParserPostApplyEventSubscriber, ITypeParser<BinaryWindData>
    {
        public BinaryWindData Value {  get; set; }

        public BinaryWindDataLoader() => Value = new BinaryWindData();

        //initialize the stuff
        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            Value.Initialize(generatedBody.celestialBody);

            AtmoToolsRedux_Data.AddWindProvider(Value, generatedBody.celestialBody);
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

        [ParserTarget("pathX")] //north/south
        public string PathX
        {
            get => Value.PathX;
            set => Value.PathX = value;
        }

        [ParserTarget("pathY")] //vertical
        public string PathY
        {
            get => Value.PathY;
            set => Value.PathY = value;
        }

        [ParserTarget("pathZ")] //east/west
        public string PathZ
        {
            get => Value.PathZ;
            set => Value.PathZ = value;
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

        [ParserTarget("verticalWindMultiplier", Optional = true)]
        public NumericParser<Double> VerticalWindMultiplier
        {
            get => Value.VerticalWindMultiplier;
            set => Value.VerticalWindMultiplier = value;
        }
    }
}
