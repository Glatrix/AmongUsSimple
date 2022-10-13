using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlatrixMemory
{
    public class Il2cppExtern
    {
        private Memory mem;
        private IntPtr GameAssembly;

        public Il2cppExtern(Memory mem)
        {
            this.mem = mem;
            GameAssembly = mem.GetModuleBaseAddress("GameAssembly.dll");
        }


        //Domain
        public const int domain = 0x2096AF8;

        //Assemblies Start Address
        public const int assemblies_start = 0x2096720;

        //Assembly.Image Offset
        public const int assembly_image = 0x00;

        //Assembly.Image.ClassCount
        public const int image_classCount = 0x0C;

        //Assembly.Info.? 
        public const int assembly_info = 0x10;
        public const int asmInfo_name = 0x0;

        //Assembly.Image.Info.?
        public const int image_info = 0x20;
        public const int imgInfo_filename = 0x48;



        /// <summary>[DEPRICATED]</summary>
        public IntPtr il2cpp_domain_get()
        {
            return GameAssembly + domain;
        }

        public IntPtr[] il2cpp_domain_get_assemblies()
        {
            List<IntPtr> assembly_cache = new List<IntPtr>();

            for(int i = 0; ; i++)
            {
                IntPtr assembliesArray = GameAssembly + assemblies_start;
                IntPtr assemblyPtr = mem.Pointer(assembliesArray, new int[] { (i * 0x4), 0 });

                if (assemblyPtr == IntPtr.Zero)
                    break;

                assembly_cache.Add(assemblyPtr);
            }

            return assembly_cache.ToArray();
        }


        public IntPtr il2cpp_assembly_get_image(IntPtr assemblyPtr)
        {
            return mem.Read<IntPtr>(assemblyPtr);
        }

        public int il2cpp_image_get_class_count(IntPtr image)
        {
            return mem.Read<int>(image + image_classCount);
        }


        public string il2cpp_assembly_get_name(IntPtr assemblyPtr)
        {
            return Helpers.CutString(mem.ReadString(assemblyPtr + assembly_info, new int[] { asmInfo_name }, 90, Encoding.ASCII));
        }

        public string il2cpp_image_get_name(IntPtr image)
        {
            return Helpers.CutString(mem.ReadString(image + image_info, new int[] {imgInfo_filename},90,Encoding.ASCII));
        }


    }
}
