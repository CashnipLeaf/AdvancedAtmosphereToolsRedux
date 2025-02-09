using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TidallyLockedPreset
{
    [ParserTargetExternal("Atmosphere", "TidallyLockedPreset", "Kopernicus")]
    public class TidallyLockedPresetLoader : BaseLoader, ITypeParser<TidallyLockedPreset>, IParserPostApplyEventSubscriber
    {
        public TidallyLockedPreset Value { get; set; }
        
        public TidallyLockedPresetLoader() => Value = new TidallyLockedPreset(generatedBody.celestialBody);

        void IParserPostApplyEventSubscriber.PostApply(ConfigNode node)
        {
            AtmoToolsRedux_Data.AddWindProvider(Value, generatedBody.celestialBody);
            AtmoToolsRedux_Data.AddFractionalPressureModifier(Value, generatedBody.celestialBody);
            AtmoToolsRedux_Data.AddFlatTemperatureModifier(Value, generatedBody.celestialBody);
            AtmoToolsRedux_Data.AddFractionalLatitudeBiasModifier(Value, generatedBody.celestialBody);
            AtmoToolsRedux_Data.AddFractionalLatitudeSunMultModifier(Value, generatedBody.celestialBody);
        }

        [ParserTarget("substellarLongitude")]
        public NumericParser<Double> SubstellarLongitude
        {
            get => Value.substellarLongitude;
            set => Value.substellarLongitude = value;
        }

        [ParserTarget("substellarPressureGradient")]
        public NumericParser<Double> SubstellarPressureGradient
        {
            get => Value.substellarPressureGradient;
            set => Value.substellarPressureGradient = value;
        }

        [ParserTarget("pressureGradientTerminator", Optional = true)]
        public NumericParser<Double> PressureGradientTerminator
        {
            get => Value.pressureGradientTerminator;
            set => Value.pressureGradientTerminator = value;
        }

        [ParserTarget("presetType", Optional = true)]
        public EnumParser<TidallyLockedPreset.PresetType> TidallyLockedPresetType
        {
            get => Value.presetType;
            set => Value.presetType = value;
        }

        [ParserTarget("maxTempAngleOffset", Optional = true)]
        public NumericParser<Double> CustomOffsetAngle
        {
            get => Value.customAngleOffset;
            set
            {
                Value.customAngleOffset = value;
                Value.useCustomOffset = true;
            }
        }

        [ParserTarget("horizontalWindSpeed", Optional = true)]
        public NumericParser<Double> H_WindSpeed
        {
            get => Value.H_wind_speed;
            set => Value.H_wind_speed = value;
        }

        [ParserTarget("verticalWindSpeed", Optional = true)]
        public NumericParser<Double> V_WindSpeed
        {
            get => Value.V_wind_speed;
            set => Value.V_wind_speed = value;
        }

        [ParserTarget("windTerminator", Optional = true)]
        public NumericParser<Double> WindTerminator
        {
            get => Value.windTerminator;
            set => Value.windTerminator = value;
        }

        [ParserTarget("windRotationalComponent", Optional = true)]
        public NumericParser<Double> RotationalComponent
        {
            get => Value.customRotationalComponent;
            set
            {
                Value.customRotationalComponent = value;
                Value.useCustomRotComp = true;
            }
        }
    }
}
