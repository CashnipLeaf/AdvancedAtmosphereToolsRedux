using System;

namespace AdvancedAtmosphereToolsRedux.CustomStructs
{
    public struct OrbitParams
    {
        public double SMA;
        public double Eccentricity;
        public double Inclination;
        public double LongitudeOfAscendingNode;
        public double ArgumentOfPeriapsis;

        public OrbitParams(double a, double e, double i, double l, double p)
        {
            SMA = a;
            Eccentricity = e;
            Inclination = i;
            LongitudeOfAscendingNode = l;
            ArgumentOfPeriapsis = p;
        }

        public Vector3d GetCartesian(double TrueAnomaly)
        {
            double tA = TrueAnomaly * UtilMath.Deg2Rad;
            double a = SMA;
            double e = Eccentricity;
            
            double i = Inclination * UtilMath.Deg2Rad;
            double Lon_AN = LongitudeOfAscendingNode * UtilMath.Deg2Rad;
            double arg_pe = ArgumentOfPeriapsis * UtilMath.Deg2Rad;

            double r = a * ((1 - Math.Pow(e,2)) / (1 + e * Math.Cos(tA)));

            double X_Value = r * (Math.Cos(Lon_AN) * Math.Cos(arg_pe + tA) - Math.Sin(Lon_AN) * Math.Sin(arg_pe + tA) * Math.Cos(i));
            double Y_Value = r * (Math.Sin(Lon_AN) * Math.Cos(arg_pe + tA) + Math.Cos(Lon_AN) * Math.Sin(arg_pe + tA) * Math.Cos(i));
            double Z_Value = r * (Math.Sin(arg_pe + tA) * Math.Sin(i));
            return new Vector3d(X_Value, Y_Value, Z_Value);
        }
    }
}
