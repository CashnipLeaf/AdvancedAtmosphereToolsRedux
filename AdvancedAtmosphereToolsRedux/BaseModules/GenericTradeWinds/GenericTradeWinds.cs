using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GenericTradeWinds
{
    public class GenericTradeWinds : IWindProvider
    {
        public GenericTradeWinds() { }

        public Vector3 GetWindVector(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity)
        {
            return Vector3.zero;
        }
    }
}
