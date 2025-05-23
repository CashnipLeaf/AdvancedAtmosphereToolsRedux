﻿using AdvancedAtmosphereToolsRedux.Interfaces;

namespace AdvancedAtmosphereToolsRedux.BaseModules.GenericUnsafeAtmosphere
{
    public class GenericUnsafeAtmosphere : IUnsafeAtmosphereIndicator
    {
        public GenericUnsafeAtmosphere() { }

        public bool IsAtmoUnsafe = false;

        public string UnsafeAtmosphereMessage
        {
            get => unsafeatmomessage;
            set => unsafeatmomessage = value;
        }
        private string unsafeatmomessage = string.Empty;

        public void Initialize() { }

        public void IsAtmosphereUnsafe(double lon, double lat, double alt, double time, double trueanomaly, double eccentricity, out bool unsafeToBreathe, out bool willDie) => unsafeToBreathe = willDie = IsAtmoUnsafe;
    }
}
