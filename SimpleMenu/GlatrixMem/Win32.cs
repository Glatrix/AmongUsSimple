using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlatrixMemory
{
    public class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId
            );

        [DllImport("Kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int size,
            IntPtr lpNumberOfBytesWritten
            );

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(
            Keys vKeys
            );

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            int bInheritHandle,
            int dwProcessId
            );




        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(
            IntPtr hObject
            );

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddres,
            int dwSize,
            uint flAllocationType,
            uint flProtect
            );

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            int dwFreeType
            );
    }

}
