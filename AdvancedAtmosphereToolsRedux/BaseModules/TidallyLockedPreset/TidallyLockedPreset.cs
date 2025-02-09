using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TidallyLockedPreset
{
    //would allow quickly configuring temperature, pressure, and wind for tidally locked bodies
    public sealed class TidallyLockedPreset : IFlatTemperatureModifier, IFractionalLatitudeBiasModifier, IFractionalLatitudeSunMultModifier, IFractionalPressureModifier, IWindProvider
    {
        public double substellarLongitude = 0d;
        public double substellarPressureGradient = 0d; //pressure drop at the substellar point as a fraction of the pressure at that altitude
        public double pressureGradientTerminator = 90d; //point at which to start decreasing pressure in degrees of great circle angle

        public PresetType presetType = PresetType.Slow;

        public double AngleOffset
        {
            get
            {
                if (useCustomOffset)
                {
                    return customAngleOffset;
                }
                switch (presetType)
                {
                    case PresetType.Slow:
                        return 5d;
                    case PresetType.Medium:
                        return 12d;
                    default:
                        return 0d;
                }
            }
        }

        public double customAngleOffset = 0d;
        public bool useCustomOffset = false;

        public double H_wind_speed = 0d;
        public double V_wind_speed = 0d;
        public double windTerminator = 90d;

        public double RotationalComponent
        {
            get
            {
                if (useCustomRotComp)
                {
                    return customRotationalComponent;
                }
                switch (presetType)
                {
                    case PresetType.Slow:
                        return 2d;
                    case PresetType.Medium:
                        return 6d;
                    default:
                        return 0d;
                }
            }
        }

        public double customRotationalComponent = 0d;
        public bool useCustomRotComp = false;

        public string body;

        private FloatCurve divergentAltitudeMultiplierCurve = new FloatCurve(new Keyframe[5] 
        { new Keyframe(0f, 0.3f, 0f, 8f),
          new Keyframe(0.08f, 0.8f, 0f, 0f),
          new Keyframe(0.22f, -1f, 0f, 0f),
          new Keyframe(0.40f, 0f, 0f, 0f),
          new Keyframe(1f, 0f, 0f, 0f)
        }); 

        private FloatCurve divergentRadiusMultiplierCurve = new FloatCurve(new Keyframe[5]
        { new Keyframe(0f, 0f, 0f, 10f),
          new Keyframe(0.05f, 0.9f, 0f, 0f),
          new Keyframe(0.1f, 1f, 0f, 0f),
          new Keyframe(0.6f, 0.9f, 0f, 0f),
          new Keyframe(1f, 0f, 0f, 0f)
        });

        private FloatCurve verticalRadiusMultiplierCurve = new FloatCurve(new Keyframe[3]
        { new Keyframe(0f, 1f, 0f, -25f),
          new Keyframe(0.05f, 0f, 0f, 0f),
          new Keyframe(1f, 0f, 0f, 0f)
        });

        private FloatCurve rotationalAltitudeMultiplierCurve = new FloatCurve(new Keyframe[5]
        { new Keyframe(0f, 0.6f, 0f, 6f),
          new Keyframe(0.1f, 1f, 0f, 0.2f),
          new Keyframe(0.4f, 3f, 10f, -2f),
          new Keyframe(0.6f, 0.1f, 0f, 0f),
          new Keyframe(1f, 0f, 0f, 0f)
        });

        private static float[,,] presetFastPressure;
        private static float[,,] presetFastTemperature;

        private static float[,,] presetFastWindX; //north/south
        private static float[,,] presetFastWindY; //vertical
        private static float[,,] presetFastWindZ; //east/west

        private const int LonSize = 64;
        private const int LatSize = 32;
        private const int AltSize = 10;
        private const int initialoffset = 128;

        private const double scaleFactor = 1.4;

        private const string PathToData = "AdvancedAtmosphereToolsRedux/Binary/";

        public TidallyLockedPreset(CelestialBody body)
        {
            this.body = body.name;
            if (presetFastPressure == null)
            {
                presetFastPressure = Utils.ReadBinaryFile(PathToData + "preset_fast_press.npy", LonSize, LatSize, AltSize + 1, 1, initialoffset, true)[0];
            }
            if (presetFastTemperature == null)
            {
                presetFastTemperature = Utils.ReadBinaryFile(PathToData + "preset_fast_temp.npy", LonSize, LatSize, AltSize, 1, initialoffset, true)[0];
            }
            if (presetFastWindX == null || presetFastWindY == null || presetFastWindZ == null)
            {
                presetFastWindX = Utils.ReadBinaryFile(PathToData + "preset_fast_wind_ns.npy", LonSize, LatSize, AltSize, 1, initialoffset, true)[0];
                presetFastWindY = Utils.ReadBinaryFile(PathToData + "preset_fast_wind_v.npy", LonSize, LatSize, AltSize, 1, initialoffset, true)[0];
                presetFastWindZ = Utils.ReadBinaryFile(PathToData + "preset_fast_wind_ew.npy", LonSize, LatSize, AltSize, 1, initialoffset, true)[0];
            }
        }

        public double GetFlatTemperatureModifier(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity)
        {
            if (presetType == PresetType.Fast)
            {
                CelestialBody mainbody = FlightGlobals.GetBodyByName(body);

                double modeltop = mainbody.atmosphereDepth * 0.4;
                double normalizedlon = UtilMath.WrapAround(lon + 540.0 + substellarLongitude, 0.0, 360.0) / 360.0;
                double normalizedlat = (180.0 - (lat + 90.0)) / 180.0;
                double normalizedalt = UtilMath.Clamp01(alt / modeltop);
                //derive the locations of the data in the arrays

                double mapx = UtilMath.WrapAround(normalizedlon * presetFastTemperature.GetLength(2), 0, presetFastTemperature.GetLength(2));
                double mapy = normalizedlat * presetFastTemperature.GetUpperBound(1);

                int x1 = (int)UtilMath.Clamp(Math.Truncate(mapx), 0, presetFastTemperature.GetUpperBound(2));
                int x2 = UtilMath.WrapAround(x1 + 1, 0, presetFastTemperature.GetLength(2));

                int y1 = Utils.Clamp((int)Math.Floor(mapy), 0, presetFastTemperature.GetUpperBound(1));
                int y2 = Utils.Clamp(y1 + 1, 0, presetFastTemperature.GetUpperBound(1));

                double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
                double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));
                double lerpz = Utils.ScaleAltitude(normalizedalt, scaleFactor, presetFastTemperature.GetUpperBound(0), out int z1, out int z2);

                float BottomPlane = Utils.BiLerp(presetFastTemperature[z1, y1, x1], presetFastTemperature[z1, y1, x2], presetFastTemperature[z1, y2, x1], presetFastTemperature[z1, y2, x2], (float)lerpx, (float)lerpy);
                float TopPlane = Utils.BiLerp(presetFastTemperature[z2, y1, x1], presetFastTemperature[z2, y1, x2], presetFastTemperature[z2, y2, x1], presetFastTemperature[z2, y2, x2], (float)lerpx, (float)lerpy);

                double multiplier = UtilMath.Lerp((double)BottomPlane, (double)TopPlane, lerpz);

                return mainbody.latitudeTemperatureSunMultCurve.Evaluate(0) * multiplier * (double)mainbody.atmosphereTemperatureSunMultCurve.Evaluate((float)alt);
            }
            else
            {
                return 0.0;
            }
        }

        public double GetFractionalLatitudeBiasModifier(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity) => presetType == PresetType.Fast ? -1.0 : 0.0;

        public double GetFractionalLatitudeSunMultModifier(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity) => presetType == PresetType.Fast ? -1.0 : 0.0;

        public double GetFractionalPressureModifier(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity)
        {
            if (presetType == PresetType.Fast)
            {
                CelestialBody mainbody = FlightGlobals.GetBodyByName(body);

                double modeltop = mainbody.atmosphereDepth * 0.4;
                double normalizedlon = UtilMath.WrapAround(lon + 540.0 + substellarLongitude, 0.0, 360.0) / 360.0;
                double normalizedlat = (180.0 - (lat + 90.0)) / 180.0;
                double normalizedalt = UtilMath.Clamp01(alt / modeltop);
                //derive the locations of the data in the arrays

                double mapx = UtilMath.WrapAround(normalizedlon * presetFastPressure.GetLength(2), 0, presetFastPressure.GetLength(2));
                double mapy = normalizedlat * presetFastPressure.GetUpperBound(1);

                int x1 = (int)UtilMath.Clamp(Math.Truncate(mapx), 0, presetFastPressure.GetUpperBound(2));
                int x2 = UtilMath.WrapAround(x1 + 1, 0, presetFastPressure.GetLength(2));

                int y1 = Utils.Clamp((int)Math.Floor(mapy), 0, presetFastPressure.GetUpperBound(1));
                int y2 = Utils.Clamp(y1 + 1, 0, presetFastPressure.GetUpperBound(1));

                double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
                double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));
                double lerpz = Utils.ScaleAltitude(normalizedalt, scaleFactor, presetFastPressure.GetUpperBound(0), out int z1, out int z2);

                float BottomPlane = Utils.BiLerp(presetFastPressure[z1, y1, x1], presetFastPressure[z1, y1, x2], presetFastPressure[z1, y2, x1], presetFastPressure[z1, y2, x2], (float)lerpx, (float)lerpy);
                float TopPlane = Utils.BiLerp(presetFastPressure[z2, y1, x1], presetFastPressure[z2, y1, x2], presetFastPressure[z2, y2, x1], presetFastPressure[z2, y2, x2], (float)lerpx, (float)lerpy);

                return UtilMath.Lerp((double)BottomPlane, (double)TopPlane, lerpz);
            }
            else
            {
                double distancetosubstellar = AtmoToolsReduxUtils.GreatCircleAngle(lon, lat, substellarLongitude + AngleOffset, 0d);

                double distancefraction = UtilMath.Clamp01(distancetosubstellar / pressureGradientTerminator);

                return UtilMath.Lerp(-1 * substellarPressureGradient, 0d, distancefraction);
            }
        }

        public Vector3 GetWindVector(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);

            if (presetType == PresetType.Fast)
            {
                double modeltop = mainbody.atmosphereDepth * 0.4;
                double normalizedlon = UtilMath.WrapAround(lon + 540.0 + substellarLongitude, 0.0, 360.0) / 360.0;
                double normalizedlat = (180.0 - (lat + 90.0)) / 180.0;
                double normalizedalt = UtilMath.Clamp01(alt / modeltop);
                //derive the locations of the data in the arrays

                double mapx = UtilMath.WrapAround(normalizedlon * presetFastWindX.GetLength(2), 0, presetFastWindX.GetLength(2));
                double mapy = normalizedlat * presetFastWindX.GetUpperBound(1);

                int x1 = (int)UtilMath.Clamp(Math.Truncate(mapx), 0, presetFastWindX.GetUpperBound(2));
                int x2 = UtilMath.WrapAround(x1 + 1, 0, presetFastWindX.GetLength(2));

                int y1 = Utils.Clamp((int)Math.Floor(mapy), 0, presetFastWindX.GetUpperBound(1));
                int y2 = Utils.Clamp(y1 + 1, 0, presetFastWindX.GetUpperBound(1));

                double lerpx = UtilMath.Clamp01(mapx - Math.Truncate(mapx));
                double lerpy = UtilMath.Clamp01(mapy - Math.Truncate(mapy));
                double lerpz = Utils.ScaleAltitude(normalizedalt, scaleFactor, presetFastWindX.GetUpperBound(0), out int z1, out int z2);

                //Bilinearly interpolate on the longitude and latitude axes 
                float BottomPlaneX1 = Utils.BiLerp(presetFastWindX[z1, y1, x1], presetFastWindX[z1, y1, x2], presetFastWindX[z1, y2, x1], presetFastWindX[z1, y2, x2], (float)lerpx, (float)lerpy);
                float TopPlaneX1 = Utils.BiLerp(presetFastWindX[z2, y1, x1], presetFastWindX[z2, y1, x2], presetFastWindX[z2, y2, x1], presetFastWindX[z2, y2, x2], (float)lerpx, (float)lerpy);

                float BottomPlaneY1 = Utils.BiLerp(presetFastWindY[z1, y1, x1], presetFastWindY[z1, y1, x2], presetFastWindY[z1, y2, x1], presetFastWindY[z1, y2, x2], (float)lerpx, (float)lerpy);
                float TopPlaneY1 = Utils.BiLerp(presetFastWindY[z2, y1, x1], presetFastWindY[z2, y1, x2], presetFastWindY[z2, y2, x1], presetFastWindY[z2, y2, x2], (float)lerpx, (float)lerpy);

                float BottomPlaneZ1 = Utils.BiLerp(presetFastWindZ[z1, y1, x1], presetFastWindZ[z1, y1, x2], presetFastWindZ[z1, y2, x1], presetFastWindZ[z1, y2, x2], (float)lerpx, (float)lerpy);
                float TopPlaneZ1 = Utils.BiLerp(presetFastWindZ[z2, y1, x1], presetFastWindZ[z2, y1, x2], presetFastWindZ[z2, y2, x1], presetFastWindZ[z2, y2, x2], (float)lerpx, (float)lerpy);

                Vector3 windvec = Vector3.zero;
                //Bilinearly interpolate on the altitude and time axes to create the wind vector
                windvec.x = Mathf.Lerp(BottomPlaneX1, TopPlaneX1, (float)lerpz) * (float)H_wind_speed;
                windvec.y = Mathf.Lerp(BottomPlaneY1, TopPlaneY1, (float)lerpz) * (float)V_wind_speed;
                windvec.z = Mathf.Lerp(BottomPlaneZ1, TopPlaneZ1, (float)lerpz) * (float)H_wind_speed;

                if (alt > modeltop)
                {
                    double altabovetop = (alt - modeltop) / (mainbody.atmosphereDepth - modeltop);
                    windvec = Vector3.Lerp(windvec, Vector3.zero, (float)(altabovetop * altabovetop));
                }

                return windvec.IsFinite() ? windvec : Vector3.zero;
            }
            else
            {
                double distancetosubstellar = AtmoToolsReduxUtils.GreatCircleAngle(lon, lat, substellarLongitude + AngleOffset, 0d);
                double altfraction = UtilMath.Clamp01(alt / mainbody.atmosphereDepth);

                double distancefraction = UtilMath.Clamp01(distancetosubstellar / windTerminator);

                // rotational component
                double loncomp = RotationalComponent * Math.Cos(lat * UtilMath.Deg2Rad);

                Vector3 rotational = new Vector3(0f, 0f, (float)loncomp);
                if (presetType == PresetType.Medium)
                {
                    rotational *= rotationalAltitudeMultiplierCurve.Evaluate((float)altfraction);
                }

                //diverging component
                double heading = AtmoToolsReduxUtils.RelativeHeading(lon, lat, substellarLongitude, 0, true);
                Vector3 divergent = new Vector3((float)(H_wind_speed * Math.Sin(heading)), 0f, (float)(H_wind_speed * Math.Cos(heading)));
                divergent *= divergentAltitudeMultiplierCurve.Evaluate((float)altfraction) * divergentRadiusMultiplierCurve.Evaluate((float)distancefraction);

                //vertical component
                double altfraction_vwind = UtilMath.Clamp01(alt / mainbody.atmosphereDepth) * 2.5;
                double verticalcomponent = UtilMath.Lerp(V_wind_speed, 0.0, altfraction_vwind * altfraction_vwind);
                Vector3 vertical = new Vector3(0f, (float)verticalcomponent, 0f) * verticalRadiusMultiplierCurve.Evaluate((float)distancefraction);

                return rotational + vertical + divergent;
            }
        }

        public enum PresetType
        {
            Slow,
            Medium,
            Fast
        }
    }
}
