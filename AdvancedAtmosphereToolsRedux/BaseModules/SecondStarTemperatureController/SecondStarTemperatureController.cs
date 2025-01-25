using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.SecondStarTemperatureController
{
    public class SecondStarTemperatureController : IFlatTemperatureModifier
    {
        public CelestialBody Body;

        private CelestialBody SecondStar;
        public string secondStarName = string.Empty;

        public FloatCurve temperatureSunMultCurve;
        public FloatCurve temperatureLatitudeBiasCurve;
        public FloatCurve temperatureLatitudeSunMultCurve;
        public float maxTempAngleOffset = 45f;

        public SecondStarTemperatureController() { }

        public void Initialize(CelestialBody body)
        {
            Body = body;
            SecondStar = FlightGlobals.GetBodyByName(secondStarName);
            if (SecondStar == null)
            {
                throw new ArgumentNullException("Could not locate a celestial body named " +  secondStarName + ".");
            }
            if (!SecondStar.isStar)
            {
                throw new ArgumentException("Celestial Body " + SecondStar.name + " is not a star.");
            }
            if (temperatureLatitudeBiasCurve == null || temperatureLatitudeSunMultCurve == null || temperatureSunMultCurve == null)
            {
                throw new ArgumentNullException("One or more of the required FloatCurves was null or was not inputted.");
            }
        }

        public double GetFlatTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            Vector3d position = ScaledSpace.LocalToScaledSpace(Body.GetWorldSurfacePosition(latitude, longitude, altitude));
            Vector3d localstarposition = SecondStar.scaledBody.transform.position;
            Vector3d sunVector = localstarposition - position;
            double magnitude = sunVector.magnitude;
            if (magnitude == 0.0)
            {
                magnitude = 1.0;
            }
            Vector3d normalSunVector = sunVector / magnitude;
            Vector3d up = Body.bodyTransform.up;
            Vector3d upAxis = Body.GetRelSurfaceNVector(latitude, longitude);

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

            double sunmult = (1.0 + (double)Vector3.Dot((Vector3)normalSunVector, Quaternion.AngleAxis(maxTempAngleOffset * Mathf.Sign((float)Body.rotationPeriod), up) * (Vector3)upAxis)) * 0.5;
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

            double latbias = (double)temperatureLatitudeBiasCurve.Evaluate((float)Math.Abs(latitude));

            double latsunmult = (double)temperatureLatitudeSunMultCurve.Evaluate((float)Math.Abs(latitude)) * num9;

            return (latbias + latsunmult) * (double)temperatureLatitudeSunMultCurve.Evaluate((float)altitude);
        }
    }
}
