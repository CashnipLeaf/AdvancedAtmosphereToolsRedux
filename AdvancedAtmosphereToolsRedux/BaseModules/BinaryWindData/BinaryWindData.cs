using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.BinaryWindData
{
    public class BinaryWindData : IWindProvider
    {
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
        public double EastWestWindMultiplier = 1.0;
        public double NorthSouthWindMultiplier = 1.0;
        public double VerticalWindMultiplier = 1.0;

        public string PathX;
        public string PathY;
        public string PathZ;

        public float[][,,] WindDataX; //north/south
        public float[][,,] WindDataY; //vertical
        public float[][,,] WindDataZ; //east/west

        public CelestialBody Body
        {
            get => FlightGlobals.GetBodyByName(body);
            set => body = value.name;
        }

        private string body;

        public BinaryWindData(CelestialBody body) => Body = body;

        public void Initialize()
        {
            if (string.IsNullOrEmpty(PathX) || string.IsNullOrEmpty(PathY) || string.IsNullOrEmpty(PathZ))
            {
                throw new ArgumentNullException();
            }
            WindDataX = Utils.ReadBinaryFile(PathX, sizeLon, sizeLat, sizeAlt, timestamps, initialoffset, invertalt);
            WindDataY = Utils.ReadBinaryFile(PathY, sizeLon, sizeLat, sizeAlt, timestamps, initialoffset, invertalt);
            WindDataZ = Utils.ReadBinaryFile(PathZ, sizeLon, sizeLat, sizeAlt, timestamps, initialoffset, invertalt);
        }

        public Vector3 GetWindVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            double normalizedlon = UtilMath.WrapAround(longitude + 360.0 - LonOffset, 0.0, 360.0) / 360.0;
            double normalizedlat = (180.0 - (latitude + 90.0)) / 180.0;
            double normalizedalt = UtilMath.Clamp01(altitude / modeltop);
            int timeindex = UtilMath.WrapAround((int)Math.Floor((time + TimeOffset) / timestamps), 0, WindDataX.GetLength(0));
            int timeindex2 = (timeindex + 1) % WindDataX.GetLength(0);
            //derive the locations of the data in the arrays

            double mapx = UtilMath.WrapAround(normalizedlon * WindDataX[timeindex].GetLength(2), 0, WindDataX[timeindex].GetLength(2));
            double mapy = normalizedlat * WindDataX[timeindex].GetUpperBound(1);

            int x1 = (int)UtilMath.Clamp(Math.Truncate(mapx), 0, WindDataX[timeindex].GetUpperBound(2));
            int x2 = UtilMath.WrapAround(x1 + 1, 0, WindDataX[timeindex].GetLength(2));

            int y1 = Utils.Clamp((int)Math.Floor(mapy), 0, WindDataX[timeindex].GetUpperBound(1));
            int y2 = Utils.Clamp(y1 + 1, 0, WindDataX[timeindex].GetUpperBound(1));

            double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
            double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));
            double lerpz = Utils.ScaleAltitude(normalizedalt, ScaleFactor, WindDataX[timeindex].GetUpperBound(0), out int z1, out int z2);
            double lerpt = UtilMath.Clamp01((time % TimeStep) / TimeStep);

            //Bilinearly interpolate on the longitude and latitude axes 
            float BottomPlaneX1 = Utils.BiLerp(WindDataX[timeindex][z1, y1, x1], WindDataX[timeindex][z1, y1, x2], WindDataX[timeindex][z1, y2, x1], WindDataX[timeindex][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlaneX1 = Utils.BiLerp(WindDataX[timeindex][z2, y1, x1], WindDataX[timeindex][z2, y1, x2], WindDataX[timeindex][z2, y2, x1], WindDataX[timeindex][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlaneX2 = Utils.BiLerp(WindDataX[timeindex2][z1, y1, x1], WindDataX[timeindex2][z1, y1, x2], WindDataX[timeindex2][z1, y2, x1], WindDataX[timeindex2][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlaneX2 = Utils.BiLerp(WindDataX[timeindex2][z2, y1, x1], WindDataX[timeindex2][z2, y1, x2], WindDataX[timeindex2][z2, y2, x1], WindDataX[timeindex2][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlaneY1 = Utils.BiLerp(WindDataY[timeindex][z1, y1, x1], WindDataY[timeindex][z1, y1, x2], WindDataY[timeindex][z1, y2, x1], WindDataY[timeindex][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlaneY1 = Utils.BiLerp(WindDataY[timeindex][z2, y1, x1], WindDataY[timeindex][z2, y1, x2], WindDataY[timeindex][z2, y2, x1], WindDataY[timeindex][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlaneY2 = Utils.BiLerp(WindDataY[timeindex2][z1, y1, x1], WindDataY[timeindex2][z1, y1, x2], WindDataY[timeindex2][z1, y2, x1], WindDataY[timeindex2][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlaneY2 = Utils.BiLerp(WindDataY[timeindex2][z2, y1, x1], WindDataY[timeindex2][z2, y1, x2], WindDataY[timeindex2][z2, y2, x1], WindDataY[timeindex2][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlaneZ1 = Utils.BiLerp(WindDataZ[timeindex][z1, y1, x1], WindDataZ[timeindex][z1, y1, x2], WindDataZ[timeindex][z1, y2, x1], WindDataZ[timeindex][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlaneZ1 = Utils.BiLerp(WindDataZ[timeindex][z2, y1, x1], WindDataZ[timeindex][z2, y1, x2], WindDataZ[timeindex][z2, y2, x1], WindDataZ[timeindex][z2, y2, x2], (float)lerpx, (float)lerpy);

            float BottomPlaneZ2 = Utils.BiLerp(WindDataZ[timeindex2][z1, y1, x1], WindDataZ[timeindex2][z1, y1, x2], WindDataZ[timeindex2][z1, y2, x1], WindDataZ[timeindex2][z1, y2, x2], (float)lerpx, (float)lerpy);
            float TopPlaneZ2 = Utils.BiLerp(WindDataZ[timeindex2][z2, y1, x1], WindDataZ[timeindex2][z2, y1, x2], WindDataZ[timeindex2][z2, y2, x1], WindDataZ[timeindex2][z2, y2, x2], (float)lerpx, (float)lerpy);

            Vector3 windvec = Vector3.zero;
            //Bilinearly interpolate on the altitude and time axes to create the wind vector
            windvec.x = Mathf.Lerp(Mathf.Lerp(BottomPlaneX1, TopPlaneX1, (float)lerpz), Mathf.Lerp(BottomPlaneX2, TopPlaneX2, (float)lerpz), (float)lerpt) * (float)NorthSouthWindMultiplier;
            windvec.y = Mathf.Lerp(Mathf.Lerp(BottomPlaneY1, TopPlaneY1, (float)lerpz), Mathf.Lerp(BottomPlaneY2, TopPlaneY2, (float)lerpz), (float)lerpt) * (float)VerticalWindMultiplier;
            windvec.z = Mathf.Lerp(Mathf.Lerp(BottomPlaneZ1, TopPlaneZ1, (float)lerpz), Mathf.Lerp(BottomPlaneZ2, TopPlaneZ2, (float)lerpz), (float)lerpt) * (float)EastWestWindMultiplier;

            return windvec.IsFinite() ? windvec : Vector3.zero;
        }
    }
}
