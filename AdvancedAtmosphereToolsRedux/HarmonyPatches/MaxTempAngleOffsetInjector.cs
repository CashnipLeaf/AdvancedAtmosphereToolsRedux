using HarmonyLib;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.HarmonyPatches
{
    //replicates the functionality of Sigma Heat Shifter's maxTempAngleOffset
    [HarmonyPatch(typeof(CelestialBody), nameof(CelestialBody.GetAtmoThermalStats))]
    public static class MaxTempAngleOffsetInjector
    {
        public static void Prefix(CelestialBody __instance, ref CelestialBody sunBody, ref Vector3d upAxis)
        {
            if (sunBody != __instance)
            {
                Vector3 up = __instance.bodyTransform.up;
                double angleoffset = __instance.MaxTempAngleOffset();
                //rotate the vessel's upaxis to counteract the rotation applied by the game.
                //default rotation is 45 degrees, so the default behavior is no rotation applied.
                upAxis = Quaternion.AngleAxis((-45f + (float)angleoffset) * Mathf.Sign((float)__instance.rotationPeriod), up) * upAxis;
            }
        }
    }
}
