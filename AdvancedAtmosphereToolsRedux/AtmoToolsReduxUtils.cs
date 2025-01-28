using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    //utility functions for anyone to use
    public static class AtmoToolsReduxUtils
    {
        //--------------------GENERAL UTILITIES---------------------------
        public const double WaterBulkModulus = 2.2E+6; //in kPa
        public const double DefaultMaxTempAngleOffset = 45.0; //in degrees

        public static double GetTemperatureAtPosition(CelestialBody body, double longitude, double latitude, double altitude, double trueAnomaly, double eccentricity)
        {
            GetTemperatureWithComponents(body, longitude, latitude, altitude, trueAnomaly, eccentricity, out double basetemp, out double latbias, out double latsunmult, out double axialbias, out double eccentricitybias);
            return basetemp + ((latbias + latsunmult + axialbias + eccentricitybias) * (double)body.atmosphereTemperatureSunMultCurve.Evaluate((float)altitude));
        }

        public static void GetTemperatureWithComponents(CelestialBody body, double longitude, double latitude, double altitude, double trueAnomaly, double eccentricity, out double basetemp, out double latbias, out double latsunmult, out double axialbias, out double eccentricitybias)
        {
            basetemp = body.GetTemperature(altitude);
            latbias = (double)body.latitudeTemperatureBiasCurve.Evaluate((float)Math.Abs(latitude));
            latsunmult = axialbias = eccentricitybias = 0.0;
            if (body != FlightIntegrator.sunBody)
            {
                Vector3d position = ScaledSpace.LocalToScaledSpace(body.GetWorldSurfacePosition(latitude, longitude, altitude));
                CelestialBody localstar = GetLocalStar(body);
                if (localstar != null)
                {
                    localstar = FlightIntegrator.sunBody;
                }
                Vector3d localstarposition = localstar.scaledBody.transform.position;
                Vector3d sunVector = localstarposition - position;
                double magnitude = sunVector.magnitude;
                if (magnitude == 0.0)
                {
                    magnitude = 1.0;
                }
                Vector3d normalSunVector = sunVector / magnitude;
                Vector3d up = body.bodyTransform.up;
                Vector3d upAxis = body.GetRelSurfaceNVector(latitude, longitude);

                double d1 = (double) Vector3.Dot((Vector3)normalSunVector, up);
                double d2 = (double) Vector3.Dot(up, (Vector3) upAxis);
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

                double sunmult = (1.0 + (double)Vector3.Dot((Vector3)normalSunVector, Quaternion.AngleAxis(body.MaxTempAngleOffset() * Mathf.Sign((float)body.rotationPeriod), up) * (Vector3)upAxis)) * 0.5;
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

                latsunmult = (double)body.latitudeTemperatureSunMultCurve.Evaluate((float)Math.Abs(latitude)) * num9;

                axialbias = (double)body.axialTemperatureSunBiasCurve.Evaluate((float)trueAnomaly) * (double)body.axialTemperatureSunMultCurve.Evaluate((float)Math.Abs(latitude));

                eccentricitybias = (double)body.eccentricityTemperatureBiasCurve.Evaluate((float)eccentricity);
            }
        }

        //CelestialBody.GetDensity() but manipulated for my own purposes
        public static double GetDensity(double pressure, double temperature, double molarmass) => pressure > 0.0 && temperature > 0.0 ? (pressure * 1000 * molarmass) / (temperature * PhysicsGlobals.IdealGasConstant) : 0.0;

        //CelestialBody.GetSpeedOfSound() but manipulated for my own purposes
        public static double GetSpeedOfSound(double pressure, double density, double adiabaticIndex) => pressure > 0.0 && density > 0.0 ? Math.Sqrt(adiabaticIndex * (pressure * 1000 / density)) : 0.0;

        public static Matrix4x4 GetVesselTransformMatrix(Vessel vessel)
        {
            Matrix4x4 LocalToWorld = Matrix4x4.identity;
            LocalToWorld.SetColumn(0, (Vector3)vessel.north);
            LocalToWorld.SetColumn(1, (Vector3)vessel.up);
            LocalToWorld.SetColumn(2, (Vector3)vessel.east);
            return LocalToWorld;
        }

        //--------------------LOCATION UTILITIES---------------------------
        //Calculate the Great Circle angle between two points.Remember, planets are spherical. Mostly, anyways.
        public static double GreatCircleAngle(double lon1, double lat1, double lon2, double lat2, bool radians = false)
        {
            //convert degrees to radians
            lon1 *= UtilMath.Deg2Rad;
            lat1 *= UtilMath.Deg2Rad;
            lon2 *= UtilMath.Deg2Rad;
            lat2 *= UtilMath.Deg2Rad;
            double angle = Math.Acos((Math.Sin(lat1) * Math.Sin(lat2)) + (Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(Math.Abs(lon1 - lon2))));
            return radians ? angle : angle * UtilMath.Rad2Deg;
        }

        //Calculate the relative heading from point 1 to point 2, with 0 being north, 90 being east, 180 being south, and -90/270 being west.
        public static double RelativeHeading(double lon1, double lat1, double lon2, double lat2, bool radians = false)
        {
            //default to north if the two points are exactly antipodal or exactly on top of each other.
            if ((lat1 == lat2 && lon1 == lon2) || ((lat1 == (lat2 * -1.0)) && (lon1 + 180.0 == lon2 || lon1 - 180.0 == lon2))) { return 0.0; }

            //Compute the angle between the north pole and the second point relative to the first point using the spherical law of cosines.
            //Don't worry, this hurt my brain, too. 
            double sideA = GreatCircleAngle(lon1, lat1, 0.0, 90.0, true); //craft to north pole
            double sideB = GreatCircleAngle(lon2, lat2, 0.0, 90.0, true); //center of current to north pole
            double sideC = GreatCircleAngle(lon1, lat1, lon2, lat2, true); //craft to center of current

            double heading = Math.Acos((Math.Cos(sideA) - (Math.Cos(sideB) * Math.Cos(sideC))) / (Math.Cos(sideB) * Math.Cos(sideC)));

            //The above function only computes the angle from 0 to 180 degrees, irrespective of east/west direction.
            //This line checks for that direction and modifies the heading accordingly.
            if (Math.Sin((lon1 - lon2) * UtilMath.Deg2Rad) < 0)
            {
                heading *= -1;
            }
            return radians ? heading : heading * UtilMath.Rad2Deg;
        }

        //--------------------CELESTIAL BODY UTILITIES---------------------

        //Get the host star of the body.
        //Adapted from Kopernicus
        public static CelestialBody GetLocalStar(CelestialBody body)
        {
            if (body == null)
            {
                throw new ArgumentNullException();
            }

            while (body?.orbit?.referenceBody != null)
            {
                if (body.isStar || body == FlightGlobals.Bodies[0])
                {
                    break;
                }
                body = body.orbit.referenceBody;
            }
            return body;
        }

        //get the body referencing the host star
        //Adapted from Kopernicus
        public static CelestialBody GetLocalPlanet(CelestialBody body)
        {
            if (body == null)
            {
                throw new ArgumentNullException();
            }

            while (body?.orbit?.referenceBody != null)
            {
                if (body.orbit.referenceBody.isStar || body.orbit.referenceBody == FlightGlobals.Bodies[0])
                {
                    break;
                }
                body = body.orbit.referenceBody;
            }
            return body;
        }

        //Get the true anomaly (0-360 degrees) and eccentricity bias (0-1 where 1 is apoapsis and 0 is periapsis) of this body (or its whatever-th parent body) around its local star
        public static void GetTrueAnomalyEccentricity(CelestialBody body, out double trueAnomaly, out double eccentricitybias)
        {
            trueAnomaly = eccentricitybias = 0.0;
            CelestialBody refbody = GetLocalPlanet(body);
            if (refbody != null && refbody != FlightIntegrator.sunBody && refbody.orbit != null)
            {
                trueAnomaly = ((refbody.orbit.trueAnomaly * UtilMath.Rad2Deg) + 360.0) % 360.0;
                eccentricitybias = refbody.orbit.eccentricity != 0.0 ? (refbody.orbit.radius - refbody.orbit.PeR) / (refbody.orbit.ApR - refbody.orbit.PeR) : 0.0;
            }
        }
    }
}
