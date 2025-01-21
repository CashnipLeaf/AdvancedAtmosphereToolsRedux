using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAtmosphereToolsRedux.Interfaces
{
    public abstract class AtmosphereModifier
    {
        public CelestialBody Body;
        public bool Initialized = false;
        public virtual void Initialize() { }
    }
}
