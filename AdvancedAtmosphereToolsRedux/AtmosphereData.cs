using System;
using System.Collections.Generic;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    //You may absolutely try to access this thing directly if you so desire.
    public sealed class AtmosphereData
    {
        private static Dictionary<string, AtmosphereData> AtmoData;

        //Only use this if you want to add something to a body.
        public static AtmosphereData GetOrCreateAtmosphereData(CelestialBody body)
        {            
            if (AtmoData == null)
            {
                AtmoData = new Dictionary<string, AtmosphereData>();
            }
            if (!AtmoData.ContainsKey(body.name)) 
            {
                AtmoData.Add(body.name, new AtmosphereData(body));
            }
            return AtmoData[body.name];
        }

        public static AtmosphereData GetAtmosphereData(CelestialBody body) => AtmoData != null && AtmoData.ContainsKey(body.name) ? AtmoData[body.name] : null;

        private string body;

        public AtmosphereData(CelestialBody body)
        {
            Utils.LogInfo("Creating a new AtmosphereData holder for " + body.name + ".");
            this.body = body.name;
        }

        private double maxtempangleoffset = AtmoToolsReduxUtils.DefaultMaxTempAngleOffset;
        public double MaxTempAngleOffset
        {
            get => maxtempangleoffset;
            internal set => maxtempangleoffset = value;
        }

        #region windandoceancurrents
        private List<IWindProvider> windProviders = new List<IWindProvider>();
        public void AddWindProvider(IWindProvider provider)
        {
            windProviders.Add(provider);
            string type = (provider.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Wind Provider for body" + body + ".");
        }

        internal Vector3 GetWindVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
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

        //Ocean-related stuff is unused for the time being
        private double oceanbulkmodulus = AtmoToolsReduxUtils.WaterBulkModulus; //in kPa
        public double OceanBulkModulus
        {
            get => oceanbulkmodulus;
            internal set => oceanbulkmodulus = value;
        }

        private List<IOceanCurrentProvider> oceanCurrentProviders = new List<IOceanCurrentProvider>();
        public void AddOceanCurrentProvider(IOceanCurrentProvider provider)
        {
            oceanCurrentProviders.Add(provider);
            string type = (provider.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as an Ocean Current Provider for body" + body + ".");
        }

        internal Vector3 GetOceanCurrentVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
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
        }
        #endregion

        #region pressure
        private IBasePressure basePressure;
        public void SetBasePressure(IBasePressure bp)
        {
            if (basePressure != null)
            {
                Utils.LogWarning("Base Pressure already exists for body " + body + ".");
            }
            else
            {
                basePressure = bp;
                string type = (bp.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Pressure for body" + body + ".");
            }
        }

        private List<IFractionalPressureModifier> fractionalPressureModifiers = new List<IFractionalPressureModifier>();
        public void AddFractionalPressureModifier(IFractionalPressureModifier modifier)
        {
            fractionalPressureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional Pressure Modifier for body" + body + ".");
        }

        private List<IFlatPressureModifier> flatPressureModifiers = new List<IFlatPressureModifier>();
        public void AddFlatPressureModifier(IFlatPressureModifier modifier)
        {
            flatPressureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Pressure Modifier for body" + body + ".");
        }

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
        public void SetBaseTemperature(IBaseTemperature bt)
        {
            if (baseTemperature != null)
            {
                Utils.LogWarning("Base Temperature already exists for body " + body + ".");
            }
            else
            {
                baseTemperature = bt;
                string type = (bt.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Temperature for body" + body + ".");
            }
        }

        private List<IFractionalTemperatureModifier> fractionalTemperatureModifiers = new List<IFractionalTemperatureModifier>();
        public void AddFractionalTemperatureModifier(IFractionalTemperatureModifier modifier)
        {
            fractionalTemperatureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional Temperature Modifier for body" + body + ".");
        }

        private List<IFlatTemperatureModifier> flatTemperatureModifiers = new List<IFlatTemperatureModifier>();
        public void AddFlatTemperatureModifier(IFlatTemperatureModifier modifier)
        {
            flatTemperatureModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Temperature Modifier for body" + body + ".");
        }

        private List<IFractionalLatitudeBiasModifier> fractionalLatitudeBiasModifiers = new List<IFractionalLatitudeBiasModifier>();
        public void AddFractionalLatitudeBiasModifier(IFractionalLatitudeBiasModifier modifier)
        {
            fractionalLatitudeBiasModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional LatitudeBias Modifier for body" + body + ".");
        }

        private List<IFractionalLatitudeSunMultModifier> fractionalLatitudeSunMultModifiers = new List<IFractionalLatitudeSunMultModifier>();
        public void AddFractionalLatitudeSunMultModifier(IFractionalLatitudeSunMultModifier modifier)
        {
            fractionalLatitudeSunMultModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional LatitudeSunMult Modifier for body" + body + ".");
        }

        private List<IFractionalAxialSunBiasModifier> fractionalAxialSunBiasModifiers = new List<IFractionalAxialSunBiasModifier>();
        public void AddFractionalAxialSunBiasModifier(IFractionalAxialSunBiasModifier modifier)
        {
            fractionalAxialSunBiasModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional AxialSunBias Modifier for body" + body + ".");
        }

        private List<IFractionalEccentricityBiasModifier> fractionalEccentricityBiasModifiers = new List<IFractionalEccentricityBiasModifier>();
        public void AddFractionalEccentricityBiasModifier(IFractionalEccentricityBiasModifier modifier)
        {
            fractionalEccentricityBiasModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Fractional EccentricityBias Modifier for body" + body + ".");
        }

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
        public void SetBaseMolarMass(IBaseMolarMass bmm)
        {
            if (baseMolarMass != null)
            {
                Utils.LogWarning("Base Molar Mass already exists for body " + body + ".");
            }
            else
            {
                baseMolarMass = bmm;
                string type = (bmm.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Molar Mass for body" + body + ".");
            }
        }

        private List<IFlatMolarMassModifier> flatMolarMassModifiers = new List<IFlatMolarMassModifier>();
        public void AddFlatMolarMassModifier(IFlatMolarMassModifier modifier)
        {
            flatMolarMassModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Molar Mass Modifier for body" + body + ".");
        }

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
        public void SetBaseAdiabaticIndex(IBaseAdiabaticIndex bai)
        {
            if (baseAdiabaticIndex != null)
            {
                Utils.LogWarning("Base Adiabatic Index already exists for body " + body + ".");
            }
            else
            {
                baseAdiabaticIndex = bai;
                string type = (bai.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Base Adiabatic Index for body" + body + ".");
            }
        }

        private List<IFlatAdiabaticIndexModifier> flatAdiabaticIndexModifiers = new List<IFlatAdiabaticIndexModifier>();
        public void AddFlatAdiabaticIndexModifier(IFlatAdiabaticIndexModifier modifier)
        {
            flatAdiabaticIndexModifiers.Add(modifier);
            string type = (modifier.GetType()).ToString();
            Utils.LogInfo("Added type " + type + " as a Flat Adiabatic Index Modifier for body" + body + ".");
        }

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
        public void SetUnsafeAtmosphereIndicator(IUnsafeAtmosphereIndicator uai)
        {
            if (unsafeAtmosphereIndicator != null)
            {
                Utils.LogWarning("Unsafe Atmosphere Indicator already exists for body " + body + ".");
            }
            else 
            {
                unsafeAtmosphereIndicator = uai;
                string type = (uai.GetType()).ToString();
                Utils.LogInfo("Added type " + type + " as a Unsafe Atmosphere Indicator for body" + body + ".");
            }
        }

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
                foreach (KeyValuePair<string, AtmosphereData> kvp in AtmoData)
                {
                    AtmoData[kvp.Key].PurgeItem(obj);
                }
            }
        }
        #endregion
    }
}
