using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    internal sealed class VesselHandler : VesselModule
    {
        internal Matrix4x4 LocalToWorld = Matrix4x4.identity;
        
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
            if (!HighLogic.LoadedSceneIsFlight)
            {
                return;
            }
            RawWind.Zero();
            AppliedWind.Zero();
            normalwind.Zero();
            transformedwind.Zero();
            InternalAppliedWind.Zero();

            RawOceanCurrent.Zero();
            AppliedOceanCurrent.Zero();
            InternalAppliedOceanCurrent.Zero();

            toxicAtmosphere = false;

            LocalToWorld.SetColumn(0, (Vector3)vessel.north);
            LocalToWorld.SetColumn(1, (Vector3)vessel.upAxis);
            LocalToWorld.SetColumn(2, (Vector3)vessel.east);
            double longitude = vessel.longitude;
            double latitude = vessel.latitude;
            double altitude = vessel.altitude;
            CelestialBody mainBody = vessel.mainBody;

            double trueAnomaly;
            try
            {
                CelestialBody starref = AtmoToolsReduxUtils.GetLocalPlanet(mainBody);
                trueAnomaly = ((starref.orbit.trueAnomaly * UtilMath.Rad2Deg) + 360.0) % 360.0;
            }
            catch
            {
                trueAnomaly = 0.0;
            }

            double eccentricitybias;
            try
            {
                CelestialBody starref = AtmoToolsReduxUtils.GetLocalPlanet(mainBody);
                if (starref.orbit.eccentricity != 0.0)
                {
                    eccentricitybias = (starref.orbit.radius - starref.orbit.PeR) / (starref.orbit.ApR - starref.orbit.PeR);
                }
                else
                {
                    eccentricitybias = 0.0;
                }
            }
            catch
            {
                eccentricitybias = 0.0;
            }

            DisableMultiplier = vessel != null && vessel.LandedOrSplashed && Settings.DisableWindWhenStationary ? (float)UtilMath.Lerp(0.0, 1.0, (vessel.srfSpeed - 5.0) * 0.2) : 1.0f;

            AtmosphereData bodydata = vessel.mainBody.gameObject.GetComponent<AtmosphereData>();
            if (bodydata != null)
            {
                //TODO: implement
            }
            else
            {
                Pressure = mainBody.GetPressure(altitude);
                FIPressureMultiplier = 1.0;
                FlightIntegrator FI = vessel.GetComponent<FlightIntegrator>();
                double temperatureoffset = FI != null ? FI.atmosphereTemperatureOffset : 0.0;
                Temperature = mainBody.GetFullTemperature(altitude, temperatureoffset);
                MolarMass = mainBody.atmosphereMolarMass;
                AdiabaticIndex = mainBody.atmosphereAdiabaticIndex;
            }
        }

        void OnDestroy()
        {

        }
    }
}
