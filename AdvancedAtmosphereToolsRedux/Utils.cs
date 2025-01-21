using System;
using System.IO;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    internal static class Utils
    {
        internal static string GameDataPath => "finish me";
        internal static void LogInfo(string message) => Debug.Log("[AdvAtmoToolsRedux] " + message);
        internal static void LogWarning(string message) => Debug.Log("[AdvAtmoToolsRedux][WARNING] " + message);
        internal static void LogError(string message) => Debug.Log("[AdvAtmoToolsRedux][ERROR] " + message);

        internal static float[][,,] ReadBinaryFile(string path, int lon, int lat, int alt, int steps, int offset, bool invertalt)
        {
            int Blocksize = lon * lat * sizeof(float);
            float[][,,] newarray = new float[steps][,,];
            using (BinaryReader reader = new BinaryReader(File.OpenRead(GameDataPath + path)))
            {
                if (offset > 0) //eat the initial offset
                {
                    reader.ReadBytes(offset);
                }
                for (int i = 0; i < steps; i++)
                {
                    if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    {
                        throw new EndOfStreamException("Attempted to read beyond the end of the file. Verify that your size and timestep parameters match that of the actual file.");
                    }
                    float[,,] floatbuffer = new float[alt, lat, lon];
                    for (int j = 0; j < alt; j++)
                    {
                        byte[] bufferarray = reader.ReadBytes(Blocksize);
                        Buffer.BlockCopy(bufferarray, 0, floatbuffer, Blocksize * (invertalt ? (alt - 1 - j) : j), Buffer.ByteLength(bufferarray));
                    }
                    newarray[i] = floatbuffer;
                }
                reader.Close();
            }
            return newarray;
        }

        public static float BiLerp(float first1, float second1, float first2, float second2, float by1, float by2)
        {
            return Mathf.Lerp(Mathf.Lerp(first1, second1, by1), Mathf.Lerp(first2, second2, by1), by2);
        }

        //allow for altitude spacing based on some factor
        public static double ScaleAltitude(double nX, double xBase, int upperbound, out int int1, out int int2)
        {
            nX = UtilMath.Clamp01(nX);
            double z = (xBase <= 1.0 ? nX : ((Math.Pow(xBase, -nX * upperbound) - 1) / (Math.Pow(xBase, -1 * upperbound) - 1))) * upperbound;
            int1 = Clamp((int)Math.Floor(z), 0, upperbound); //layer 1
            int2 = Clamp(int1 + 1, 0, upperbound); //layer 2
            return nX >= 1d ? 1d : UtilMath.Clamp01(z - Math.Truncate(z));
        }

        public static double InterpolatePressure(double first, double second, double by)
        {
            if (first < 0.0 || second < 0.0) //negative values will break the logarithm, so they are not allowed.
            {
                throw new ArgumentOutOfRangeException();
            }
            if (first <= float.Epsilon || second <= float.Epsilon)
            {
                return UtilMath.Lerp(first, second, by);
            }
            double scalefactor = Math.Log(first / second);
            if (double.IsNaN(scalefactor))
            {
                throw new NotFiniteNumberException();
            }
            return first * Math.Pow(Math.E, -1 * UtilMath.Lerp(0.0, UtilMath.Clamp(scalefactor, float.MinValue, float.MaxValue), by));
        }

        //Apparently no such function exists for integers in either UtilMath or Mathf. Why?
        public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
    }
}
