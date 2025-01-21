using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    public sealed class AtmosphereData : MonoBehaviour
    {
        public CelestialBody body;

        public void Setup(CelestialBody body)
        {
            this.body = body;

            windProviders = new List<IWindProvider>();
            oceanCurrentProviders = new List<IOceanCurrentProvider>();

            fractionalPressureModifiers = new List<IFractionalPressureModifier>();
            flatPressureModifiers = new List<IFlatPressureModifier>();

            fractionalTemperatureModifiers = new List<IFractionalTemperatureModifier>();
            flatTemperatureModifiers = new List<IFlatTemperatureModifier>();
            fractionalLatitudeBiasModifiers = new List<IFractionalLatitudeBiasModifier>();
            fractionalLatitudeSunMultModifiers = new List<IFractionalLatitudeSunMultModifier>();
            fractionalAxialSunBiasModifiers = new List<IFractionalAxialSunBiasModifier>();
            fractionalEccentricityBiasModifiers = new List<IFractionalEccentricityBiasModifier>();

            flatMolarMassModifiers = new List<IFlatMolarMassModifier>();
            flatAdiabaticIndexModifiers = new List<IFlatAdiabaticIndexModifier>();
        }

        //excessive use of LINQ
        public void CleanupModifiers()
        {
            IEnumerable<IWindProvider> WP = from provider in windProviders where ((AtmosphereModifier)provider).Initialized select provider;
            windProviders = WP.ToList();

            IEnumerable<IOceanCurrentProvider> OCP = from provider in oceanCurrentProviders where ((AtmosphereModifier)provider).Initialized select provider;
            oceanCurrentProviders = OCP.ToList();

            if (basePressure != null && !((AtmosphereModifier)basePressure).Initialized)
            {
                basePressure = null;
            }

            IEnumerable<IFractionalPressureModifier> fractalpressmod = from mod in fractionalPressureModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            fractionalPressureModifiers = fractalpressmod.ToList();

            IEnumerable<IFlatPressureModifier> flatpressmod = from mod in flatPressureModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            flatPressureModifiers = flatpressmod.ToList();

            if (baseTemperature != null && !((AtmosphereModifier)baseTemperature).Initialized)
            {
                baseTemperature = null;
            }

            IEnumerable<IFractionalTemperatureModifier> fractaltempmod = from mod in fractionalTemperatureModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            fractionalTemperatureModifiers = fractaltempmod.ToList();

            IEnumerable<IFlatTemperatureModifier> flattempmod = from mod in flatTemperatureModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            flatTemperatureModifiers = flattempmod.ToList();

            IEnumerable<IFractionalLatitudeBiasModifier> fractallatbias = from mod in fractionalLatitudeBiasModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            fractionalLatitudeBiasModifiers = fractallatbias.ToList();

            IEnumerable<IFractionalLatitudeSunMultModifier> fractallatsunmult = from mod in fractionalLatitudeSunMultModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            fractionalLatitudeSunMultModifiers = fractallatsunmult.ToList();

            IEnumerable<IFractionalAxialSunBiasModifier> fractalaxialsunbias = from mod in fractionalAxialSunBiasModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            fractionalAxialSunBiasModifiers = fractalaxialsunbias.ToList();

            IEnumerable<IFractionalEccentricityBiasModifier> fractaleccentricitybias = from mod in fractionalEccentricityBiasModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            fractionalEccentricityBiasModifiers = fractaleccentricitybias.ToList();

            if (baseMolarMass != null && !((AtmosphereModifier)baseMolarMass).Initialized)
            {
                baseMolarMass = null;
            }

            IEnumerable<IFlatMolarMassModifier> flatmolarmassmod = from mod in flatMolarMassModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            flatMolarMassModifiers = flatmolarmassmod.ToList();

            if (baseAdiabaticIndex != null && !((AtmosphereModifier)baseAdiabaticIndex).Initialized)
            {
                baseAdiabaticIndex = null;
            }

            IEnumerable<IFlatAdiabaticIndexModifier> flatadiabaticmod = from mod in flatAdiabaticIndexModifiers where ((AtmosphereModifier)mod).Initialized select mod;
            flatAdiabaticIndexModifiers = flatadiabaticmod.ToList();
        }

        public double maxTempAngleOffset;

        #region windandoceancurrents
        private List<IWindProvider> windProviders;
        public void AddWindProvider(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IWindProvider provider)
            {
                windProviders.Add(provider);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Wind Providerfor body " + body.name + ". " + type + " does not implement the interface IWindProvider.");
            }
        }
        internal Vector3 GetWindVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            Vector3 windvec = Vector3.zero;
            foreach (IWindProvider provider in windProviders)
            {
                try
                {
                    Vector3 bodywindvec = provider.GetWindVector(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    windvec.Add(bodywindvec);
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return windvec;
        }

        private List<IOceanCurrentProvider> oceanCurrentProviders;
        public void AddOceanCurrentProvider(AtmosphereModifier modifier)
        {
            modifier.Body = body; 
            if (modifier is IOceanCurrentProvider provider)
            {
                oceanCurrentProviders.Add(provider);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as an Ocean Current Provider for body " + body.name + ". " + type + " does not implement the interface IOceanCurrentProvider.");
            }
        }
        internal Vector3 GetOceanCurrentVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            Vector3 oceanvec = Vector3.zero;
            foreach (IOceanCurrentProvider provider in oceanCurrentProviders)
            {
                try
                {
                    Vector3 bodywindvec = provider.GetOceanCurrentVector(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                    oceanvec.Add(bodywindvec);
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
        public void SetBasePressure(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (basePressure != null)
            {
                Utils.LogWarning("Base Pressure already exists for body " + body.name + ".");
            }
            else if (modifier is IBasePressure bp)
            {
                basePressure = bp;
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Base Pressure for body " + body.name + ". " + type + " does not implement the interface IBasePressure.");
            }
        }
        private List<IFractionalPressureModifier> fractionalPressureModifiers;
        public void AddFractionalPressureModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFractionalPressureModifier mod)
            {
                fractionalPressureModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Fractional Pressure Modifier for body " + body.name + ". " + type + " does not implement the interface IFractionalPressureModifier.");
            }
        }
        private List<IFlatPressureModifier> flatPressureModifiers;
        public void AddFlatPressureModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFlatPressureModifier mod)
            {
                flatPressureModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Flat Pressure Modifier for body " + body.name + ". " + type + " does not implement the interface IFlatPressureModifier.");
            }
        }

        internal double GetPressure(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            double basepressure = basePressure != null ? basePressure.GetBasePressure(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : body.GetPressure(altitude);
            double fractionalmodifier = 1.0;
            double flatmodifier = 0.0;
            foreach (IFractionalPressureModifier mod in fractionalPressureModifiers)
            {
                try
                {
                    fractionalmodifier += mod.GetFractionalPressureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
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
                    flatmodifier += mod.GetFlatPressureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
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
        public void SetBaseTemperature(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (baseTemperature != null)
            {
                Utils.LogWarning("Base Temperature already exists for body " + body.name + ".");
            }
            else if (modifier is IBaseTemperature bt)
            {
                baseTemperature = bt;
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Base Temperature for body " + body.name + ". " + type + " does not implement the interface IBaseTemperature.");
            }
        }
        private List<IFractionalTemperatureModifier> fractionalTemperatureModifiers;
        public void AddFractionalTemperatureModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFractionalTemperatureModifier mod)
            {
                fractionalTemperatureModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Fractional Temperature Modifier for body " + body.name + ". " + type + " does not implement the interface IFractionalTemperatureModifier.");
            }
        }
        private List<IFlatTemperatureModifier> flatTemperatureModifiers;
        public void AddFlatTemperatureModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFlatTemperatureModifier mod)
            {
                flatTemperatureModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Flat Temperature Modifier for body " + body.name + ". " + type + " does not implement the interface IFlatTemperatureModifier.");
            }
        }
        private List<IFractionalLatitudeBiasModifier> fractionalLatitudeBiasModifiers;
        public void AddFractionalLatitudeBiasModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFractionalLatitudeBiasModifier mod)
            {
                fractionalLatitudeBiasModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Fractional LatitudeBias Modifier for body " + body.name + ". " + type + " does not implement the interface IFractionalLatitudeBiasModifier.");
            }
        }
        private List<IFractionalLatitudeSunMultModifier> fractionalLatitudeSunMultModifiers;
        public void AddFractionalLatitudeSunMultModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFractionalLatitudeSunMultModifier mod)
            {
                fractionalLatitudeSunMultModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Fractional LatitudeSunMult Modifier for body " + body.name + ". " + type + " does not implement the interface IFractionalLatitudeSunMultModifier.");
            }
        }
        private List<IFractionalAxialSunBiasModifier> fractionalAxialSunBiasModifiers;
        public void AddFractionalAxialSunBiasModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFractionalAxialSunBiasModifier mod)
            {
                fractionalAxialSunBiasModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Fractional AxialSunBias Modifier for body " + body.name + ". " + type + " does not implement the interface IFractionalAxialSunBiasModifier.");
            }
        }
        private List<IFractionalEccentricityBiasModifier> fractionalEccentricityBiasModifiers;
        public void AddFractionalEccentricityBiasModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFractionalEccentricityBiasModifier mod)
            {
                fractionalEccentricityBiasModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Fractional EccentricityBias Modifier for body " + body.name + ". " + type + " does not implement the interface IFractionalEccentricityBiasModifier.");
            }
        }

        public double GetTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            double basetemperature = baseTemperature != null ? baseTemperature.GetBaseTemperature(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : body.GetTemperature(altitude);
            double fractionalmodifier = 1.0;
            double flatmodifier = 0.0;
            foreach (IFractionalTemperatureModifier mod in fractionalTemperatureModifiers)
            {
                try
                {
                    fractionalmodifier += mod.GetFractionalTemperatureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
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
                    flatmodifier += mod.GetFlatTemperatureModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            double temperatureoffset = 0.0;
            if (body != FlightIntegrator.sunBody)
            {
                Vector3d position = ScaledSpace.LocalToScaledSpace(body.GetWorldSurfacePosition(latitude, longitude, altitude));
                CelestialBody localstar = PublicUtils.GetLocalStar(body);
                if (localstar != null)
                {
                    localstar = FlightIntegrator.sunBody;
                }
                Vector3d localstarposition = localstar.scaledBody.transform.position;
                Vector3d sunVector = localstarposition - position;
                double magnitude = sunVector.magnitude;
                if (magnitude == 0.0)
                {
                    magnitude = 1.0;
                }
                Vector3d normalSunVector = sunVector / magnitude;
                Vector3d up = body.bodyTransform.up;
                Vector3d upAxis = body.GetRelSurfaceNVector(latitude, longitude);

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

                double sunmult = (1.0 + (double)Vector3.Dot((Vector3)sunVector, Quaternion.AngleAxis((float)maxTempAngleOffset * Mathf.Sign((float)body.rotationPeriod), up) * (Vector3)upAxis)) * 0.5;
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

                double latitudebias = 0.0;
                if (baseTemperature != null && !baseTemperature.DisableLatitudeBias)
                {
                    double fractionallatitudebiasmodifier = 1.0;
                    foreach (IFractionalLatitudeBiasModifier mod in fractionalLatitudeBiasModifiers)
                    {
                        try
                        {
                            fractionallatitudebiasmodifier += mod.GetFractionalLatitudeBiasModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                    }
                    latitudebias = (double)body.latitudeTemperatureBiasCurve.Evaluate((float)Math.Abs(latitude)) * fractionallatitudebiasmodifier;
                }

                double latitudesunmult = 0.0;
                if (baseTemperature != null && !baseTemperature.DisableLatitudeSunMult)
                {
                    double fractionallatitudesunmultmodifier = 1.0;
                    foreach (IFractionalLatitudeSunMultModifier mod in fractionalLatitudeSunMultModifiers)
                    {
                        try
                        {
                            fractionallatitudesunmultmodifier += mod.GetFractionalLatitudeSunMultModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                    }
                    latitudebias = (double)body.latitudeTemperatureSunMultCurve.Evaluate((float)Math.Abs(latitude)) * num9 * fractionallatitudesunmultmodifier;
                }

                double axialsunbias = 0.0;
                if (baseTemperature != null && !baseTemperature.DisableAxialSunBias)
                {
                    double fractionalaxialsunbiasmodifier = 1.0;
                    foreach (IFractionalAxialSunBiasModifier mod in fractionalAxialSunBiasModifiers)
                    {
                        try
                        {
                            fractionalaxialsunbiasmodifier += mod.GetFractionalAxialSunBiasModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                    }
                    axialsunbias = (double)body.axialTemperatureSunBiasCurve.Evaluate((float)trueAnomaly) * (double)body.axialTemperatureSunMultCurve.Evaluate((float)Math.Abs(latitude)) * fractionalaxialsunbiasmodifier;
                }

                double eccentricitybias = 0.0;
                if (baseTemperature != null && !baseTemperature.DisableEccentricityBias)
                {
                    double fractionaleccentricitybiasmodifier = 1.0;
                    foreach (IFractionalEccentricityBiasModifier mod in fractionalEccentricityBiasModifiers)
                    {
                        try
                        {
                            fractionaleccentricitybiasmodifier += mod.GetFractionalEccentricityBiasModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                    }
                    eccentricitybias = (double)body.eccentricityTemperatureBiasCurve.Evaluate((float)eccentricity) * fractionaleccentricitybiasmodifier;
                }
                temperatureoffset = (latitudebias + latitudesunmult + axialsunbias + eccentricitybias) * (double)body.atmosphereTemperatureSunMultCurve.Evaluate((float)altitude);
            }
            return Math.Max(Math.Max(basetemperature * fractionalmodifier,0.0) + flatmodifier + temperatureoffset, 0.0);
        }
        #endregion

        #region molarmass
        private IBaseMolarMass baseMolarMass;
        public void SetBaseMolarMass(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (baseMolarMass != null)
            {
                Utils.LogWarning("Base Molar Mass already exists for body " + body.name + ".");
            }
            else if (modifier is IBaseMolarMass bmm)
            {
                baseMolarMass = bmm;
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Base Molar Mass for body " + body.name + ". " + type + " does not implement the interface IBaseMolarMass.");
            }
        }
        private List<IFlatMolarMassModifier> flatMolarMassModifiers;
        public void AddFlatMolarMassModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFlatMolarMassModifier mod)
            {
                flatMolarMassModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Flat Molar Mass Modifier for body " + body.name + ". " + type + " does not implement the interface IFlatMolarMassModifier.");
            }
        }

        public double GetMolarMass(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            double basemolarmass = baseMolarMass != null ? baseMolarMass.GetBaseMolarMass(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : body.atmosphereMolarMass;
            double flatmodifier = 0.0;
            foreach (IFlatMolarMassModifier mod in flatMolarMassModifiers)
            {
                try
                {
                    flatmodifier += mod.GetFlatMolarMassModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
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
        public void SetBaseAdiabaticIndex(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (baseAdiabaticIndex != null)
            {
                Utils.LogWarning("Base Adiabatic Index already exists for body " + body.name + ".");
            }
            else if (modifier is IBaseAdiabaticIndex bai)
            {
                baseAdiabaticIndex = bai;
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Base Adiabatic Index for body " + body.name + ". " + type + " does not implement the interface IBaseAdiabaticIndex.");
            }
        }
        private List<IFlatAdiabaticIndexModifier> flatAdiabaticIndexModifiers;
        public void AddFlatAdiabaticIndexModifier(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (modifier is IFlatAdiabaticIndexModifier mod)
            {
                flatAdiabaticIndexModifiers.Add(mod);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Flat Adiabatic Index Modifier for body " + body.name + ". " + type + " does not implement the interface IFlatAdiabaticIndexModifier.");
            }
        }

        public double GetAdiabaticIndex(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            double baseadiabaticindex = baseAdiabaticIndex != null ? baseAdiabaticIndex.GetBaseAdiabaticIndex(longitude, latitude, altitude, time, trueAnomaly, eccentricity) : body.atmosphereMolarMass;
            double flatmodifier = 0.0;
            foreach (IFlatAdiabaticIndexModifier mod in flatAdiabaticIndexModifiers)
            {
                try
                {
                    flatmodifier += mod.GetFlatAdiabaticIndexModifier(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            return Math.Max(baseadiabaticindex + flatmodifier, 0.0);
        }
        #endregion

        #region toxicatmo
        private IToxicAtmosphereIndicator toxicAtmosphereIndicator;
        public void SetToxicAtmosphereIndicator(AtmosphereModifier modifier)
        {
            modifier.Body = body;
            if (toxicAtmosphereIndicator != null)
            {
                Utils.LogWarning("Toxic Atmosphere Indicator already exists for body " + body.name + ".");
            }
            else if (modifier is IToxicAtmosphereIndicator tai)
            {
                toxicAtmosphereIndicator = tai;
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Toxic Atmosphere Indicator for body " + body.name + ". " + type + " does not implement the interface IToxicAtmosphereIndicator.");
            }
        }

        public bool CheckToxicAtmosphere(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity)
        {
            return (toxicAtmosphereIndicator != null) && toxicAtmosphereIndicator.IsAtmosphereToxic(longitude, latitude, altitude, time, trueAnomaly, eccentricity);
        }
        #endregion
    }
}
