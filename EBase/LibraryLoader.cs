using System;
using System.Runtime.InteropServices;

namespace Erde
{
    public static class LibraryLoader
    {
        [DllImport("libdl.so")]
        static extern IntPtr dlopen(string a_filename, int a_flags);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string a_filename);

        public static IntPtr LoadAssembly(string a_filename)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                {
                    return LoadLibrary(a_filename);
                }
                case PlatformID.Unix:
                {
                    return dlopen(a_filename, 2 | 8);
                }
            }

            return IntPtr.Zero;
        }
    }
}
