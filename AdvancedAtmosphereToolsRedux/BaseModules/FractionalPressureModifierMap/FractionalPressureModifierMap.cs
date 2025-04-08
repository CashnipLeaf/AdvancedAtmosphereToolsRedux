using AdvancedAtmosphereToolsRedux.Interfaces;
using AdvancedAtmosphereToolsRedux.GenericClasses;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.FractionalPressureModifierMap
{
    public class FractionalPressureModifierMap : IFractionalPressureModifier
    {
        public GenericMap map;

        public double PressureModifierRange
        {
            get => map.deformity;
            set => map.deformity = value;
        }

        public double PressureModifierOffset
        {
            get => map.offset;
            set => map.offset = value;
        }

        public FloatCurve AltitudeMultiplierCurve
        {
            get => map.AltitudeMultiplierCurve;
            set => map.AltitudeMultiplierCurve = value;
        }

        public FloatCurve TimeMultiplierCurve
        {
            get => map.TimeMultiplierCurve;
            set => map.TimeMultiplierCurve = value;
        }

        public FloatCurve EccentricityMultiplierCurve
        {
            get => map.EccentricityMultiplierCurve;
            set => map.EccentricityMultiplierCurve = value;
        }

        public FloatCurve TrueAnomalyMultiplierCurve
        {
            get => map.TrueAnomalyMultiplierCurve; 
            set => map.TrueAnomalyMultiplierCurve = value;
        }

        public double ScrollPeriod
        {
            get => map.scrollperiod;
            set
            {
                map.scrollperiod = value;
                map.CanScroll = true;
            }
        }

        public string FilePath;
        public void SetTexture(string path)
        {
            FilePath = path;
            Texture2D texture = Utils.CreateTexture(path);
            map.OffsetMap = texture;
        }

        public FractionalPressureModifierMap() => map = new GenericMap();

        public void Initialize() { }

        public double GetFractionalPressureModifier(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity) => map.GetValue(lon, lat, alt, time, trueAnomaly, eccentricity);
    }
}
