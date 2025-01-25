using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.Configuration.Parsing;

//Loaders for individual properties that do not require an entire object to hold them.
namespace AdvancedAtmosphereToolsRedux.BaseModules
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class MaxTempAngleOffsetLoader : BaseLoader
    {
        [ParserTarget("maxTempAngleOffset", Optional = true)]
        public NumericParser<Double> MaxTempAngleOffset
        {
            get => (double)generatedBody.celestialBody.MaxTempAngleOffset();
            set 
            {
                AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody); 
                data.MaxTempAngleOffset = value;
            }
        }
    }

    //commented out because it's not in use
    /*
    [ParserTargetExternal("Body", "Ocean", "Kopernicus")]
    public class OceanBulkModulusLoader : BaseLoader
    {
        [ParserTarget("bulkModulus", Optional = true)]
        public NumericParser<Double> OceanBulkModulus
        {
            get => generatedBody.celestialBody.OceanBulkModulus();
            set
            {
                AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
                data.OceanBulkModulus = value;
            }
        }
    }
    */
}
