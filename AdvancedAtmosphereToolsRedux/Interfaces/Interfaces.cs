using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.Interfaces
{
    /* IMPORTANT NOTES:
     * 
     * Base Properties and Flat Modifiers must be in the respective units of their type, specified below. 
     * Fractional Modifiers are unitless.
     * 
     * Wind and Ocean Current Vectors must be in m/s, with x being the north/south component, y being the up/down component, z being the east/west component.
     * Pressure must be in kPa (kiloPascals).
     * Temperature must be in Kelvin (K).
     * Molar mass must be in kg/mol (*not* g/mol).
     * Adiabatic Index is unitless. Google it for details.
     * 
     * The Unsafe Atmosphere Indicator returns a pair of booleans indicating if the atmosphere is safe to breathe for kerbals. 
     * the first, "unsafeToBreathe", prevents the kerbal from removing their helmet, but the atmosphere wont kill them if they have already their helmet off. This is meant to provide a safety margin.
     * the second, "willDie", means the kerbal will die if they have their helmet off. setting this to "true" also prevents the kerbal from removing their helmet if they have it on.
     * This check takes priority over the "check for oxygen" check.
     * 
     * Intake Choke Factor is a unitless property that reduces the effectiveness of air intakes. 
     * A value of 0 means intakes will have maximum performance, a value of 1 means intakes will be completely choked (no air will be collected by the intake)
    */

    public interface IWindProvider : IBaseAtmosphereData
    {
        Vector3 GetWindVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IOceanCurrentProvider : IBaseAtmosphereData
    {
        Vector3 GetOceanCurrentVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IBasePressure : IBaseAtmosphereData
    {
        double GetBasePressure(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalPressureModifier : IBaseAtmosphereData
    {
        double GetFractionalPressureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatPressureModifier : IBaseAtmosphereData
    {
        double GetFlatPressureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }
    
    public interface IBaseTemperature : IBaseAtmosphereData
    {
        bool DisableLatitudeBias { get; }
        bool DisableLatitudeSunMult { get; }
        bool DisableAxialSunBias { get; }
        bool DisableEccentricityBias { get; }
        double GetBaseTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalTemperatureModifier : IBaseAtmosphereData
    {
        double GetFractionalTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatTemperatureModifier : IBaseAtmosphereData
    {
        double GetFlatTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalLatitudeBiasModifier : IBaseAtmosphereData
    {
        double GetFractionalLatitudeBiasModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalLatitudeSunMultModifier : IBaseAtmosphereData
    {
        double GetFractionalLatitudeSunMultModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalAxialSunBiasModifier : IBaseAtmosphereData
    {
        double GetFractionalAxialSunBiasModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalEccentricityBiasModifier : IBaseAtmosphereData
    {
        double GetFractionalEccentricityBiasModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IBaseMolarMass : IBaseAtmosphereData
    {
        double GetBaseMolarMass(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatMolarMassModifier : IBaseAtmosphereData
    {
        double GetFlatMolarMassModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IBaseAdiabaticIndex : IBaseAtmosphereData
    {
        double GetBaseAdiabaticIndex(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatAdiabaticIndexModifier : IBaseAtmosphereData
    {
        double GetFlatAdiabaticIndexModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IUnsafeAtmosphereIndicator : IBaseAtmosphereData
    {
        string UnsafeAtmosphereMessage { get; }
        void IsAtmosphereUnsafe(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity, out bool unsafeToBreathe, out bool willDie);
    }

    public interface IAirIntakeChokeFactor : IBaseAtmosphereData
    {
        double GetAirIntakeChokeFactor(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    //this is to ensure an Initialize() function is in every object
    public interface IBaseAtmosphereData
    {
        void Initialize();
    }
}
