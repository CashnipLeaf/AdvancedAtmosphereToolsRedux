using System;
using System.Collections.Generic;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.MultiStarTemperatureController
{
    public class MultiStarTemperatureController : IBaseTemperature
    {
        public bool DisableLatitudeBias => true;
        public bool DisableLatitudeSunMult => true;
        public bool DisableAxialSunBias => true;
        public bool DisableEccentricityBias => true;

        private string body;

        private List<TemperatureController> Stars = new List<TemperatureController>();

        public MultiStarTemperatureController(CelestialBody body) => this.body = body.name;

        public void Initialize() { }

        public void AddStar(ConfigNode cn)
        {
            if (Stars == null)
            {
                Stars = new List<TemperatureController>();
            }
            Stars.Add(new TemperatureController(cn));
        }

        public double GetBaseTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);

            //final temperature is the 4th root of the
            //sum of the 4th power of each body temperature from the given star
            double fourthpowertemp = Math.Pow(AtmoToolsReduxUtils.GetTemperatureAtPosition(mainbody, longitude, latitude, altitude, trueAnomaly, eccentricity), 4.0);

            foreach (TemperatureController star in Stars)
            {
                fourthpowertemp += Math.Pow(star.GetTemperature(mainbody, longitude, latitude, altitude, trueAnomaly), 4.0);
            }
            return Math.Pow(fourthpowertemp, 0.25);
        }
    }

    public sealed class TemperatureController
    {
        public string starName = string.Empty;

        public FloatCurve temperatureCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureSunMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureLatitudeBiasCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureLatitudeSunMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureAxialSunBiasCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureAxialSunMultCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve temperatureEccentricityBiasCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });

        public float maxTempAngleOffset = 45f;

        public double minDistance = double.MaxValue;
        public double maxDistance = 0.0;

        public TemperatureController() { }

        public TemperatureController(string starName, FloatCurve temperatureCurve, FloatCurve temperatureSunMultCurve, FloatCurve temperatureLatitudeBiasCurve, FloatCurve temperatureLatitudeSunMultCurve, FloatCurve temperatureAxialSunBiasCurve, FloatCurve temperatureAxialSunMultCurve, FloatCurve temperatureEccentricityBiasCurve, float maxTempAngleOffset, double minDistance, double maxDistance)
        {
            this.starName = starName;
            this.temperatureCurve = temperatureCurve;
            this.temperatureSunMultCurve = temperatureSunMultCurve;
            this.temperatureLatitudeBiasCurve = temperatureLatitudeBiasCurve;
            this.temperatureLatitudeSunMultCurve = temperatureLatitudeSunMultCurve;
            this.temperatureAxialSunBiasCurve = temperatureAxialSunBiasCurve;
            this.temperatureAxialSunMultCurve = temperatureAxialSunMultCurve;
            this.temperatureEccentricityBiasCurve = temperatureEccentricityBiasCurve;
            this.maxTempAngleOffset = maxTempAngleOffset;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
        }

        //create the object from a config node
        public TemperatureController(ConfigNode cn)
        {
            if (!cn.TryGetValue("starName", ref starName) || string.IsNullOrEmpty(starName))
            {
                throw new ArgumentNullException("starName cannot be empty or null");
            }
            temperatureCurve = Utils.CreateCurveFromNode(cn, "temperatureCurve");
            temperatureSunMultCurve = Utils.CreateCurveFromNode(cn, "temperatureSunMultCurve");
            temperatureLatitudeBiasCurve = Utils.CreateCurveFromNode(cn, "temperatureLatitudeBiasCurve");
            temperatureLatitudeSunMultCurve = Utils.CreateCurveFromNode(cn, "temperatureLatitudeSunMultCurve");
            temperatureAxialSunBiasCurve = Utils.CreateCurveFromNode(cn, "temperatureAxialSunBiasCurve");
            temperatureAxialSunMultCurve = Utils.CreateCurveFromNode(cn, "temperatureAxialSunMultCurve");
            temperatureEccentricityBiasCurve = Utils.CreateCurveFromNode(cn, "temperatureEccentricityBiasCurve");

            cn.TryGetValue("maxTempAngleOffset", ref maxTempAngleOffset);
            cn.TryGetValue("minDistance", ref minDistance);
            cn.TryGetValue("maxDistance", ref maxDistance);
        }

        public double GetTemperature(CelestialBody mainbody, double longitude, double latitude, double altitude, double trueAnomaly)
        {
            CelestialBody otherstar = FlightGlobals.GetBodyByName(starName);
            double star2basetemp = temperatureCurve.Evaluate((float)altitude);
            double star2latbias = temperatureLatitudeBiasCurve.Evaluate((float)Math.Abs(latitude));

            Vector3d position = ScaledSpace.LocalToScaledSpace(mainbody.GetWorldSurfacePosition(latitude, longitude, altitude));

            Vector3d localstarposition = otherstar.scaledBody.transform.position;
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

            return star2basetemp + ((star2latbias + star2latsunmult + star2axialbias + star2eccentricitybias) * (double)temperatureSunMultCurve.Evaluate((float)altitude));
        }
    }
}
