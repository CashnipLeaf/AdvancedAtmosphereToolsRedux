using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAtmosphereToolsRedux
{
    // runs the UI and does some caching
    internal sealed class FlightSceneHandler //: MonoBehavior
    {
        internal static FlightSceneHandler Instance { get; private set; }
    }
}
