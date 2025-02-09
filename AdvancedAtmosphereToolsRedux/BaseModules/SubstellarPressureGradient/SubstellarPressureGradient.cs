using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.SubstellarPressureGradient
{
    public class SubstellarPressureGradient : IFractionalPressureModifier
    {
        private string body;

        public FloatCurve GradientCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 0f, 0f, 0f) });
        public FloatCurve AltitudeCurve = new FloatCurve(new Keyframe[1] { new Keyframe(0f, 1f, 0f, 0f) });
        public float angleOffset = 0f;

        public SubstellarPressureGradient(CelestialBody body) => this.body = body.name;

        public double GetFractionalPressureModifier(double lon, double lat, double alt, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);
            CelestialBody localstar = AtmoToolsReduxUtils.GetLocalStar(mainbody);
            Vector3d up = mainbody.bodyTransform.up;

            AtmoToolsReduxUtils.GetUpVectorAndSunVector(mainbody, localstar, lon, lat, alt, out Vector3d upAxis, out Vector3d sunvec);

            double dotproduct = (1.0 + (double)Vector3.Dot((Vector3)sunvec, Quaternion.AngleAxis(angleOffset * Mathf.Sign((float)mainbody.rotationPeriod), up) * (Vector3)upAxis)) * 0.5;
            return (double)(GradientCurve.Evaluate((float)dotproduct) * AltitudeCurve.Evaluate((float)alt));
        }
    }
}
