using UnityEngine;
using System.Reflection;
using HarmonyLib;

namespace AdvancedAtmosphereToolsRedux.HarmonyPatches
{
    //patch the PartBuoyancy script to include ocean currents
    /* I ultimately decided that the juice is not worth the squeeze. If you believe otherwise, please send me a PR with your implementation
    [HarmonyPatch]
    public static class WaterPhysics
    {
        public static MethodBase TargetInfo()
        {
            return AccessTools.Method(typeof(PartBuoyancy), "FixedUpdate");
        }

        public static void Postfix(PartBuoyancy __instance)
        {
            
        }
    }
    */
}
