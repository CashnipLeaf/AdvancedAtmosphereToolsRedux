using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
