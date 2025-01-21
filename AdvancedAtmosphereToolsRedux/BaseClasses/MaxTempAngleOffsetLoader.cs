using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseClasses
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class MaxTempAngleOffsetLoader : BaseLoader
    {
        private AtmosphereData data;

        public MaxTempAngleOffsetLoader()
        {
            CelestialBody body = generatedBody.celestialBody;
            data = AtmoToolsReduxUtils.GetAtmosphereData(body);
        }

        [ParserTarget("maxTempAngleOffset", Optional = true)]
        public double MaxTempAngleOffset
        {
            get => data.maxTempAngleOffset;
            set => data.maxTempAngleOffset = value;
        }
    }
}
