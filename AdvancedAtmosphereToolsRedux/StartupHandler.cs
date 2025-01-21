using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class StartupHandler : MonoBehaviour
    {
        void Awake()
        {
            Utils.LogInfo("Initializing AtmosphereModifiers.");
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                AtmosphereData data = body.GetComponent<AtmosphereData>();
                if (data != null)
                {
                    Utils.LogInfo("Initializing AtmosphereModifiers for body " + body.name + ".");
                    data.InitializeModifiers();
                }
            }
            Utils.LogInfo("AtmosphereModifier Initialization Complete.");
            Destroy(this);
        }
    }
}
