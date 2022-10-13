using GlatrixMemory;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;


namespace GlatrixMemory
{
    public class Memory
    {

        public bool FindBytes(IntPtr Address, ref byte[] Buffer)
        {
            return Win32.ReadProcessMemory(Handle, Address, Buffer, Buffer.Length, IntPtr.Zero) ? true : false;
        }

        public int PatternScan(string module_name, string signature)
        {
            byte[] ModuleBuffer = null;
            IntPtr ModuleAddress = IntPtr.Zero;

            foreach (ProcessModule process_module in TargetProcess.Modules)
            {
                if (process_module.ModuleName == module_name)
                {
                    ModuleBuffer = new byte[process_module.ModuleMemorySize];
                    ModuleAddress = process_module.BaseAddress;
                }
            }

            if (ModuleAddress == IntPtr.Zero || ModuleBuffer == null)
                return -1;

            byte[] pattern = Helpers.SignatureToPattern(signature);
            string mask = Helpers.GetSignatureMask(signature);

            if (FindBytes(ModuleAddress, ref ModuleBuffer))
            {
                for (int i = 0; i < ModuleBuffer.Length; i++)
                {
                    bool IsFounded = false;

                    for (int a = 0; a < pattern.Length; a++)
                    {
                        if (mask[a] == '?')
                            continue;

                        if (pattern[a] == ModuleBuffer[i + a])
                            IsFounded = true;
                    }

                    if (!IsFounded) continue;
                    return i;
                }
            }

            return -1;
        }

        public IntPtr Pointer(IntPtr ptr, int[] offsets)
        {
            var buffer = new byte[IntPtr.Size];

            foreach (int i in offsets)
            {
                Win32.ReadProcessMemory(Handle, ptr, buffer, buffer.Length, IntPtr.Zero);

                ptr = (IntPtr.Size == 4)
                ? IntPtr.Add(new IntPtr(BitConverter.ToInt32(buffer, 0)), i)
                : ptr = IntPtr.Add(new IntPtr(BitConverter.ToInt64(buffer, 0)), i);
            }
            return ptr;
        }

        public IntPtr CreateCodeCave(int Size)
        {
            IntPtr buffer = Win32.VirtualAllocEx(Handle, IntPtr.Zero, Size, 0x1000 | 0x2000, 0x40);
            return buffer;
        }

        public IntPtr CreateCodeCave(IntPtr Address, int Size)
        {
            IntPtr buffer = Win32.VirtualAllocEx(Handle, Address, Size, 0x1000 | 0x2000, 0x40);
            return buffer;
        }

        public void WriteToCodeCave(IntPtr Address, byte[] Codes)
        {
            WriteMemory(Address, Codes);
        }

        public void WriteToCodeCave(IntPtr Address, int[] Offsets, byte[] Codes)
        {
            WriteMemory(Address, Offsets, Codes);
        }

        public bool DestroyCodeCave(IntPtr Address)
        {
            return Win32.VirtualFreeEx(Handle, Address, 0, 0x00008000);
        }


        public bool IsKeyDown(Keys Key)
        {
            return (Win32.GetAsyncKeyState(Key) < 0);
        }




        public Process TargetProcess;

        public IntPtr BaseAddress;
        public IntPtr Handle;


        public bool Attatch(string ProcessName)
        {
            Process[] processByName = Process.GetProcessesByName(ProcessName);

            if (processByName.Length == 0)
            {
                return false;
            }
            else
            {
                TargetProcess = processByName[0];
                Handle = TargetProcess.Handle;
                //Handle = Win32.OpenProcess((uint)MemoryProtection.ExecuteReadWrite, 0, TargetProcess.Id);
                BaseAddress = TargetProcess.MainModule.BaseAddress;

                if (Handle.ToInt64() == 0 || BaseAddress.ToInt64() == 0)
                {
                    return false;
                }
                return true;
            }
        }


        public void Detatch()
        {
            if (Handle != null && Handle != IntPtr.Zero)
            {
                Win32.CloseHandle(Handle);
                TargetProcess = null;
                Handle = IntPtr.Zero;
            }
        }

        public IntPtr GetModuleBaseAddress(string ModuleName)
        {
            foreach (ProcessModule processModule in TargetProcess.Modules)
            {
                if (processModule.ModuleName == ModuleName)
                {
                    return processModule.BaseAddress;
                }
            }
            return IntPtr.Zero;
        }



        //Bytes to/from structs
        public static byte[] Serialize<T>(T s) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(s, ptr, true);
            Marshal.Copy(ptr, array, 0, size);
            Marshal.FreeHGlobal(ptr);
            return array;
        }

        public static T Deserialize<T>(byte[] array) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);
            var s = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return s;
        }


        public string ReadString(IntPtr Address, int length, Encoding encoding)
        {
            byte[] buffer = ReadMemory(Address, length);
            string conv = encoding.GetString(buffer);
            return conv;
        }

        public string ReadString(IntPtr Address, int[] offsets, int length, Encoding encoding)
        {
            byte[] buffer = ReadMemory(Address, offsets, length);
            string conv = encoding.GetString(buffer);
            return conv;
        }



        // Read Memory Functions
        public byte[] ReadMemory(IntPtr Address, int Size)
        {
            byte[] buffer = new byte[Size];
            Win32.ReadProcessMemory(Handle, Address, buffer, buffer.Length, IntPtr.Zero);
            return buffer;
        }

        public byte[] ReadMemory(IntPtr Address, int[] Offsets, int Size)
        {
            byte[] buffer = new byte[Size];
            Win32.ReadProcessMemory(Handle, Pointer(Address, Offsets), buffer, buffer.Length, IntPtr.Zero);
            return buffer;
        }

        public T Read<T>(IntPtr Address, params int[] Offsets) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = ReadMemory(Pointer(Address, Offsets), size);
            return Deserialize<T>(buffer);
        }

        public T Read<T>(IntPtr Address) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = ReadMemory(Address, size);
            return Deserialize<T>(buffer);
        }

        // Write Memory Functions

        public void WriteMemory(IntPtr Address, byte[] NewBytes)
        {
            Win32.WriteProcessMemory(Handle, Address, NewBytes, NewBytes.Length, IntPtr.Zero);
        }

        public void WriteMemory(IntPtr Address, int[] Offsets, byte[] NewBytes)
        {
            Win32.WriteProcessMemory(Handle, Pointer(Address, Offsets), NewBytes, NewBytes.Length, IntPtr.Zero);
        }

        public void Write<T>(T Value, IntPtr Address, params int[] Offsets) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            buffer = Serialize(Value);
            IntPtr newAddress = Pointer(Address, Offsets);
            WriteMemory(newAddress, buffer);
        }

        public void Write<T>(T Value, IntPtr Address) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            buffer = Serialize(Value);
            WriteMemory(Address, buffer);
        }
    }
}
