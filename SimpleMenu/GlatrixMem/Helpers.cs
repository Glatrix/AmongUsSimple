using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GlatrixMemory
{
    public class Helpers
    {
        public static byte[] SignatureToPattern(string sig)
        {
            string[] parts = sig.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] patternArray = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "?")
                {
                    patternArray[i] = 0;
                    continue;
                }

                if (!byte.TryParse(parts[i], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.DefaultThreadCurrentCulture, out patternArray[i]))
                {
                    throw new Exception();
                }
            }

            return patternArray;
        }

        public static string GetSignatureMask(string sig)
        {
            string[] parts = sig.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string mask = "";

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "?")
                {
                    mask += "?";
                }
                else
                {
                    mask += "x";
                }
            }

            return mask;
        }

        public static string CutString(string mystring)
        {
            char[] chArray = mystring.ToCharArray();
            string str = "";
            for (int i = 0; i < mystring.Length; i++)
            {
                if ((chArray[i] == ' ') && (chArray[i + 1] == ' '))
                {
                    return str;
                }
                if (chArray[i] == '\0')
                {
                    return str;
                }
                str = str + chArray[i].ToString();
            }
            return mystring.TrimEnd(new char[] { '0' });
        }

        public static float[] ConvertToFloatArray(byte[] bytes)
        {
            if (bytes.Length % 4 != 0) throw new ArgumentException();

            float[] floats = new float[bytes.Length / 4];

            for (int i = 0; i < floats.Length; i++) floats[i] = BitConverter.ToSingle(bytes, i * 4);

            return floats;
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] StructureToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);

            byte[] array = new byte[length];

            IntPtr pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }

    }
}
