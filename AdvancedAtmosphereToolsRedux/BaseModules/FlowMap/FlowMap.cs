using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.FlowMap
{
    public class FlowMap : IWindProvider
    {
        public string Path;
        
        public bool useThirdChannel = false; //whether or not to use the Blue channel to add a vertical component to the winds.
        public FloatCurve AltitudeSpeedMultCurve;
        public FloatCurve X_AltitudeSpeedMultCurve;
        public FloatCurve Y_AltitudeSpeedMultCurve;
        public FloatCurve Z_AltitudeSpeedMultCurve;
        public FloatCurve WindSpeedMultiplierTimeCurve;
        public FloatCurve TrueAnomalyMultiplierCurve;
        public FloatCurve EccentricityMultiplierCurve;

        public float x_WindSpeed = 0f; //North/South or Radial
        public float y_WindSpeed = 0f; //Vertical
        public float z_WindSpeed = 0f; //East/West or Tangential

        public bool canscroll = false;
        public double scrollperiod = 0.0;

        public float timeoffset = 0f;

        public float minalt = 0f;
        public bool minentered = false;
        public float maxalt = 0f;
        public bool maxentered = false;
        public float lowerfade = 1000f;
        public float upperfade = 1000f;

        private bool CanScroll => scrollperiod != 0.0 && canscroll;

        private Texture2D flowmap;

        private int x = 0;
        private int y = 0;

        public FlowMap() { }

        public void Initialize()
        {
            flowmap = Utils.CreateTexture(Path);
            x = flowmap.width;
            y = flowmap.height;
            if (AltitudeSpeedMultCurve == null)
            {
                if (minentered && maxentered && maxalt > minalt)
                {
                    AltitudeSpeedMultCurve = Utils.CreateAltitudeCurve(minalt, maxalt, minalt - lowerfade, maxalt + upperfade);
                }
                else
                {
                    AltitudeSpeedMultCurve = Utils.CreateFlatCurve(1.0);
                }
            }
            if (X_AltitudeSpeedMultCurve == null)
            {
                X_AltitudeSpeedMultCurve = Utils.CreateFlatCurve(1.0);
            }
            if (Y_AltitudeSpeedMultCurve == null)
            {
                Y_AltitudeSpeedMultCurve = Utils.CreateFlatCurve(1.0);
            }
            if (Z_AltitudeSpeedMultCurve == null)
            {
                Z_AltitudeSpeedMultCurve = Utils.CreateFlatCurve(1.0);
            }
            if (WindSpeedMultiplierTimeCurve == null)
            {
                WindSpeedMultiplierTimeCurve = Utils.CreateFlatCurve(1.0);
            }
            if (TrueAnomalyMultiplierCurve == null)
            {
                TrueAnomalyMultiplierCurve = Utils.CreateFlatCurve(1.0);
            }
            if (EccentricityMultiplierCurve == null)
            {
                EccentricityMultiplierCurve = Utils.CreateFlatCurve(1.0);
            }
        }

        public Vector3 GetWindVector(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity)
        {
            //AltitudeSpeedMultiplierCurve cannot go below 0.
            float speedmult = Math.Max(AltitudeSpeedMultCurve.Evaluate((float)alt) * Utils.GetValAtLoopTime(WindSpeedMultiplierTimeCurve, time + timeoffset), 0.0f);
            speedmult *= TrueAnomalyMultiplierCurve.Evaluate((float)trueanomaly) * EccentricityMultiplierCurve.Evaluate((float)eccentricity);
            if (speedmult > 0.0f)
            {
                double scroll = CanScroll ? ((time / scrollperiod) * 360.0) % 360.0 : 0.0;
                //adjust longitude so the center of the map is the prime meridian for the purposes of these calculations
                double mapx = ((UtilMath.WrapAround(lon + 630.0 - scroll, 0.0, 360.0) / 360.0) * x) - 0.5;
                double mapy = (((lat + 90.0) / 180.0) * y) - 0.5;
                double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
                double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));

                //locate the four nearby points, but don't go over the poles.
                int leftx = UtilMath.WrapAround((int)Math.Truncate(mapx), 0, x);
                int topy = Utils.Clamp((int)Math.Truncate(mapy), 0, y - 1);
                int rightx = UtilMath.WrapAround(leftx + 1, 0, x);
                int bottomy = Utils.Clamp(topy + 1, 0, y - 1);

                Color[] colors = new Color[4] { flowmap.GetPixel(leftx, topy), flowmap.GetPixel(rightx, topy), flowmap.GetPixel(leftx, bottomy), flowmap.GetPixel(rightx, bottomy) };

                Span<double> windx = stackalloc double[4];
                Span<double> windy = stackalloc double[4];
                Span<double> windz = stackalloc double[4];

                for (int i = 0; i < 4; i++)
                {
                    windx[i] = (colors[i].g * 2.0) - 1.0;
                    windz[i] = (colors[i].r * 2.0) - 1.0;
                    windy[i] = useThirdChannel ? (colors[i].b * 2.0f) - 1.0f : 0.0f;
                }
                double windvecx = UtilMath.Lerp(UtilMath.Lerp(windx[0], windx[1], lerpx), UtilMath.Lerp(windx[2], windx[3], lerpx), lerpy) * x_WindSpeed * X_AltitudeSpeedMultCurve.Evaluate((float)alt) * speedmult;
                double windvecy = UtilMath.Lerp(UtilMath.Lerp(windy[0], windy[1], lerpx), UtilMath.Lerp(windy[2], windy[3], lerpx), lerpy) * y_WindSpeed * Y_AltitudeSpeedMultCurve.Evaluate((float)alt) * speedmult;
                double windvecz = UtilMath.Lerp(UtilMath.Lerp(windz[0], windz[1], lerpx), UtilMath.Lerp(windz[2], windz[3], lerpx), lerpy) * z_WindSpeed * Z_AltitudeSpeedMultCurve.Evaluate((float)alt) * speedmult;

                return new Vector3((float)windvecx, (float)windvecy, (float)windvecz);
            }
            return Vector3.zero;
        }
    }
}
