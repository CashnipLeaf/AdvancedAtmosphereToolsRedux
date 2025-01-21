using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseClasses
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class MaxTempAngleOffsetLoader : BaseLoader
    {
        private CelestialBody body;
        private AtmosphereData data;

        public MaxTempAngleOffsetLoader()
        {
            body = generatedBody.celestialBody;
            data = PublicUtils.GetAtmosphereData(body);
        }

        [ParserTarget("maxTempAngleOffset", Optional = true)]
        public double MaxTempAngleOffset
        {
            get => data.maxTempAngleOffset;
            set => data.maxTempAngleOffset = value;
        }
    }
}
