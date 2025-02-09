using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    public static class AtmoToolsRedux_API
    {
        private const string NotFlight = "Currently loaded scene is not Flight.";
        private const string NullVessel = "Inputted Vessel was null.";

        public static Vector3 GetWindVector(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            if (body.atmosphere && altitude <= body.atmosphereDepth)
            {
                AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
                return data != null ? data.GetWindVector(longitude, latitude, altitude, time, trueAnomaly, eccentricityBias) : Vector3.zero;
            }
            return Vector3.zero;
        }

        public static Vector3 GetOceanCurrentVector(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            throw new NotImplementedException("Ocean Currents are not implemented.");
            /*
            if (body.atmosphere && altitude <= body.atmosphereDepth)
            {
                AtmosphereData data = AtmosphereData.GetAtmosphereData(body);
                return data != null ? data.GetOceanCurrentVector(longitude, latitude, altitude, time, trueAnomaly, eccentricityBias) : Vector3.zero;
            }
            return Vector3.zero;
            */
        }

        public static double GetTemperature(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            if (body.atmosphere && altitude <= body.atmosphereDepth)
            {
                AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
                return data != null ? data.GetTemperature(longitude, latitude, altitude, time, trueAnomaly, eccentricityBias) : AtmoToolsReduxUtils.GetTemperatureAtPosition(body, longitude, latitude, altitude, trueAnomaly, eccentricityBias);
            }
            return PhysicsGlobals.SpaceTemperature;
        }

        public static double GetPressure(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            if (body.atmosphere && altitude <= body.atmosphereDepth)
            {
                AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
                return data != null ? data.GetPressure(longitude, latitude, altitude, time, trueAnomaly, eccentricityBias) : body.GetPressure(altitude);
            }
            return 0.0;
        }

        public static double GetMolarMass(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            if (body.atmosphere && altitude <= body.atmosphereDepth)
            {
                AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
                return data != null ? data.GetMolarMass(longitude, latitude, altitude, time, trueAnomaly, eccentricityBias) : body.atmosphereMolarMass;
            }
            return body.atmosphereMolarMass;
        }

        public static double GetAdiabaticIndex(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            if (body.atmosphere && altitude <= body.atmosphereDepth)
            {
                AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
                return data != null ? data.GetAdiabaticIndex(longitude, latitude, altitude, time, trueAnomaly, eccentricityBias) : body.atmosphereAdiabaticIndex;
            }
            return body.atmosphereAdiabaticIndex;
        }

        public static double GetAtmosphereDensity(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            double pressure = GetPressure(body, longitude, latitude, altitude, time, trueAnomaly, eccentricityBias);
            double temperature = GetTemperature(body, longitude, latitude, altitude, time, trueAnomaly, eccentricityBias);
            double molarmass = GetMolarMass(body, longitude, latitude, altitude, time, trueAnomaly, eccentricityBias);
            return AtmoToolsReduxUtils.GetDensity(pressure, temperature, molarmass);
        }

        public static double GetSpeedOfSound(CelestialBody body, double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricityBias)
        {
            double pressure = GetPressure(body, longitude, latitude, altitude, time, trueAnomaly, eccentricityBias);
            double density = GetAtmosphereDensity(body, longitude, latitude, altitude, time, trueAnomaly, eccentricityBias);
            double adiabaticindex = GetAdiabaticIndex(body, longitude, latitude, altitude, time, trueAnomaly, eccentricityBias);
            return AtmoToolsReduxUtils.GetSpeedOfSound(pressure, density, adiabaticindex);
        }

        public static Vector3 GetVesselWindVector(Vessel vessel)
        {
            if (vessel == null)
            {
                throw new ArgumentNullException(NullVessel);
            }
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(vessel);
            if (VH != null)
            {
                return AtmoToolsReduxUtils.GetVesselTransformMatrix(vessel) * VH.RawWind;
            }
            return Vector3.zero;
        }

        public static Vector3 GetVesselOceanCurrentVector(Vessel vessel)
        {
            throw new NotImplementedException("Ocean Currents are not implemented.");
            /*
            if (vessel == null)
            {
                throw new ArgumentNullException(NullVessel);
            }
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(vessel);
            if (VH != null)
            {
                return AtmoToolsReduxUtils.GetVesselTransformMatrix(vessel) * VH.RawOceanCurrent;
            }
            return Vector3.zero;
            */
        }

        public static double GetVesselTemperature(Vessel vessel)
        {
            if (vessel == null)
            {
                throw new ArgumentNullException(NullVessel);
            }
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(vessel);
            return VH != null ? VH.Temperature : 0.0;
        }

        public static double GetVesselPressure(Vessel vessel)
        {
            if (vessel == null)
            {
                throw new ArgumentNullException(NullVessel);
            }
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(vessel);
            return VH != null ? VH.Pressure : 0.0;
        }

        public static double GetVesselMolarMass(Vessel vessel)
        {
            if (vessel == null)
            {
                throw new ArgumentNullException(NullVessel);
            }
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(vessel);
            return VH != null ? VH.MolarMass : 0.0;
        }

        public static double GetVesselAdiabaticIndex(Vessel vessel)
        {
            if (vessel == null)
            {
                throw new ArgumentNullException(NullVessel);
            }
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(vessel);
            return VH != null ? VH.AdiabaticIndex : 0.0;
        }

        public static Vector3 GetActiveVesselWindVector()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                throw new InvalidOperationException(NotFlight);
            }
            return GetVesselWindVector(FlightGlobals.ActiveVessel);
        }

        public static Vector3 GetActiveVesselOceanCurrentVector()
        {
            throw new NotImplementedException("Ocean Currents are not implemented.");
            /*
            if (!HighLogic.LoadedSceneIsFlight)
            {
                throw new InvalidOperationException(NotFlight);
            }
            return GetVesselOceanCurrentVector(FlightGlobals.ActiveVessel);
            */
        }

        public static double GetActiveVesselTemperature()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                throw new InvalidOperationException();
            }
            return GetVesselTemperature(FlightGlobals.ActiveVessel);
        }

        public static double GetActiveVesselPressure()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                throw new InvalidOperationException(NotFlight);
            }
            return GetVesselPressure(FlightGlobals.ActiveVessel);
        }

        public static double GetActiveVesselMolarMass()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                throw new InvalidOperationException(NotFlight);
            }
            return GetVesselMolarMass(FlightGlobals.ActiveVessel);
        }

        public static double GetActiveVesselAdiabaticIndex()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                throw new InvalidOperationException(NotFlight);
            }
            return GetVesselAdiabaticIndex(FlightGlobals.ActiveVessel);
        }
    }
}
