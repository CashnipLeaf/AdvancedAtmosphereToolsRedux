using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TwinStarTemperatureController
{
    public class TwinStarTemperatureController : IBaseTemperature
    {
        public bool DisableLatitudeBias
        {
            get => true;
            set => Garbage = value;
        }
        public bool DisableLatitudeSunMult
        {
            get => true;
            set => Garbage = value;
        }
        public bool DisableAxialSunBias
        {
            get => true;
            set => Garbage = value;
        }
        public bool DisableEccentricityBias
        {
            get => true;
            set => Garbage = value;
        }
        private bool Garbage = true; //all of the above properties must only ever be true

        private string body;

        public string secondStarName = string.Empty;

        public FloatCurve temperatureCurve = AtmoToolsReduxUtils.ZeroCurve();
        public FloatCurve temperatureSunMultCurve = AtmoToolsReduxUtils.ZeroCurve();
        public FloatCurve temperatureLatitudeBiasCurve = AtmoToolsReduxUtils.ZeroCurve();
        public FloatCurve temperatureLatitudeSunMultCurve = AtmoToolsReduxUtils.ZeroCurve();
        public FloatCurve temperatureAxialSunBiasCurve = AtmoToolsReduxUtils.ZeroCurve();
        public FloatCurve temperatureAxialSunMultCurve = AtmoToolsReduxUtils.ZeroCurve();
        public FloatCurve temperatureEccentricityBiasCurve = AtmoToolsReduxUtils.ZeroCurve();

        public float maxTempAngleOffset = 45f;

        public double minDistance = double.MaxValue;
        public double maxDistance = 0.0;

        public TwinStarTemperatureController(CelestialBody body)
        {
            if (body.isStar)
            {
                throw new ArgumentException("TwinStarTemperatureController cannot be applied to a star.");
            }
            this.body = body.name;
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(secondStarName))
            {
                throw new ArgumentException("secondaryStar field cannot be blank.");
            }
        }

        public double GetBaseTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);
            CelestialBody SecondaryStar = FlightGlobals.GetBodyByName(secondStarName);
            double star1temp = AtmoToolsReduxUtils.GetTemperatureAtPosition(mainbody, longitude, latitude, altitude, trueAnomaly, eccentricity);

            double star2basetemp = temperatureCurve.Evaluate((float)altitude);
            double star2latbias = temperatureLatitudeBiasCurve.Evaluate((float)Math.Abs(latitude));

            Vector3d position = ScaledSpace.LocalToScaledSpace(mainbody.GetWorldSurfacePosition(latitude, longitude, altitude));

            Vector3d localstarposition = SecondaryStar.scaledBody.transform.position;
            Vector3d sunVector = localstarposition - position;
            double magnitude = sunVector.magnitude;
            if (magnitude == 0.0)
            {
                magnitude = 1.0;
            }
            Vector3d normalSunVector = sunVector / magnitude;
            Vector3d up = mainbody.bodyTransform.up;
            Vector3d upAxis = mainbody.GetSurfaceNVector(latitude, longitude);

            double d1 = (double)Vector3.Dot((Vector3)normalSunVector, up);
            double d2 = (double)Vector3.Dot(up, (Vector3)upAxis);
            double d3 = Math.Acos(d2);
            if (double.IsNaN(d3))
            {
                d3 = d2 >= 0.0 ? 0.0 : Math.PI;
            }
            double d4 = Math.Acos(d1);
            if (double.IsNaN(d4))
            {
                d4 = d1 >= 0.0 ? 0.0 : Math.PI;
            }
            double t1 = (1.0 + Math.Cos(d4 - d3)) * 0.5;
            double num1 = (1.0 + Math.Cos(d4 + d3)) * 0.5;

            double sunmult = (1.0 + (double)Vector3.Dot((Vector3)normalSunVector, Quaternion.AngleAxis(maxTempAngleOffset * Mathf.Sign((float)mainbody.rotationPeriod), up) * (Vector3)upAxis)) * 0.5;
            double num8 = t1 - num1;
            double num9;
            if (num8 > 0.001)
            {
                num9 = (sunmult - num1) / num8;
                if (double.IsNaN(num9))
                {
                    num9 = sunmult > 0.5 ? 1.0 : 0.0;
                }
            }
            else
            {
                num9 = num1 + num8 * 0.5;
            }

            Vector3d truesunvector = ScaledSpace.ScaledToLocalSpace(sunVector);
            double star2eccentricity = minDistance > maxDistance ? 0.0 : UtilMath.Clamp01((truesunvector.magnitude - minDistance) / (maxDistance - minDistance));

            double star2latsunmult = (double)temperatureLatitudeSunMultCurve.Evaluate((float)Math.Abs(latitude)) * num9;
            double star2axialbias = (double)temperatureAxialSunBiasCurve.Evaluate((float)trueAnomaly) * (double)temperatureAxialSunMultCurve.Evaluate((float)Math.Abs(latitude));

            double star2eccentricitybias = (double)temperatureEccentricityBiasCurve.Evaluate((float)star2eccentricity);

            double star2temp = star2basetemp + ((star2latbias + star2latsunmult + star2axialbias + star2eccentricitybias) * (double)temperatureSunMultCurve.Evaluate((float)altitude));
            double finalquartictemp = Math.Pow(star1temp,4) + Math.Pow(star2temp,4);
            return Math.Pow(finalquartictemp,0.25);
        }
    }
}
