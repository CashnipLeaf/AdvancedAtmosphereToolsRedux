using System;
using UnityEngine;
using static Kopernicus.ConfigParser.ParserOptions;

namespace AdvancedAtmosphereToolsRedux
{
    //utility functions that anyone can use
    public static class PublicUtils
    {
        public static AtmosphereData GetAtmosphereData(CelestialBody body)
        {
            AtmosphereData data = body.gameObject.GetComponent<AtmosphereData>();
            if (data != null)
            {
                data = body.gameObject.AddComponent<AtmosphereData>();
                data.Setup(body);
            }
            return data;
        }
        
        public static float BiLerp(float first1, float second1, float first2, float second2, float by1, float by2)
        {
            return Mathf.Lerp(Mathf.Lerp(first1, second1, by1), Mathf.Lerp(first2, second2, by1), by2);
        }

        //allow for altitude spacing based on some factor
        public static double ScaleAltitude(double nX, double xBase, int upperbound, out int int1, out int int2)
        {
            nX = UtilMath.Clamp01(nX);
            double z = (xBase <= 1.0 ? nX : ((Math.Pow(xBase, -nX * upperbound) - 1) / (Math.Pow(xBase, -1 * upperbound) - 1))) * upperbound;
            int1 = Clamp((int)Math.Floor(z), 0, upperbound); //layer 1
            int2 = Clamp(int1 + 1, 0, upperbound); //layer 2
            return nX >= 1d ? 1d : UtilMath.Clamp01(z - Math.Truncate(z));
        }

        public static double InterpolatePressure(double first, double second, double by)
        {
            if (first < 0.0 || second < 0.0) //negative values will break the logarithm, so they are not allowed.
            {
                throw new ArgumentOutOfRangeException();
            }
            if (first <= float.Epsilon || second <= float.Epsilon)
            {
                return UtilMath.Lerp(first, second, by);
            }
            double scalefactor = Math.Log(first / second);
            if (double.IsNaN(scalefactor))
            {
                throw new NotFiniteNumberException();
            }
            return first * Math.Pow(Math.E, -1 * UtilMath.Lerp(0.0, UtilMath.Clamp(scalefactor, float.MinValue, float.MaxValue), by));
        }

        //Apparently no such function exists for integers in either UtilMath or Mathf. Why?
        public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

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

        //Calculate the Great Circle angle between two points. Remember, planets are spherical. Mostly, anyways.
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
    }
}
