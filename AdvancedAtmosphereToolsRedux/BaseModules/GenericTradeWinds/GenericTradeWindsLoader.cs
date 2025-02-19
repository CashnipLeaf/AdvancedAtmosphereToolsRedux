using System;
using System.Collections.Generic;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GenericTradeWinds
{
    [ParserTargetExternal("Atmosphere", "WindObject", "Kopernicus")]
    internal class GenericTradeWindsLoader : BaseLoader, ITypeParser<GenericTradeWinds>, IParserPostApplyEventSubscriber
    {
        public GenericTradeWinds Value { get; set; }

        public GenericTradeWindsLoader() => Value = new GenericTradeWinds();

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            AtmoToolsRedux_Data.AddWindProvider(Value, generatedBody.celestialBody);
        }
    }
}
