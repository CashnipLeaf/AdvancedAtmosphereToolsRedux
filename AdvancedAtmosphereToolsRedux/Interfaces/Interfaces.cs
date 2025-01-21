using System;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.Interfaces
{
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

    public interface IToxicAtmosphereIndicator
    {
        string ToxicAtmosphereMessage { get; set; }
        bool IsAtmosphereToxic(double longitude, double latitude, double altitude, double time, double trueAnomaly, double eccentricity);
    }
}
