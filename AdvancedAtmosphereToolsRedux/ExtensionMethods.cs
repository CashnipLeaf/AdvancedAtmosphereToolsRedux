using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    internal static class ExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Add(ref this Vector3 v, Vector3 other)
        {
            v.x += other.x;
            v.y += other.y;
            v.z += other.z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Subtract(ref this Vector3 v, Vector3 other)
        {
            v.x -= other.x;
            v.y -= other.y;
            v.z -= other.z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Set(ref this Vector3 v, Vector3 other)
        {
            v.x = other.x;
            v.y = other.y;
            v.z = other.z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void MultiplyByConstant(ref this Vector3 v, float other)
        {
            v.x *= other;
            v.y *= other;
            v.z *= other;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LerpWith(ref this Vector3 v, Vector3 other, float by)
        {
            by = Mathf.Clamp01(by);
            v.x = (v.x * (1.0f - by)) + (other.x * by);
            v.y = (v.y * (1.0f - by)) + (other.y * by);
            v.z = (v.z * (1.0f - by)) + (other.z * by);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Multiply(ref this Vector3 v, Vector3 other)
        {
            v.x *= other.x;
            v.y *= other.y;
            v.z *= other.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Zero(ref this Vector3 v) => v.x = v.y = v.z = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsFinite(ref this Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsZero(ref this Vector3 v) => v.x == 0.0f && v.y == 0.0f && v.z == 0.0f;

        //Celestial Body Extension Methods
        internal static double OceanBulkModulus(this CelestialBody body)
        {
            AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
            return data != null ? data.OceanBulkModulus : AtmoToolsReduxUtils.WaterBulkModulus;
        }

        internal static double OceanSpeedOfSound(this CelestialBody body)
        {
            return Math.Sqrt(body.OceanBulkModulus() / body.oceanDensity);
        }

        internal static float MaxTempAngleOffset(this CelestialBody body)
        {
            AtmoToolsRedux_Data data = AtmoToolsRedux_Data.GetAtmosphereData(body);
            return (float)(data != null ? data.MaxTempAngleOffset : AtmoToolsReduxUtils.DefaultMaxTempAngleOffset);
        }
    }
}