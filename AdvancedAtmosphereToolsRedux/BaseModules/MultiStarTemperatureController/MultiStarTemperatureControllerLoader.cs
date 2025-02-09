using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.MultiStarTemperatureController
{
    [ParserTargetExternal("Atmosphere", "MultiStarTemperatureController", "Kopernicus")]
    public class MultiStarTemperatureControllerLoader : BaseLoader, IParserPostApplyEventSubscriber, ITypeParser<MultiStarTemperatureController>
    {
        public MultiStarTemperatureController Value { get; set; }

        public MultiStarTemperatureControllerLoader() => Value = new MultiStarTemperatureController(generatedBody.celestialBody);

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            ConfigNode[] starnodes = node.GetNodes("TemperatureController");
            foreach (ConfigNode starnode in starnodes)
            {
                Value.AddStar(starnode);
            }

            AtmoToolsRedux_Data.SetBaseTemperature(Value, generatedBody.celestialBody);
        }
    }
}
