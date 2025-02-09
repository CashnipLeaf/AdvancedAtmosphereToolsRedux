using System;
using System.Collections.Generic;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    public sealed class AtmoToolsRedux_Data
    {
        private static Dictionary<string, AtmoToolsRedux_Data> AtmoData;

        //used for setting up modifiers
        private static AtmoToolsRedux_Data GetOrCreateAtmosphereData(string body)
        {
            if (AtmoData == null)
            {
                AtmoData = new Dictionary<string, AtmoToolsRedux_Data>();
            }
            if (!AtmoData.ContainsKey(body))
            {
                AtmoData.Add(body, new AtmoToolsRedux_Data(body));
            }
            return AtmoData[body];
        }

        //used only for getting data. if you wanna get data through this, be my guest :)
        public static AtmoToolsRedux_Data GetAtmosphereData(CelestialBody body) => GetAtmosphereData(body.name);
        public static AtmoToolsRedux_Data GetAtmosphereData(string body) => AtmoData != null && AtmoData.ContainsKey(body) ? AtmoData[body] : null;

        private AtmoToolsRedux_Data(string bodyname)
        {
            Utils.LogInfo("Creating a new AtmosphereData holder for " + bodyname + ".");
            body = bodyname;
        }
        private string body;

        public double MaxTempAngleOffset { get; private set; } = AtmoToolsReduxUtils.DefaultMaxTempAngleOffset;
        public static void SetMaxTempAngleOffset(double maxTempAngleOffset, string body) => GetOrCreateAtmosphereData(body).MaxTempAngleOffset = maxTempAngleOffset;
        public static void SetMaxTempAngleOffset(double maxTempAngleOffset, CelestialBody body) => SetMaxTempAngleOffset(maxTempAngleOffset, body.name);

        #region wind
        private List<IWindProvider> windProviders = new List<IWindProvider>();
        public static void AddWindProvider(IWindProvider provider, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.windProviders.Add(provider);
            string type = (provider.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Wind Provider for body" + body + ".");
        }
        public static void AddWindProvider(IWindProvider provider, CelestialBody body) => AddWindProvider(provider, body.name);

        public Vector3 GetWindVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            Vector3 windvec = Vector3.zero;
            foreach (IWindProvider provider in windProviders)
            {
                try
                {
                    Vector3 bodywindvec = provider.GetWindVector(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (bodywindvec.IsFinite())
                    {
                        windvec.Add(bodywindvec);
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return windvec;
        }
        #endregion

        #region ocean
        //Ocean-related stuff is unused for the time being
        public double OceanBulkModulus { get; private set; } = AtmoToolsReduxUtils.WaterBulkModulus; //in kPa
        public void SetOceanBulkModulus(double bulkmodulus, string body) => throw new NotImplementedException(); //GetOrCreateAtmosphereData(body).OceanBulkModulus = bulkmodulus;
        public void SetOceanBulkModulus(double bulkmodulus, CelestialBody body) => SetOceanBulkModulus(bulkmodulus, body.name);

        private List<IOceanCurrentProvider> oceanCurrentProviders = new List<IOceanCurrentProvider>();
        public static void AddOceanCurrentProvider(IOceanCurrentProvider provider, string body)
        {
            throw new NotImplementedException();
            /*
            AtmosphereData data = GetOrCreateAtmosphereData(body);
            data.oceanCurrentProviders.Add(provider);
            string type = (provider.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as an Ocean Current Provider for body" + body + ".");
            */
        }
        public static void AddOceanCurrentProvider(IOceanCurrentProvider provider, CelestialBody body) => AddOceanCurrentProvider(provider, body.name);

        public Vector3 GetOceanCurrentVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            throw new NotImplementedException();
            /*
            Vector3 oceanvec = Vector3.zero;
            foreach (IOceanCurrentProvider provider in oceanCurrentProviders)
            {
                try
                {
                    Vector3 bodyoceanvec = provider.GetOceanCurrentVector(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (bodyoceanvec.IsFinite())
                    {
                        oceanvec.Add(bodyoceanvec);
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return oceanvec;
            */
        }
        #endregion

        #region pressure
        private IBasePressure basePressure;
        public static void SetBasePressure(IBasePressure bp, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            if (data.basePressure != null)
            {
                Utils.LogWarning("Base Pressure already exists for body " + body + ".");
            }
            else
            {
                data.basePressure = bp;
                string type = (bp.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Pressure for body" + body + ".");
            }
        }
        public static void SetBasePressure(IBasePressure bp, CelestialBody body) => SetBasePressure(bp, body.name);

        private List<IFractionalPressureModifier> fractionalPressureModifiers = new List<IFractionalPressureModifier>();
        public static void AddFractionalPressureModifier(IFractionalPressureModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.fractionalPressureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional Pressure Modifier for body" + body + ".");
        }
        public static void AddFractionalPressureModifier(IFractionalPressureModifier modifier, CelestialBody body) => AddFractionalPressureModifier(modifier, body.name);

        private List<IFlatPressureModifier> flatPressureModifiers = new List<IFlatPressureModifier>();
        public static void AddFlatPressureModifier(IFlatPressureModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.flatPressureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Pressure Modifier for body" + body + ".");
        }
        public static void AddFlatPressureModifier(IFlatPressureModifier modifier, CelestialBody body) => AddFlatPressureModifier(modifier, body.name);

        internal double GetPressure(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);
            if (altitude > mainbody.atmosphereDepth)
            {
                return 0.0;
            }
            double basepressure = basePressure != null ? basePressure.GetBasePressure(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : mainbody.GetPressure(altitude);
            basepressure = double.IsFinite(basepressure) ? basepressure : mainbody.GetPressure(altitude);
            double fractionalmodifier = 1.0;
            double flatmodifier = 0.0;
            foreach (IFractionalPressureModifier mod in fractionalPressureModifiers)
            {
                try
                {
                    double newmodifier = mod.GetFractionalPressureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                    {
                        fractionalmodifier += newmodifier;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            foreach (IFlatPressureModifier mod in flatPressureModifiers)
            {
                try
                {
                    double newmodifier = mod.GetFlatPressureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                    {
                        flatmodifier += newmodifier;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return Math.Max(Math.Max(basepressure * fractionalmodifier,0.0) + flatmodifier, 0.0);
        }
        #endregion

        #region temperature
        private IBaseTemperature baseTemperature;
        public static void SetBaseTemperature(IBaseTemperature bt, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            if (data.baseTemperature != null)
            {
                Utils.LogWarning("Base Temperature already exists for body " + body + ".");
            }
            else
            {
                data.baseTemperature = bt;
                string type = (bt.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Temperature for body" + body + ".");
            }
        }
        public static void SetBaseTemperature(IBaseTemperature bt, CelestialBody body) => SetBaseTemperature(bt, body.name);

        private List<IFractionalTemperatureModifier> fractionalTemperatureModifiers = new List<IFractionalTemperatureModifier>();
        public static void AddFractionalTemperatureModifier(IFractionalTemperatureModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.fractionalTemperatureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional Temperature Modifier for body" + body + ".");
        }
        public static void AddFractionalTemperatureModifier(IFractionalTemperatureModifier modifier, CelestialBody body) => AddFractionalTemperatureModifier(modifier, body.name);

        private List<IFlatTemperatureModifier> flatTemperatureModifiers = new List<IFlatTemperatureModifier>();
        public static void AddFlatTemperatureModifier(IFlatTemperatureModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.flatTemperatureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Temperature Modifier for body" + body + ".");
        }
        public static void AddFlatTemperatureModifier(IFlatTemperatureModifier modifier, CelestialBody body) => AddFlatTemperatureModifier(modifier, body.name);

        private List<IFractionalLatitudeBiasModifier> fractionalLatitudeBiasModifiers = new List<IFractionalLatitudeBiasModifier>();
        public static void AddFractionalLatitudeBiasModifier(IFractionalLatitudeBiasModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.fractionalLatitudeBiasModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional LatitudeBias Modifier for body" + body + ".");
        }
        public static void AddFractionalLatitudeBiasModifier(IFractionalLatitudeBiasModifier modifier, CelestialBody body) => AddFractionalLatitudeBiasModifier(modifier, body.name);

        private List<IFractionalLatitudeSunMultModifier> fractionalLatitudeSunMultModifiers = new List<IFractionalLatitudeSunMultModifier>();
        public static void AddFractionalLatitudeSunMultModifier(IFractionalLatitudeSunMultModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.fractionalLatitudeSunMultModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional LatitudeSunMult Modifier for body" + body + ".");
        }
        public static void AddFractionalLatitudeSunMultModifier(IFractionalLatitudeSunMultModifier modifier, CelestialBody body) => AddFractionalLatitudeSunMultModifier(modifier, body.name);

        private List<IFractionalAxialSunBiasModifier> fractionalAxialSunBiasModifiers = new List<IFractionalAxialSunBiasModifier>();
        public static void AddFractionalAxialSunBiasModifier(IFractionalAxialSunBiasModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.fractionalAxialSunBiasModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional AxialSunBias Modifier for body" + body + ".");
        }
        public static void AddFractionalAxialSunBiasModifier(IFractionalAxialSunBiasModifier modifier, CelestialBody body) => AddFractionalAxialSunBiasModifier(modifier, body.name);

        private List<IFractionalEccentricityBiasModifier> fractionalEccentricityBiasModifiers = new List<IFractionalEccentricityBiasModifier>();
        public static void AddFractionalEccentricityBiasModifier(IFractionalEccentricityBiasModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.fractionalEccentricityBiasModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional EccentricityBias Modifier for body" + body + ".");
        }
        public static void AddFractionalEccentricityBiasModifier(IFractionalEccentricityBiasModifier modifier, CelestialBody body) => AddFractionalEccentricityBiasModifier(modifier, body.name);

        public double GetTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);
            AtmoToolsReduxUtils.GetTemperatureWithComponents(mainbody, longitude, latitude, altitude, trueAnomaly, eccentricity, out double basetemp, out double baselatbias, out double baselatsunmult, out double baseaxialbias, out double baseeccentricitybias);
            double basetemperature = baseTemperature != null ? baseTemperature.GetBaseTemperature(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : basetemp;
            basetemperature = double.IsFinite(basetemperature) ? basetemperature : basetemp;
            double fractionalmodifier = 1.0;
            double flatmodifier = 0.0;
            foreach (IFractionalTemperatureModifier mod in fractionalTemperatureModifiers)
            {
                try
                {
                    double newmodifier = mod.GetFractionalTemperatureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                    {
                        fractionalmodifier += newmodifier;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            foreach (IFlatTemperatureModifier mod in flatTemperatureModifiers)
            {
                try
                {
                    double newmodifier = mod.GetFlatTemperatureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                    {
                        flatmodifier += newmodifier;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }

            // apply modifiers to the temperature components
            double latitudebias = 0.0;
            if (baseTemperature == null || !baseTemperature.DisableLatitudeBias)
            {
                double fractionallatitudebiasmodifier = 1.0;
                foreach (IFractionalLatitudeBiasModifier mod in fractionalLatitudeBiasModifiers)
                {
                    try
                    {
                        double newmodifier = mod.GetFractionalLatitudeBiasModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                        {
                            fractionallatitudebiasmodifier += newmodifier;
                        }    
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex.ToString());
                    }
                }
                latitudebias = baselatbias * fractionallatitudebiasmodifier;
            }

            double latitudesunmult = 0.0;
            if (baseTemperature == null || !baseTemperature.DisableLatitudeSunMult)
            {
                double fractionallatitudesunmultmodifier = 1.0;
                foreach (IFractionalLatitudeSunMultModifier mod in fractionalLatitudeSunMultModifiers)
                {
                    try
                    {
                        double newmodifier = mod.GetFractionalLatitudeSunMultModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                        {
                            fractionallatitudesunmultmodifier += newmodifier;
                        } 
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex.ToString());
                    }
                }
                latitudesunmult = baselatsunmult * fractionallatitudesunmultmodifier;
            }

            double axialsunbias = 0.0;
            if (baseTemperature == null || !baseTemperature.DisableAxialSunBias)
            {
                double fractionalaxialsunbiasmodifier = 1.0;
                foreach (IFractionalAxialSunBiasModifier mod in fractionalAxialSunBiasModifiers)
                {
                    try
                    {
                        double newmodifier = mod.GetFractionalAxialSunBiasModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                        {
                            fractionalaxialsunbiasmodifier += newmodifier;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex.ToString());
                    }
                }
                axialsunbias = baseaxialbias * fractionalaxialsunbiasmodifier;
            }

            double eccentricitybias = 0.0;
            if (baseTemperature == null || !baseTemperature.DisableEccentricityBias)
            {
                double fractionaleccentricitybiasmodifier = 1.0;
                foreach (IFractionalEccentricityBiasModifier mod in fractionalEccentricityBiasModifiers)
                {
                    try
                    {
                        double newmodifier = mod.GetFractionalEccentricityBiasModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                        {
                            fractionaleccentricitybiasmodifier += newmodifier;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex.ToString());
                    }
                }
                eccentricitybias = baseeccentricitybias * fractionaleccentricitybiasmodifier;
            }
            double temperatureoffset = (latitudebias + latitudesunmult + axialsunbias + eccentricitybias) * (double)mainbody.atmosphereTemperatureSunMultCurve.Evaluate((float)altitude);
            return Math.Max(Math.Max(basetemperature * fractionalmodifier, 0.0) + flatmodifier + temperatureoffset, 0.0);
        }
        #endregion

        #region molarmass
        private IBaseMolarMass baseMolarMass;
        public static void SetBaseMolarMass(IBaseMolarMass bmm, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            if (data.baseMolarMass != null)
            {
                Utils.LogWarning("Base Molar Mass already exists for body " + body + ".");
            }
            else
            {
                data.baseMolarMass = bmm;
                string type = (bmm.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Molar Mass for body" + body + ".");
            }
        }
        public static void SetBaseMolarMass(IBaseMolarMass bmm, CelestialBody body) => SetBaseMolarMass(bmm, body.name);

        private List<IFlatMolarMassModifier> flatMolarMassModifiers = new List<IFlatMolarMassModifier>();
        public static void AddFlatMolarMassModifier(IFlatMolarMassModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.flatMolarMassModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Molar Mass Modifier for body" + body + ".");
        }
        public static void AddFlatMolarMassModifier(IFlatMolarMassModifier modifier, CelestialBody body) => AddFlatMolarMassModifier(modifier, body.name);

        public double GetMolarMass(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);
            double basemolarmass = baseMolarMass != null ? baseMolarMass.GetBaseMolarMass(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : mainbody.atmosphereMolarMass;
            basemolarmass = double.IsFinite(basemolarmass) ? basemolarmass : mainbody.atmosphereMolarMass;
            double flatmodifier = 0.0;
            foreach (IFlatMolarMassModifier mod in flatMolarMassModifiers)
            {
                try
                {
                    double newmodifier = mod.GetFlatMolarMassModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                    {
                        flatmodifier += newmodifier;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return Math.Max(basemolarmass + flatmodifier, 0.0);
        }
        #endregion

        #region adiabaticindex
        private IBaseAdiabaticIndex baseAdiabaticIndex;
        public static void SetBaseAdiabaticIndex(IBaseAdiabaticIndex bai, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            if (data.baseAdiabaticIndex != null)
            {
                Utils.LogWarning("Base Adiabatic Index already exists for body " + body + ".");
            }
            else
            {
                data.baseAdiabaticIndex = bai;
                string type = (bai.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Adiabatic Index for body" + body + ".");
            }
        }
        public static void SetBaseAdiabaticIndex(IBaseAdiabaticIndex bai, CelestialBody body) => SetBaseAdiabaticIndex(bai, body.name);

        private List<IFlatAdiabaticIndexModifier> flatAdiabaticIndexModifiers = new List<IFlatAdiabaticIndexModifier>();
        public static void AddFlatAdiabaticIndexModifier(IFlatAdiabaticIndexModifier modifier, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            data.flatAdiabaticIndexModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Adiabatic Index Modifier for body" + body + ".");
        }
        public static void AddFlatAdiabaticIndexModifier(IFlatAdiabaticIndexModifier modifier, CelestialBody body) => AddFlatAdiabaticIndexModifier(modifier, body.name);

        public double GetAdiabaticIndex(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            CelestialBody mainbody = FlightGlobals.GetBodyByName(body);
            double baseadiabaticindex = baseAdiabaticIndex != null ? baseAdiabaticIndex.GetBaseAdiabaticIndex(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : mainbody.atmosphereAdiabaticIndex;
            baseadiabaticindex = double.IsFinite(baseadiabaticindex) ? baseadiabaticindex : mainbody.atmosphereAdiabaticIndex;
            double flatmodifier = 0.0;
            foreach (IFlatAdiabaticIndexModifier mod in flatAdiabaticIndexModifiers)
            {
                try
                {
                    double newmodifier = mod.GetFlatAdiabaticIndexModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    if (double.IsFinite(newmodifier) && newmodifier != 0.0)
                    {
                        flatmodifier += newmodifier;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return Math.Max(baseadiabaticindex + flatmodifier, 0.0);
        }
        #endregion

        #region unsafeatmo
        private IUnsafeAtmosphereIndicator unsafeAtmosphereIndicator;
        public static void SetUnsafeAtmosphereIndicator(IUnsafeAtmosphereIndicator uai, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            if (data.unsafeAtmosphereIndicator != null)
            {
                Utils.LogWarning("Unsafe Atmosphere Indicator already exists for body " + body + ".");
            }
            else 
            {
                data.unsafeAtmosphereIndicator = uai;
                string type = (uai.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Unsafe Atmosphere Indicator for body" + body + ".");
            }
        }
        public static void SetUnsafeAtmosphereIndicator(IUnsafeAtmosphereIndicator uai, CelestialBody body) => SetUnsafeAtmosphereIndicator(uai, body.name);

        public bool CheckAtmosphereUnsafe(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity, out string unsafeAtmoMessage)
        {
            try
            {
                bool isatmoUnsafe = (unsafeAtmosphereIndicator != null) && unsafeAtmosphereIndicator.IsAtmosphereUnsafe(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                unsafeAtmoMessage = (unsafeAtmosphereIndicator != null) ? unsafeAtmosphereIndicator.UnsafeAtmosphereMessage : string.Empty;
                return isatmoUnsafe;
            }
            catch
            {
                unsafeAtmoMessage = string.Empty;
                return false;
            }
        }
        #endregion

        #region intakechoking
        private IAirIntakeChokeFactor intakechokefactor;

        public static void SetAirIntakeChokeFactor(IAirIntakeChokeFactor aicf, string body)
        {
            AtmoToolsRedux_Data data = GetOrCreateAtmosphereData(body);
            if (data.intakechokefactor != null)
            {
                Utils.LogWarning("Air Intake Choke Factor already exists for body " + body + ".");
            }
            else
            {
                data.intakechokefactor = aicf;
                string type = (aicf.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as an Air Intake Choke Factor for body" + body + ".");
            }
        }
        public static void SetAirIntakeChokeFactor(IAirIntakeChokeFactor aicf, CelestialBody body) => SetAirIntakeChokeFactor(aicf, body.name);

        public double GetAirIntakeChokeFactor(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            return intakechokefactor != null ? intakechokefactor.GetAirIntakeChokeFactor(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : 0.0;
        }
        #endregion

        #region purging
        private void PurgeItem(object item)
        {
            if (item == null)
            {
                return;
            }

            if (item is IWindProvider provider && windProviders.Contains(provider))
            {
                windProviders.Remove(provider);
            }
            if (item is IOceanCurrentProvider oceanprovider && oceanCurrentProviders.Contains(oceanprovider))
            {
                oceanCurrentProviders.Remove(oceanprovider);
            }

            if (item is IBasePressure bp && basePressure != null && bp == basePressure)
            {
                basePressure = null;
            }
            if (item is IFractionalPressureModifier fracpress && fractionalPressureModifiers.Contains(fracpress))
            {
                fractionalPressureModifiers.Remove(fracpress);
            }
            if (item is IFlatPressureModifier flatpress && flatPressureModifiers.Contains(flatpress))
            {
                flatPressureModifiers.Remove(flatpress);
            }

            if (item is IBaseTemperature bt && baseTemperature != null && bt == baseTemperature)
            {
                baseTemperature = null;
            }
            if (item is IFractionalTemperatureModifier fractemp && fractionalTemperatureModifiers.Contains(fractemp))
            {
                fractionalTemperatureModifiers.Remove(fractemp);
            }
            if (item is IFlatTemperatureModifier flattemp && flatTemperatureModifiers.Contains(flattemp))
            {
                flatTemperatureModifiers.Remove(flattemp);
            }
            if (item is IFractionalLatitudeBiasModifier fraclatbias && fractionalLatitudeBiasModifiers.Contains(fraclatbias))
            {
                fractionalLatitudeBiasModifiers.Remove(fraclatbias);
            }
            if (item is IFractionalLatitudeSunMultModifier fraclatsunmult && fractionalLatitudeSunMultModifiers.Contains(fraclatsunmult))
            {
                fractionalLatitudeSunMultModifiers.Remove(fraclatsunmult);
            }
            if (item is IFractionalAxialSunBiasModifier fracaxialsunbias && fractionalAxialSunBiasModifiers.Contains(fracaxialsunbias))
            {
                fractionalAxialSunBiasModifiers.Remove(fracaxialsunbias);
            }
            if (item is IFractionalEccentricityBiasModifier fraceccbias && fractionalEccentricityBiasModifiers.Contains(fraceccbias))
            {
                fractionalEccentricityBiasModifiers.Remove(fraceccbias);
            }

            if (item is IBaseMolarMass bmm && baseMolarMass != null && bmm == baseMolarMass)
            {
                baseMolarMass = null;
            }
            if (item is IFlatMolarMassModifier flatmm && flatMolarMassModifiers.Contains(flatmm))
            {
                flatMolarMassModifiers.Remove(flatmm);
            }

            if (item is IBaseAdiabaticIndex bai && baseAdiabaticIndex != null && bai == baseAdiabaticIndex)
            {
                baseAdiabaticIndex = null;
            }
            if (item is IFlatAdiabaticIndexModifier flatai && flatAdiabaticIndexModifiers.Contains(flatai))
            {
                flatAdiabaticIndexModifiers.Remove(flatai);
            }

            if (item is IUnsafeAtmosphereIndicator uai && unsafeAtmosphereIndicator != null && uai == unsafeAtmosphereIndicator)
            {
                unsafeAtmosphereIndicator = null;
            }
        }

        internal static void PurgeFromAll(object obj)
        {
            if (AtmoData != null)
            {
                foreach (KeyValuePair<string, AtmoToolsRedux_Data> kvp in AtmoData)
                {
                    AtmoData[kvp.Key].PurgeItem(obj);
                }
            }
        }
        #endregion
    }
}
