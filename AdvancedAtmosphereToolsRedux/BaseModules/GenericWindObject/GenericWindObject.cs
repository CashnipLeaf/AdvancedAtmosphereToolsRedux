﻿using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GenericWindObject
{
    public class GenericWindObject : IWindProvider
    {
        public double radius = 1.0;
        public bool useCylindricalFrame = false;

        public double longitudeCenter = 0.0;
        public FloatCurve LongitudeCenterTimeCurve;
        public double latitudeCenter = 0.0;
        public FloatCurve LatitudeCenterTimeCurve;

        public float x_WindSpeed = 0f; //North/South or Radial
        public float y_WindSpeed = 0f; //Vertical
        public float z_WindSpeed = 0f; //East/West or Tangential

        public FloatCurve RadiusSpeedMultCurve;
        public FloatCurve AltitudeSpeedMultCurve;
        public FloatCurve X_AltitudeSpeedMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) }); 
        public FloatCurve Y_AltitudeSpeedMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) }); 
        public FloatCurve Z_AltitudeSpeedMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) }); 
        public FloatCurve WindSpeedMultiplierTimeCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) });
        public FloatCurve TrueAnomalyMultiplierCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) });
        public FloatCurve EccentricityMultiplierCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) });

        public float timeoffset = 0f;

        public float minalt = 0f;
        public bool minentered = false;
        public float maxalt = 0f;
        public bool maxentered = false;
        public float lowerfade = 1000f;
        public float upperfade = 1000f;

        public GenericWindObject() { }

        public void Initialize()
        {
            if (AltitudeSpeedMultCurve == null)
            {
                if (minentered && maxentered && maxalt > minalt)
                {
                    AltitudeSpeedMultCurve = Utils.CreateAltitudeCurve(minalt, maxalt, minalt - lowerfade, maxalt + upperfade);
                }
                else
                {
                    AltitudeSpeedMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) });
                }
            }
            if (RadiusSpeedMultCurve == null)
            {
                RadiusSpeedMultCurve = new FloatCurve();
                RadiusSpeedMultCurve.Add(0f, 1f, 0f, 0f);
                RadiusSpeedMultCurve.Add(0.8f, 1f, 0f, -5f);
                RadiusSpeedMultCurve.Add(1f, 0f, -5f, 0f);
            }
            if (LongitudeCenterTimeCurve == null)
            {
                LongitudeCenterTimeCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, (float)longitudeCenter, 0f, 0f) });
            }
            if (LatitudeCenterTimeCurve == null)
            {
                LatitudeCenterTimeCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, (float)latitudeCenter, 0f, 0f) });
            }
        }

        public Vector3 GetWindVector(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity)
        {
            double lonAtTime = (double)Utils.GetValAtLoopTime(LongitudeCenterTimeCurve, time + timeoffset);
            double latAtTime = (double)Utils.GetValAtLoopTime(LatitudeCenterTimeCurve, time + timeoffset);
            double distfraction = UtilMath.Clamp01(AtmoToolsReduxUtils.GreatCircleAngle(lon, lat, lonAtTime, latAtTime) / radius);
            if (distfraction >= 1.0)
            {
                return Vector3.zero;
            }
            float speedmult = Utils.GetValAtLoopTime(WindSpeedMultiplierTimeCurve, time + timeoffset) * RadiusSpeedMultCurve.Evaluate((float)distfraction) * AltitudeSpeedMultCurve.Evaluate((float)alt);
            speedmult *= TrueAnomalyMultiplierCurve.Evaluate((float)trueanomaly) * EccentricityMultiplierCurve.Evaluate((float)eccentricity);

            float xspeed = x_WindSpeed * X_AltitudeSpeedMultCurve.Evaluate((float)alt) * speedmult;
            float yspeed = y_WindSpeed * Y_AltitudeSpeedMultCurve.Evaluate((float)alt) * speedmult;
            float zspeed = z_WindSpeed * Z_AltitudeSpeedMultCurve.Evaluate((float)alt) * speedmult;

            if (useCylindricalFrame)
            {
                double heading = AtmoToolsReduxUtils.RelativeHeading(lon, lat, lonAtTime, latAtTime, true);
                Vector3 tangential = new Vector3(zspeed * (float)Math.Cos(heading), 0f, zspeed * (float)Math.Sin(heading));
                Vector3 radial = new Vector3(xspeed * (float)Math.Sin(heading), 0f, xspeed * (float)Math.Cos(heading));
                Vector3 vertical = new Vector3(0f, yspeed, 0f);
                return tangential + radial + vertical;
            }
            else
            {
                return new Vector3(xspeed, yspeed, zspeed);
            }
        }
    }
}
