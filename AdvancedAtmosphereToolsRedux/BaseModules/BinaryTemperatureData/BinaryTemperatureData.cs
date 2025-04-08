using System;
using AdvancedAtmosphereToolsRedux.Interfaces;

namespace AdvancedAtmosphereToolsRedux.BaseModules.BinaryTemperatureData
{
    public class BinaryTemperatureData : IBaseTemperature
    {
        private bool disablelatbias = false;
        private bool disablelatsunmult = false;
        private bool disableaxialsunbias = false;
        private bool disableeccentricitybias = false;

        private string body;

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

        public float[][,,] TempData;

        public bool DisableLatitudeBias
        {
            get => disablelatbias;
            set => disablelatbias = value;
        }

        public bool DisableLatitudeSunMult
        {
            get => disablelatsunmult;
            set => disablelatsunmult = value;
        }

        public bool DisableAxialSunBias
        {
            get => disableaxialsunbias;
            set => disableaxialsunbias = value;
        }

        public bool DisableEccentricityBias
        {
            get => disableeccentricitybias;
            set => disableeccentricitybias = value;
        }

        public BinaryTemperatureData(CelestialBody body) => this.body = body.name;

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException();
            }
            TempData = Utils.ReadBinaryFile(Path, sizeLon, sizeLat, sizeAlt, timestamps, initialoffset, invertalt);
        }

        public double GetBaseTemperature(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody currentbody = FlightGlobals.GetBodyByName(body);

            double truetop = Math.Min(modeltop, currentbody.atmosphereDepth);
            double normalizedlon = UtilMath.WrapAround(lon + 360.0 - LonOffset, 0.0, 360.0) / 360.0;
            double normalizedlat = (180.0 - (lat + 90.0)) / 180.0;
            double normalizedalt = UtilMath.Clamp01(alt / truetop);
            int timeindex = UtilMath.WrapAround((int)Math.Floor((time + TimeOffset) / TimeStep), 0, TempData.GetLength(0));
            int timeindex2 = (timeindex + 1) % TempData.GetLength(0);

            //derive the locations of the data in the arrays
            double mapx = UtilMath.WrapAround(normalizedlon * TempData[timeindex].GetLength(2), 0, TempData[timeindex].GetLength(2));
            double mapy = normalizedlat * TempData[timeindex].GetUpperBound(1);

            int x1 = (int)UtilMath.Clamp(Math.Truncate(mapx), 0, TempData[timeindex].GetUpperBound(2));
            int x2 = UtilMath.WrapAround(x1 + 1, 0, TempData[timeindex].GetLength(2));

            int y1 = Utils.Clamp((int)Math.Floor(mapy), 0, TempData[timeindex].GetUpperBound(1));
            int y2 = Utils.Clamp(y1 + 1, 0, TempData[timeindex].GetUpperBound(1));

            double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
            double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));
            double lerpz = Utils.ScaleAltitude(normalizedalt, ScaleFactor, TempData[timeindex].GetUpperBound(0), out int z1, out int z2);
            double lerpt = UtilMath.Clamp01((time % TimeStep) / TimeStep);

            //Bilinearly interpolate on the longitude and latitude axes
            float BottomPlane1 = Utils.BiLerp(TempData[timeindex][z1, y1, x1], TempData[timeindex][z1, y1, x2], TempData[timeindex][z1, y2, x1], TempData[timeindex][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlane1 = Utils.BiLerp(TempData[timeindex][z2, y1, x1], TempData[timeindex][z2, y1, x2], TempData[timeindex][z2, y2, x1], TempData[timeindex][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlane2 = Utils.BiLerp(TempData[timeindex2][z1, y1, x1], TempData[timeindex2][z1, y1, x2], TempData[timeindex2][z1, y2, x1], TempData[timeindex2][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlane2 = Utils.BiLerp(TempData[timeindex2][z2, y1, x1], TempData[timeindex2][z2, y1, x2], TempData[timeindex2][z2, y2, x1], TempData[timeindex2][z2, y2, x2], (float)lerpx, (float)lerpy);

            //Bilinearly interpolate on the altitude and time axes
            double temp = UtilMath.Lerp(UtilMath.Lerp((double)BottomPlane1, (double)TopPlane1, lerpz), UtilMath.Lerp((double)BottomPlane2, (double)TopPlane2, lerpz), lerpt);
            if (alt > truetop)
            {
                AtmoToolsReduxUtils.GetTemperatureWithComponents(currentbody, lon, lat, alt, trueAnomaly, eccentricity, out double basetemp, out double latbias, out double latsunmult, out double axialbias, out double eccentricitybias);
                double tempoffset = 0.0;
                if (DisableLatitudeBias)
                {
                    tempoffset += latbias;
                }
                if (DisableLatitudeSunMult)
                {
                    tempoffset += latsunmult;
                }
                if (DisableAxialSunBias)
                {
                    tempoffset += axialbias;
                }
                if (DisableEccentricityBias)
                {
                    tempoffset += eccentricitybias;
                }
                tempoffset *= (double)currentbody.atmosphereTemperatureSunMultCurve.Evaluate((float)alt);
                double extralerp = (alt - truetop) / (currentbody.atmosphereDepth - truetop);
                return UtilMath.Lerp(temp, basetemp + tempoffset, Math.Sqrt(extralerp));
            }
            else
            {
                return temp;
            }
        }
    }
}
