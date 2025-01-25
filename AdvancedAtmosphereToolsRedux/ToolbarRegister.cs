using UnityEngine;
using ToolbarControl_NS;

namespace AdvancedAtmosphereToolsRedux
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class ToolbarRegister : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(FlightSceneHandler.modID, FlightSceneHandler.modNAME);
        }
    }
}
