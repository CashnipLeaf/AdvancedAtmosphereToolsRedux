using HarmonyLib;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.HarmonyPatches
{
    //Add an offset to the velocity vector used for wing lift calculations to account for wind.
    [HarmonyPatch(typeof(ModuleLiftingSurface), nameof(ModuleLiftingSurface.SetupCoefficients))]
    public static class WingHijacker
    {
        static void Prefix(ref Vector3 pointVelocity, ModuleLiftingSurface __instance)
        {
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(__instance.part.vessel);
            if (!pointVelocity.IsFinite() || VH == null || Settings.FAR_Exists)
            {
                return;
            }
            Vector3 windvec = VH.InternalAppliedWind;
            double submerged = __instance.part.submergedPortion;
            windvec.LerpWith(Vector3.zero, (float)(submerged * submerged));

            if (windvec.IsFinite() && !Mathf.Approximately(windvec.magnitude, 0.0f))
            {
                pointVelocity -= windvec;
            }
        }
    }
}
