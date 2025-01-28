using System;
using AdvancedAtmosphereToolsRedux.Interfaces;
using AdvancedAtmosphereToolsRedux.CustomStructs;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.TwinStarTemperatureController
{
    public class TwinStarTemperatureController : IBaseTemperature, IRequiresFinalSetup
    {
        public const int degreesperiteration = 2;
        
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

        public CelestialBody Body;

        private CelestialBody PrimaryStar;
        private CelestialBody SecondaryStar;
        public string secondStarName = string.Empty;

        public FloatCurve temperatureCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureSunMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureLatitudeBiasCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureLatitudeSunMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureAxialSunBiasCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureAxialSunMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureEccentricityBiasCurve = new FloatCurve(new Keyframe[1] {new Keyframe(0f, 0f, 0f ,0f) });

        public float maxTempAngleOffset = 45f;

        public double minDistance = double.MaxValue;
        public double maxDistance = 0.0;

        public TwinStarTemperatureController() { }

        public void Initialize(CelestialBody body)
        {
            Body = body;

            if (body.isStar)
            {
                throw new ArgumentException("TwinStarTemperatureController cannot be applied to a star.");
            }
        }

        public void FinalSetup()
        {
            PrimaryStar = AtmoToolsReduxUtils.GetLocalStar(Body);
            SecondaryStar = FlightGlobals.GetBodyByName(secondStarName);
            if (SecondaryStar == null)
            {
                throw new ArgumentNullException("Could not locate a celestial body named " + secondStarName + ".");
            }
            if (!SecondaryStar.isStar)
            {
                throw new ArgumentException("Celestial Body " + SecondaryStar.name + " is not a star.");
            }
            if (PrimaryStar == SecondaryStar)
            {
                throw new InvalidOperationException("Secondary Star cannot be the same body as the Primary.");
            }

            CelestialBody localplanet = AtmoToolsReduxUtils.GetLocalPlanet(Body);
            Orbit localorbit = localplanet.orbit;
            OrbitParams orbit1 = new OrbitParams(localorbit.semiMajorAxis, localorbit.eccentricity, localorbit.inclination, localorbit.LAN, localorbit.argumentOfPeriapsis);
            
            if (SecondaryStar.HasChild(PrimaryStar))
            {
                CelestialBody secondaryRef = CelestialBody.GetBodyReferencing(PrimaryStar, SecondaryStar);
                Orbit secondaryorbit = secondaryRef.orbit;
                OrbitParams orbit2 = new OrbitParams(secondaryorbit.semiMajorAxis, secondaryorbit.eccentricity, secondaryorbit.inclination, secondaryorbit.LAN, secondaryorbit.argumentOfPeriapsis);

                for (int i = 0; i < 360; i += degreesperiteration)
                {
                    for (int j = 0; j < 360; j += degreesperiteration)
                    {
                        Vector3d vec1 = orbit1.GetCartesian((double)i);
                        Vector3d vec2 = orbit2.GetCartesian((double)j);
                        Vector3d combined = vec1 + vec2;
                        double distance = combined.magnitude;
                        minDistance = Math.Min(minDistance, distance);
                        maxDistance = Math.Max(maxDistance, distance);
                    }
                }
            }
            else if (SecondaryStar.HasParent(PrimaryStar))
            {
                CelestialBody secondaryRef = CelestialBody.GetBodyReferencing(SecondaryStar, PrimaryStar);
                Orbit secondaryorbit = secondaryRef.orbit;
                OrbitParams orbit2 = new OrbitParams(secondaryorbit.semiMajorAxis, secondaryorbit.eccentricity, secondaryorbit.inclination, secondaryorbit.LAN, secondaryorbit.argumentOfPeriapsis);

                for (int i = 0; i < 360; i += degreesperiteration)
                {
                    for (int j = 0; j < 360; j += degreesperiteration)
                    {
                        Vector3d vec1 = orbit1.GetCartesian((double)i);
                        Vector3d vec2 = orbit2.GetCartesian((double)j);
                        Vector3d combined = vec1 - vec2;
                        double distance = combined.magnitude;
                        minDistance = Math.Min(minDistance, distance);
                        maxDistance = Math.Max(maxDistance, distance);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Primary Star is not a child of the Secondary Star or vice versa.");
            }
            
            Utils.LogInfo($"TwinStarTemperatureController for body {Body.name} computed the following:\nMinimum Distance: {Math.Truncate(minDistance)}\nMaximum Distance: {Math.Truncate(maxDistance)}");
        }

        public double GetBaseTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            double star1temp = AtmoToolsReduxUtils.GetTemperatureAtPosition(Body, longitude, latitude, altitude, trueAnomaly, eccentricity);

            double star2basetemp = temperatureCurve.Evaluate((float)altitude);
            double star2latbias = temperatureLatitudeBiasCurve.Evaluate((float)Math.Abs(latitude));

            Vector3d position = ScaledSpace.LocalToScaledSpace(Body.GetWorldSurfacePosition(latitude, longitude, altitude));

            Vector3d localstarposition = SecondaryStar.scaledBody.transform.position;
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

            Vector3d truesunvector = ScaledSpace.ScaledToLocalSpace(sunVector);
            double star2eccentricity = (truesunvector.magnitude - minDistance) / (maxDistance - minDistance);

            double star2latsunmult = (double)temperatureLatitudeSunMultCurve.Evaluate((float)Math.Abs(latitude)) * num9;
            double star2axialbias = (double)temperatureAxialSunBiasCurve.Evaluate((float)trueAnomaly) * (double)temperatureAxialSunMultCurve.Evaluate((float)Math.Abs(latitude));

            double star2eccentricitybias = (double)temperatureEccentricityBiasCurve.Evaluate((float)star2eccentricity);

            double star2temp = star2basetemp + ((star2latbias + star2latsunmult + star2axialbias + star2eccentricitybias) * (double)temperatureSunMultCurve.Evaluate((float)altitude));
            double finalquartictemp = Math.Pow(star1temp,4) + Math.Pow(star2temp,4);
            return Math.Pow(finalquartictemp,0.25);
        }
    }
}
