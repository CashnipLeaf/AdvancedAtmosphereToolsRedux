using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.MolarMassCurve
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class MolarMassCurveLoader : BaseLoader
    {
        private MolarMassCurve molarMassCurve;
        
        public MolarMassCurveLoader() { }

        [ParserTargetCollection("MolarMassCurve", Key = "key", NameSignificance = NameSignificance.Key, Optional = true)]
        public List<NumericCollectionParser<Single>> BaseMolarMassCurve
        {
            get => molarMassCurve != null ? Utility.FloatCurveToList(molarMassCurve.BaseMolarMassCurve) : null;
            set
            {
                molarMassCurve = new MolarMassCurve
                {
                    BaseMolarMassCurve = Utility.ListToFloatCurve(value)
                };
                AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
                data.SetBaseMolarMass(molarMassCurve);
            }
        }
    }
}
