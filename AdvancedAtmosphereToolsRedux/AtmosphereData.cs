using System;
using System.Collections.Generic;
using AdvancedAtmosphereToolsRedux.Interfaces;

namespace AdvancedAtmosphereToolsRedux
{
    // TODO: inherit from MonoBehavior
    public sealed class AtmosphereData //: MonoBehavior
    {
        public AtmosphereData() 
        {
            windProviders = new List<IWindProvider>();
            oceanCurrentProviders = new List<IOceanCurrentProvider>();
        }

        public void InitializeModifiers()
        {
            foreach (IWindProvider provider in windProviders)
            {
                try
                {
                    ((AtmosphereModifier)provider).Initialize();
                }
                catch(Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
            foreach (IOceanCurrentProvider provider in oceanCurrentProviders)
            {
                try
                {
                    ((AtmosphereModifier)provider).Initialize();
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.ToString());
                }
            }
        }

        private List<IWindProvider> windProviders;
        public void AddWindProvider(AtmosphereModifier modifier)
        {
            if (modifier is IWindProvider provider)
            {
                windProviders.Add(provider);
            }
            else
            {
                string type = (modifier.GetType()).ToString();
                Utils.LogWarning("Unable to add " + type + " as a Wind Provider. " + type + " does not implement the interface IWindProvider.");
            }
        }

        private List<IOceanCurrentProvider> oceanCurrentProviders;
        public void AddOceanCurrentProvider(AtmosphereModifier modifier)
        {
            if (modifier is IOceanCurrentProvider provider)
            {
                oceanCurrentProviders.Add(provider);
            }
            else
            {

            }
        }
    }
}
