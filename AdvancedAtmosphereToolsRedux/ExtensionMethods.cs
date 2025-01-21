using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    internal static class ExtensionMethods
    {
        //faster extension methods for Vector3
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
    }
}
