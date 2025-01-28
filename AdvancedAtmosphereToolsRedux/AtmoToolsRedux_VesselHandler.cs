using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    //TODO: add aero and hydrodynamic drag/lift stats
    internal sealed class AtmoToolsRedux_VesselHandler : VesselModule
    {
        FlightIntegrator CacheFI;
        internal Matrix4x4 LocalToWorld = Matrix4x4.identity;
        
        internal Vector3 RawWind = Vector3.zero;
        internal Vector3 AppliedWind = Vector3.zero;

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
            private set => temperature = Math.Max(0.0,value);
        }

        private double pressure = 0.0;
        internal double Pressure
        {
            get => pressure;
            private set => pressure = Math.Max(0.0, value);
        }
        private double pressuremultiplier = 1.0;
        internal double FIPressureMultiplier
        {
            get => pressuremultiplier;
            private set => pressuremultiplier = double.IsFinite(value) ? value : 1.0;
        }

        private double molarmass = 0.0;
        internal double MolarMass
        {
            get => molarmass;
            private set => molarmass = Math.Max(value, 0.0);
        }

        private double adiabaticindex = 0.0;
        internal double AdiabaticIndex
        {
            get => adiabaticindex;
            private set => adiabaticindex = Math.Max(value, 0.0);
        }

        public override int GetOrder() => -5;

        public override bool ShouldBeActive() => vessel.loaded;

        public override Activation GetActivation() => Activation.FlightScene;

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        void FixedUpdate()
        {
            if (CacheFI == null)
            {
                foreach (VesselModule VM in vessel.vesselModules)
                {
                    if (VM is FlightIntegrator flight)
                    {
                        CacheFI = flight;
                        break;
                    }
                }
            }

            RawWind.Zero();
            AppliedWind.Zero();
            InternalAppliedWind.Zero();

            RawOceanCurrent.Zero();
            AppliedOceanCurrent.Zero();
            InternalAppliedOceanCurrent.Zero();

            LocalToWorld.SetColumn(0, (Vector3)vessel.north);
            LocalToWorld.SetColumn(1, (Vector3)vessel.up);
            LocalToWorld.SetColumn(2, (Vector3)vessel.east);

            double longitude = vessel.longitude;
            double latitude = vessel.latitude;
            double altitude = vessel.altitude;
            double time = Planetarium.GetUniversalTime();
            CelestialBody mainBody = vessel.mainBody;

            DisableMultiplier = 1.0f;
            if (vessel != null)
            {
                if (vessel.easingInToSurface)
                {
                    DisableMultiplier = 0.0f;
                }
                else
                {
                    DisableMultiplier = vessel.LandedOrSplashed && Settings.DisableWindWhenStationary ? (float)UtilMath.Lerp(0.0, 1.0, (vessel.srfSpeed - 5.0) * 0.2) : 1.0f;
                }
            }

            if (mainBody == null || !mainBody.atmosphere || altitude > mainBody.atmosphereDepth)
            {
                Pressure = 0.0;
                FIPressureMultiplier = 1.0;
                Temperature = PhysicsGlobals.SpaceTemperature;
                MolarMass = mainBody.atmosphereMolarMass;
                AdiabaticIndex = mainBody.atmosphereAdiabaticIndex;
                return;
            }

            AtmoToolsReduxUtils.GetTrueAnomalyEccentricity(mainBody, out double trueAnomaly, out double eccentricitybias);

            AtmosphereData bodydata = AtmosphereData.GetAtmosphereData(mainBody);
            if (bodydata != null)
            {
                //Wind
                try
                {
                    RawWind = bodydata.GetWindVector(longitude, latitude, altitude, time, trueAnomaly, eccentricitybias);
                    AppliedWind = LocalToWorld * RawWind * Settings.GlobalWindSpeedMultiplier;

                    InternalAppliedWind = AppliedWind * DisableMultiplier;
                }
                catch
                {
                    RawWind.Zero();
                    AppliedWind.Zero();
                    InternalAppliedWind.Zero();
                }

                //Ocean Currents (unused)
                try
                {
                    RawOceanCurrent = bodydata.GetOceanCurrentVector(longitude, latitude, altitude, time, trueAnomaly, eccentricitybias);
                    AppliedOceanCurrent = LocalToWorld * RawOceanCurrent;

                    InternalAppliedOceanCurrent = AppliedOceanCurrent * DisableMultiplier;
                }
                catch
                {
                    RawOceanCurrent.Zero();
                    AppliedOceanCurrent.Zero();
                    InternalAppliedOceanCurrent.Zero();
                }

                //Temperature
                try
                {
                    Temperature = bodydata.GetTemperature(longitude, latitude, altitude, time, trueAnomaly, eccentricitybias);
                }
                catch
                {
                    double temperatureoffset = CacheFI != null ? CacheFI.atmosphereTemperatureOffset : 0.0;
                    Temperature = mainBody.GetFullTemperature(altitude, temperatureoffset);
                }

                //Pressure
                try
                {
                    Pressure = bodydata.GetPressure(longitude, latitude, altitude, time, trueAnomaly, eccentricitybias);
                    FIPressureMultiplier = Pressure / mainBody.GetPressure(altitude);
                    if (!double.IsFinite(FIPressureMultiplier))
                    {
                        FIPressureMultiplier = 1.0;
                    }
                }
                catch
                {
                    Pressure = mainBody.GetPressure(altitude);
                    FIPressureMultiplier = 1.0;
                }

                //Molar Mass
                try
                {
                    MolarMass = bodydata.GetMolarMass(longitude, latitude, altitude, time, trueAnomaly, eccentricitybias);
                }
                catch
                {
                    MolarMass = mainBody.atmosphereMolarMass;
                }

                //Adiabatic Index
                try
                {
                    AdiabaticIndex = bodydata.GetAdiabaticIndex(longitude, latitude, altitude, time, trueAnomaly, eccentricitybias);
                }
                catch
                {
                    AdiabaticIndex = mainBody.atmosphereAdiabaticIndex;
                }
            }
            else
            {
                Pressure = mainBody.GetPressure(altitude);
                FIPressureMultiplier = 1.0;
                double temperatureoffset = CacheFI != null ? CacheFI.atmosphereTemperatureOffset : 0.0;
                Temperature = mainBody.GetFullTemperature(altitude, temperatureoffset);
                MolarMass = mainBody.atmosphereMolarMass;
                AdiabaticIndex = mainBody.atmosphereAdiabaticIndex;
            }
        }

        void OnDestroy()
        {
            CacheFI = null;
            FlightSceneHandler.ClearCache();
        }
    }
}
