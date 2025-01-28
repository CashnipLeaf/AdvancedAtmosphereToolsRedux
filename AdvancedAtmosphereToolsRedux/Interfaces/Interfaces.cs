using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.Interfaces
{
    /* IMPORTANT NOTES:
     * 
     * Base and Flat Modifiers must be in the respective units of their type, specified below. 
     * Fractional Modifiers are unitless.
     * 
     * Wind and Ocean Current Vectors must be in m/s, with x being the north/south component, y being the up/down component, z being the east/west component.
     * Pressure must be in kPa (kiloPascals).
     * Temperature must be in Kelvin (K).
     * Molar mass must be in kg/mol (*not* g/mol).
     * Adiabatic Index is unitless. Google it for details.
     * 
     * The Unsafe Atmosphere Indicator simply returns a boolean about whether the atmosphere at the given location (or possibly the entire atmosphere) is unsafe for kerbals to breathe. 
     * This check takes priority over the "check for oxygen" check.
    */

    public interface IWindProvider
    {
        Vector3 GetWindVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IOceanCurrentProvider
    {
        Vector3 GetOceanCurrentVector(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IBasePressure
    {
        double GetBasePressure(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalPressureModifier
    {
        double GetFractionalPressureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatPressureModifier
    {
        double GetFlatPressureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }
    
    public interface IBaseTemperature
    {
        bool DisableLatitudeBias { get; set; }
        bool DisableLatitudeSunMult { get; set; }
        bool DisableAxialSunBias { get; set; }
        bool DisableEccentricityBias { get; set; }
        double GetBaseTemperature(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalTemperatureModifier
    {
        double GetFractionalTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatTemperatureModifier
    {
        double GetFlatTemperatureModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalLatitudeBiasModifier
    {
        double GetFractionalLatitudeBiasModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalLatitudeSunMultModifier 
    {
        double GetFractionalLatitudeSunMultModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalAxialSunBiasModifier
    {
        double GetFractionalAxialSunBiasModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFractionalEccentricityBiasModifier
    {
        double GetFractionalEccentricityBiasModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IBaseMolarMass
    {
        double GetBaseMolarMass(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatMolarMassModifier
    {
        double GetFlatMolarMassModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IBaseAdiabaticIndex
    {
        double GetBaseAdiabaticIndex(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IFlatAdiabaticIndexModifier
    {
        double GetFlatAdiabaticIndexModifier(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    public interface IUnsafeAtmosphereIndicator
    {
        string UnsafeAtmosphereMessage { get; set; }
        bool IsAtmosphereUnsafe(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }

    //used for setting up parameters that require all bodies to be fully loaded before computing.
    public interface IRequiresFinalSetup
    {
        void FinalSetup();
    }
}
