using System.Collections.Generic;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    [KSPAddon(KSPAddon.Startup.MainMenu,true)]
    internal class AtmoToolsStartup : MonoBehaviour
    {
        //remove any uninitialized modifiers as they could cause issues that I'd rather not deal with.
        void Awake()
        {
            Utils.LogInfo("Initializing AtmosphereModifiers.");
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                AtmosphereData data = body.GetComponent<AtmosphereData>();
                if (data != null)
                {
                    Utils.LogInfo("Cleaning up uninitialized AtmosphereModifiers for body " + body.name + ".");
                    data.CleanupModifiers();
                }
            }
            Utils.LogInfo("AtmosphereModifier Cleanup Complete.");
            FlightSceneHandler.VesselHandlerCache = new Dictionary<Vessel, VesselHandler>();
            Destroy(this);
        }
    }
}
