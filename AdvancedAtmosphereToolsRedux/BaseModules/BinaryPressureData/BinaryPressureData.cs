using System;
using AdvancedAtmosphereToolsRedux.Interfaces;

namespace AdvancedAtmosphereToolsRedux.BaseModules.BinaryPressureData
{
    public class BinaryPressureData : IBasePressure
    {
        private CelestialBody Body;

        public int sizeLon;
        public int sizeLat;
        public int sizeAlt;
        public int timestamps;
        public bool invertalt = false;
        public int initialoffset = 0;
        public double ScaleFactor = double.NaN;
        public double TimeStep = double.NaN;
        public double modeltop = 0.0;
        public double LonOffset = 0.0;
        public double TimeOffset = 0.0;

        public string Path;

        public float[][,,] PressData;

        public BinaryPressureData() { }

        public void Initialize(CelestialBody body)
        {
            Body = body;
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException();
            }
            PressData = Utils.ReadBinaryFile(Path, sizeLon, sizeLat, sizeAlt, timestamps, initialoffset, invertalt);
        }

        public double GetBasePressure(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity)
        {
            double truetop = Math.Min(modeltop, Body.atmosphereDepth);
            double normalizedlon = UtilMath.WrapAround(lon + 360.0 - LonOffset, 0.0, 360.0) / 360.0;
            double normalizedlat = (180.0 - (lat + 90.0)) / 180.0;
            double normalizedalt = UtilMath.Clamp01(alt / truetop);
            int timeindex = UtilMath.WrapAround((int)Math.Floor((time + TimeOffset) / TimeStep), 0, PressData.GetLength(0));
            int timeindex2 = (timeindex + 1) % PressData.GetLength(0);

            //derive the locations of the data in the arrays
            double mapx = UtilMath.WrapAround(normalizedlon * PressData[timeindex].GetLength(2), 0.0, PressData[timeindex].GetLength(2));
            double mapy = normalizedlat * PressData[timeindex].GetUpperBound(1);

            int x1 = (int)UtilMath.Clamp(Math.Truncate(mapx), 0, PressData[timeindex].GetUpperBound(2));
            int x2 = UtilMath.WrapAround(x1 + 1, 0, PressData[timeindex].GetLength(2));

            int y1 = Utils.Clamp((int)Math.Floor(mapy), 0, PressData[timeindex].GetUpperBound(1));
            int y2 = Utils.Clamp(y1 + 1, 0, PressData[timeindex].GetUpperBound(1));

            double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
            double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));
            double lerpz = Utils.ScaleAltitude(normalizedalt, ScaleFactor, PressData[timeindex].GetUpperBound(0), out int z1, out int z2);
            double lerpt = UtilMath.Clamp01((time % TimeStep) / TimeStep);

            //Bilinearly interpolate on the longitude and latitude axes
            float BottomPlane1 = Utils.BiLerp(PressData[timeindex][z1, y1, x1], PressData[timeindex][z1, y1, x2], PressData[timeindex][z1, y2, x1], PressData[timeindex][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlane1 = Utils.BiLerp(PressData[timeindex][z2, y1, x1], PressData[timeindex][z2, y1, x2], PressData[timeindex][z2, y2, x1], PressData[timeindex][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlane2 = Utils.BiLerp(PressData[timeindex2][z1, y1, x1], PressData[timeindex2][z1, y1, x2], PressData[timeindex2][z1, y2, x1], PressData[timeindex2][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlane2 = Utils.BiLerp(PressData[timeindex2][z2, y1, x1], PressData[timeindex2][z2, y1, x2], PressData[timeindex2][z2, y2, x1], PressData[timeindex2][z2, y2, x2], (float)lerpx, (float)lerpy);

            //Linearly interpolate on the time axis
            double BottomPlaneFinal = UtilMath.Lerp((double)BottomPlane1, (double)BottomPlane2, lerpt);
            double TopPlaneFinal = UtilMath.Lerp((double)TopPlane1, (double)TopPlane2, lerpt);

            double pressure = Utils.InterpolatePressure(BottomPlaneFinal, TopPlaneFinal, lerpz) * 0.001;
            if (alt > truetop)
            {
                double extralerp = (alt - truetop) / (Body.atmosphereDepth - truetop);
                double press0 = Body.GetPressure(0);
                double press1 = Body.GetPressure(truetop);
                double scaleheight = truetop / Math.Log(press0 / press1, Math.E);
                return UtilMath.Lerp(pressure * Math.Pow(Math.E, -((alt - truetop) / scaleheight)), Body.GetPressure(alt), Math.Pow(extralerp, 0.125));
            }
            else
            {
                return pressure;
            }
        }
    }
}
