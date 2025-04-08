using System.Reflection;
using HarmonyLib;
using KSP.Localization;

namespace AdvancedAtmosphereToolsRedux.HarmonyPatches
{    
    //Splices in an additional check for kerbal breathing to see if it is safe for them to breathe the atmosphere
    [HarmonyPatch]
    public static class UnsafeAtmoHijacker
    {
        public static MethodBase TargetMethod() => AccessTools.Method(typeof(KerbalEVA), "CheckHelmetOffSafe");

        public static bool Prefix(ref bool __result, KerbalEVA __instance, ref string ___helmetUnsafeReason, ref bool includeSafetyMargins)
        {
            Vessel vessel = __instance.vessel;
            CelestialBody body = vessel.mainBody;
            if (body == null || vessel == null)
            {
                return true;
            }
            AtmoToolsRedux_Data atmodata = AtmoToolsRedux_Data.GetAtmosphereData(body);
            if (body.atmosphere && vessel.altitude < body.atmosphereDepth && atmodata != null)
            {
                double time = Planetarium.GetUniversalTime();
                AtmoToolsReduxUtils.GetTrueAnomalyEccentricity(body, out double trueAnomaly, out double eccentricity);
                atmodata.CheckAtmosphereUnsafe(vessel.longitude, vessel.latitude, vessel.altitude, time, trueAnomaly, eccentricity, out bool unsafeToBreathe, out bool willDie, out string unsafeAtmoMessage);

                __result = includeSafetyMargins ? !(unsafeToBreathe || willDie) : !willDie;

                if (!__result)
                {
                    ___helmetUnsafeReason = !string.IsNullOrEmpty(unsafeAtmoMessage) ? Localizer.Format(unsafeAtmoMessage) : Localizer.Format("#autoLOC_8003204"); //"No breathable atmosphere"
                }
                return false;
            }
            return true;
        }
    }
}
