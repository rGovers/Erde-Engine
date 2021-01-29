using Erde;
using Erde.IO;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Erde.Graphics
{
    public static class FontCollection
    {
        static PrivateFontCollection Fonts;

        public static void AddFont(string a_file, IFileSystem a_fileSystem)
        {
            if (Fonts == null)
            {
                Fonts = new PrivateFontCollection();
            }

            byte[] bytes; 
            if (a_fileSystem.Load(a_file, out bytes))
            {
                int len = bytes.Length;

                IntPtr ptr = Marshal.AllocHGlobal(len);
                Marshal.Copy(bytes, 0, ptr, len);

                Fonts.AddMemoryFont(ptr, len);

                Marshal.FreeHGlobal(ptr);
            }
        }

        public static FontFamily GetFontFamily(string a_familyName)
        {
            if (Fonts == null)
            {
                return null;
            }

            FontFamily[] fontFamilies = Fonts.Families;
            foreach (FontFamily font in fontFamilies)
            {
                if (font.Name == a_familyName)
                {
                    return font;
                }
            }

            return null;
        }
    }
}