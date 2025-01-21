using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseClasses
{
    [ParserTargetExternal("Atmosphere", "BinaryWindData", "Kopernicus")]
    public class BinaryWindDataLoader : BaseLoader, IParserPostApplyEventSubscriber, ITypeParser<BinaryWindData>
    {
        public BinaryWindData Value { get; set; }

        private CelestialBody body;

        public BinaryWindDataLoader()
        {
            body = generatedBody.celestialBody;
            AtmosphereData data = PublicUtils.GetAtmosphereData(body);

            Value = new BinaryWindData();
            data.AddWindProvider(Value);
        }

        //initialize the stuff
        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            try
            {
                Value.Initialize();
                Value.Initialized = true;
            }
            catch
            {
                Value.Initialized = false;
            }
        }

        [ParserTarget("sizeLon")]
        public int SizeLon
        {
            get => Value.sizeLon;
            set => Value.sizeLon = value;
        }

        [ParserTarget("sizeLat")]
        public int SizeLat
        {
            get => Value.sizeLat;
            set => Value.sizeLat = value;
        }

        [ParserTarget("sizeAlt")]
        public int SizeAlt
        {
            get => Value.sizeAlt;
            set => Value.sizeAlt = value;
        }

        [ParserTarget("timestamps")]
        public int Timestamps
        {
            get => Value.timestamps;
            set => Value.timestamps = value;
        }

        [ParserTarget("modelTop")]
        public double ModelTop
        {
            get => Value.modeltop;
            set => Value.modeltop = value;
        }

        [ParserTarget("TimeStep")]
        public double TimeStep
        {
            get => Value.TimeStep;
            set => Value.TimeStep = value;
        }

        [ParserTarget("PathX")]
        public string PathX
        {
            get => Value.PathX;
            set => Value.PathX = value;
        }

        [ParserTarget("PathY")]
        public string PathY
        {
            get => Value.PathY;
            set => Value.PathY = value;
        }

        [ParserTarget("PathZ")]
        public string PathZ
        {
            get => Value.PathZ;
            set => Value.PathZ = value;
        }

        //optional values below
        [ParserTarget("invertAlt", Optional = true)]
        public bool InvertAlt
        {
            get => Value.invertalt;
            set => Value.invertalt = value;
        }

        [ParserTarget("initialOffset", Optional = true)]
        public int InitialOffset
        {
            get => Value.initialoffset;
            set => Value.initialoffset = value;
        }

        [ParserTarget("lonOffset", Optional = true)]
        public double LonOffset
        {
            get => Value.LonOffset;
            set => Value.LonOffset = value;
        }

        [ParserTarget("timeOffset", Optional = true)]
        public double TimeOffset
        {
            get => Value.TimeOffset;
            set => Value.TimeOffset = value;
        }

        [ParserTarget("verticalWindMultiplier", Optional = true)]
        public double VerticalWindMultiplier
        {
            get => Value.VerticalWindMultiplier;
            set => Value.VerticalWindMultiplier = value;
        }
    }
}
