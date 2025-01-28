using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GenericUnsafeAtmosphere
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    public class GenericUnsafeAtmosphereLoader : BaseLoader, ITypeParser<GenericUnsafeAtmosphere>
    {
        public GenericUnsafeAtmosphere Value { get; set; }

        public GenericUnsafeAtmosphereLoader()
        {
            Value = new GenericUnsafeAtmosphere();
        }

        [ParserTarget("isAtmosphereUnsafe", Optional = true)]
        public NumericParser<Boolean> IsAtmoUnsafe
        {
            get => Value.IsAtmoUnsafe;
            set
            {
                if (value)
                {
                    Value.IsAtmoUnsafe = true;
                    AtmosphereData data = AtmosphereData.GetOrCreateAtmosphereData(generatedBody.celestialBody);
                    data.SetUnsafeAtmosphereIndicator(Value);
                }
            }
        }

        [ParserTarget("unsafeAtmosphereMessage", Optional = true)]
        public string UnsafeAtmosphereMessage
        {
            get => Value?.UnsafeAtmosphereMessage;
            set => Value.UnsafeAtmosphereMessage = value;
        }
    }
}
