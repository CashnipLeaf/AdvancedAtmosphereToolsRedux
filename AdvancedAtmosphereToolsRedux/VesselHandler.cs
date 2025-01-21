using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    internal sealed class VesselHandler : VesselModule
    {
        internal Vector3 RawWind = Vector3.zero;
        internal Vector3 AppliedWind = Vector3.zero;

        //wind not multiplied by the wind speed multiplier. for API only.
        internal Vector3 normalwind = Vector3.zero;
        internal Vector3 transformedwind = Vector3.zero;

        //for the "disable wind while splashed or landed and craft is stationary" setting. for flight dynamics use only.
        internal Vector3 InternalAppliedWind = Vector3.zero;

        internal Vector3 RawOceanCurrent = Vector3.zero;
        internal Vector3 AppliedOceanCurrent = Vector3.zero;

        internal Vector3 InternalAppliedOceanCurrent = Vector3.zero;

        private float DisableMultiplier = 1.0f;

        private double temperature = 0.0;
        internal double Temperature
        {
            get => temperature;
            private set => temperature = UtilMath.Clamp(value, 0.0, float.MaxValue);
        }

        private double pressure = 0.0;
        internal double Pressure
        {
            get => pressure;
            private set => pressure = UtilMath.Clamp(value, 0.0, float.MaxValue);
        }
        private double pressuremultiplier = 1.0;
        internal double FIPressureMultiplier
        {
            get => pressuremultiplier;
            set => pressuremultiplier = double.IsFinite(value) ? value : 1.0;
        }

        private double molarmass = 0.0;
        internal double MolarMass
        {
            get => molarmass;
            set => molarmass = Math.Max(value, 0.0);
        }

        private double adiabaticindex = 0.0;
        internal double AdiabaticIndex
        {
            get => adiabaticindex;
            set => adiabaticindex = Math.Max(value, 0.0);
        }

        internal bool toxicAtmosphere = false;
        internal string toxicAtmosphereMessage = string.Empty;

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        //TODO: implement
        void FixedUpdate()
        {

        }

        void OnDestroy()
        {

        }
    }
}
