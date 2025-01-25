using System.Reflection;
using HarmonyLib;
using KSP.Localization;

namespace AdvancedAtmosphereToolsRedux.HarmonyPatches
{    
    //Splices in an additional check for kerbal breathing to see if it is safe for them to breathe the atmosphere
    [HarmonyPatch]
    public static class UnsafeAtmoHijacker
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(KerbalEVA), "CheckHelmetOffSafe");
        }

        public static bool Prefix(ref bool __result, KerbalEVA __instance, ref string ___helmetUnsafeReason)
        {
            Vessel vessel = __instance.vessel;
            CelestialBody body = vessel.mainBody;
            if (body == null || vessel == null)
            {
                return true;
            }
            AtmosphereData atmodata = AtmosphereData.GetAtmosphereData(body);
            if (body.atmosphere && vessel.altitude < body.atmosphereDepth && atmodata != null)
            {
                double time = Planetarium.GetUniversalTime();
                AtmoToolsReduxUtils.GetTrueAnomalyEccentricity(body, out double trueAnomaly, out double eccentricity);
                bool atmoUnsafe = atmodata.CheckAtmosphereUnsafe(vessel.longitude, vessel.latitude, vessel.altitude, time, trueAnomaly, eccentricity, out string unsafeAtmoMessage);
                if (atmoUnsafe)
                {
                    ___helmetUnsafeReason = !string.IsNullOrEmpty(unsafeAtmoMessage) ? Localizer.Format(unsafeAtmoMessage) : Localizer.Format("#autoLOC_8003204"); //"No breathable atmosphere"
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    /*
    [HarmonyPatch(typeof(KerbalEVA), nameof(KerbalEVA.CanEVAWithoutHelmet))]
    public static class KerbalBreathHijacker1
    {
        public static bool Prefix(ref bool __result, KerbalEVA __instance, ref string ___helmetUnsafeReason)
        {
            if (UnsafeAtmoHijacker.IsAtmoUnsafe(__instance.vessel, ref ___helmetUnsafeReason))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KerbalEVA), nameof(KerbalEVA.CanSafelyRemoveHelmet))]
    public static class KerbalBreathHijacker2
    {
        public static bool Prefix(ref bool __result, KerbalEVA __instance, ref string ___helmetUnsafeReason)
        {
            if (UnsafeAtmoHijacker.IsAtmoUnsafe(__instance.vessel, ref ___helmetUnsafeReason))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KerbalEVA), nameof(KerbalEVA.WillDieWithoutHelmet))]
    public static class KerbalBreathHijacker3
    {
        public static bool Prefix(ref bool __result, KerbalEVA __instance, ref string ___helmetUnsafeReason)
        {
            if (UnsafeAtmoHijacker.IsAtmoUnsafe(__instance.vessel, ref ___helmetUnsafeReason))
            {
                __result = true; //this differs from the other two because this should return "true" if kerbal will die, while the other two return "true" if the kerbal will live
                return false;
            }
            return true;
        }
    }
    */
}
