using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.AdiabaticIndexCurve
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class AdiabaticIndexCurveLoader : BaseLoader
    {
        private AdiabaticIndexCurve adiabaticIndexCurve;

        public AdiabaticIndexCurveLoader() { }

        [ParserTargetCollection("AdiabaticIndexCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> BaseAdiabaticIndexCurve
        {
            get => adiabaticIndexCurve != null ? Utility.FloatCurveToList(adiabaticIndexCurve.BaseAdiabaticIndexCurve) : null;
            set
            {
                adiabaticIndexCurve = new AdiabaticIndexCurve
                {
                    BaseAdiabaticIndexCurve = Utility.ListToFloatCurve(value)
                };
                AtmoToolsRedux_Data.SetBaseAdiabaticIndex(adiabaticIndexCurve, generatedBody.celestialBody);
            }
        }
    }
}
