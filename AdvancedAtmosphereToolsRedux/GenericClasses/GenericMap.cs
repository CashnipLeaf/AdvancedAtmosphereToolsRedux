using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.GenericClasses
{
    public class GenericMap
    {
        private Texture2D offsetMap;

        public Texture2D OffsetMap
        {
            get => offsetMap;
            set
            {
                offsetMap = value;
                x = offsetMap.width;
                y = offsetMap.height;
            }
        }

        public double deformity = 0.0;
        public double offset = 0.0;

        public FloatCurve AltitudeMultiplierCurve = new FloatCurve(new Keyframe[3] 
        {
             new Keyframe(0f, 1f, 0f, -2.5f),
             new Keyframe(0.4f, 0f, -2.5f, 0f),
             new Keyframe(1f, 0f, 0f, 0f)
        });
        public FloatCurve TimeMultiplierCurve = AtmoToolsReduxUtils.FlatCurve(1f);
        public FloatCurve TrueAnomalyMultiplierCurve = AtmoToolsReduxUtils.FlatCurve(1f);
        public FloatCurve EccentricityMultiplierCurve = AtmoToolsReduxUtils.FlatCurve(1f);

        private bool canscroll = false;
        public bool CanScroll
        {
            get => canscroll && scrollperiod != 0.0;
            set => canscroll = value;
        }
        public double scrollperiod = 0.0;

        private int x = 0;
        private int y = 0;

        public GenericMap() { }

        public double GetValue(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity)
        {
            double multiplier = (double)(AltitudeMultiplierCurve.Evaluate((float)alt) * Utils.GetValAtLoopTime(TimeMultiplierCurve,time));
            multiplier *= TrueAnomalyMultiplierCurve.Evaluate((float)trueanomaly) * EccentricityMultiplierCurve.Evaluate((float)eccentricity);
            if (double.IsFinite(multiplier) && multiplier != 0.0)
            {
                double scroll = CanScroll ? ((time / scrollperiod) * 360.0) % 360.0 : 0.0;
                double mapx = ((UtilMath.WrapAround(lon + 630.0 - scroll, 0, 360) / 360.0) * x) - 0.5;
                double mapy = (((lat + 90.0) / 180.0) * y) - 0.5;
                double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
                double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));

                //locate the four nearby points, but don't go over the poles.
                int leftx = UtilMath.WrapAround((int)Math.Truncate(mapx), 0, x);
                int rightx = UtilMath.WrapAround(leftx + 1, 0, x);
                int topy = Utils.Clamp((int)Math.Truncate(mapy), 0, y - 1);
                int bottomy = Utils.Clamp(topy + 1, 0, y - 1);

                Color TopLeft = offsetMap.GetPixel(leftx, topy);
                Color TopRight = offsetMap.GetPixel(rightx, topy);
                Color BottomLeft = offsetMap.GetPixel(leftx, bottomy);
                Color BottomRight = offsetMap.GetPixel(rightx, bottomy);

                double value = ((UtilMath.Lerp(UtilMath.Lerp(TopLeft.grayscale, TopRight.grayscale, lerpx), UtilMath.Lerp(BottomLeft.grayscale, BottomRight.grayscale, lerpx), lerpy) * deformity) + offset) * multiplier;
                return double.IsFinite(value) ? value : 0.0;
            }

            return 0.0;
        }
    }
}
