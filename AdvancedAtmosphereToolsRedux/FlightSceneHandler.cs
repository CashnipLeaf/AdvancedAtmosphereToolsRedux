using System;
using System.Reflection;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    //Delegates for FAR
    using WindDelegate = Func<CelestialBody, Part, Vector3, Vector3>;
    using PropertyDelegate = Func<CelestialBody, Vector3d, double, double>;

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightSceneHandler : MonoBehaviour
    {
        void Awake()
        {
            if (Settings.FAR_Exists)
            {
                Utils.LogInfo("Registering AdvAtmoTools:Redux with FerramAerospaceResearch.");
                RegisterWithFAR();
            }
        }

        void FixedUpdate() => Settings.CheckGameSettings();

        #region FARCompatibility
        internal Vector3 GetTheWind(CelestialBody body, Part p, Vector3 pos)
        {
            AtmoToolsRedux_VesselHandler VH = AtmoToolsRedux_VesselHandler.GetVesselHandler(p.vessel);
            return VH != null ? Vector3.Lerp(VH.InternalAppliedWind, Vector3.zero, (float)(p.submergedPortion * p.submergedPortion)) : Vector3.zero;
        }
        internal double GetTheTemperature(CelestialBody body, Vector3d pos, double time)
        {
            body.GetLatLonAlt(pos, out double lat, out double lon, out double alt);
            AtmoToolsReduxUtils.GetTrueAnomalyEccentricity(body, out double trueAnomaly, out double eccentricity);
            AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
            return data != null ? data.GetTemperature(lon, lat, alt, time, trueAnomaly, eccentricity) : AtmoToolsReduxUtils.GetTemperatureAtPosition(body, lon, lat, alt, trueAnomaly, eccentricity);
        }
        internal double GetThePressure(CelestialBody body, Vector3d pos, double time)
        {
            body.GetLatLonAlt(pos, out double lat, out double lon, out double alt);
            AtmoToolsReduxUtils.GetTrueAnomalyEccentricity(body, out double trueAnomaly, out double eccentricity);
            AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
            return data != null ? data.GetPressure(lon, lat, alt, time, trueAnomaly, eccentricity) : body.GetPressure(alt);
        }

        internal bool RegisterWithFAR() //Register AdvAtmoTools:Redux with FAR.
        {
            try
            {
                Type FARWindFunc = null;
                Type FARAtm = null;
                foreach (var assembly in AssemblyLoader.loadedAssemblies)
                {
                    if (assembly.name == "FerramAerospaceResearch")
                    {
                        var types = assembly.assembly.GetExportedTypes();
                        foreach (Type t in types)
                        {
                            if (t.FullName.Equals("FerramAerospaceResearch.FARWind"))
                            {
                                FARAtm = t;
                            }
                            if (t.FullName.Equals("FerramAerospaceResearch.FARWind+WindFunction"))
                            {
                                FARWindFunc = t;
                            }
                            if (t.FullName.Equals("FerramAerospaceResearch.FARAtmosphere"))
                            {
                                FARAtm = t;
                            }
                        }
                    }
                }
                if (FARAtm == null)
                {
                    Utils.LogWarning("Unable to register with FerramAerospaceResearch.");
                    return false;
                }
                if (FARWindFunc != null) //Check if an older version of FAR is installed
                {
                    //Get FAR Wind Method
                    MethodInfo SetWindFunction = FARAtm.GetMethod("SetWindFunction");
                    if (SetWindFunction == null)
                    {
                        Utils.LogWarning("Unable to register with FerramAerospaceResearch.");
                        return false;
                    }
                    //Set FARWind function
                    Utils.LogInfo("An older version of FerramAerospaceResearch is installed. Temperature and Pressure data will not be available to FAR.");
                    var del = Delegate.CreateDelegate(FARWindFunc, this, typeof(FlightSceneHandler).GetMethod("GetTheWind"), true);
                    SetWindFunction.Invoke(null, new object[] { del });
                }
                else
                {
                    //Get FAR Atmosphere Methods 
                    MethodInfo SetWindFunction = FARAtm.GetMethod("SetWindFunction");
                    MethodInfo SetTemperatureFunction = FARAtm.GetMethod("SetTemperatureFunction");
                    MethodInfo SetPressureFunction = FARAtm.GetMethod("SetPressureFunction");
                    if (SetWindFunction == null && SetTemperatureFunction == null && SetPressureFunction == null)
                    {
                        Utils.LogWarning("Unable to register with FerramAerospaceResearch.");
                        return false;
                    }
                    if (SetWindFunction != null)
                    {
                        Utils.LogInfo("Registering Wind function with FerramAerospaceResearch");
                        SetWindFunction.Invoke(null, new object[] { (WindDelegate)GetTheWind });
                    }
                    if (SetTemperatureFunction != null)
                    {
                        Utils.LogInfo("Registering Temperature function with FerramAerospaceResearch");
                        SetTemperatureFunction.Invoke(null, new object[] { (PropertyDelegate)GetTheTemperature });
                    }
                    if (SetPressureFunction != null)
                    {
                        Utils.LogInfo("Registering Pressure function with FerramAerospaceResearch");
                        SetPressureFunction.Invoke(null, new object[] { (PropertyDelegate)GetThePressure });
                    }
                }
                Utils.LogInfo("Successfully registered with FerramAerospaceResearch.");
                return true;
            }
            catch (Exception e)
            {
                Utils.LogError("Exception thrown when registering with FerramAerospaceResearch: " + e.ToString());
            }
            return false;
        }
        #endregion
    }
}
