using System;
using System.IO;

namespace Erde.IO
{
    public class DriveFileSystem : IFileSystem
    {
        string m_path;

        public DriveFileSystem (string a_path = "./")
        {
            m_path = a_path;
        }

        public bool Exists (string a_path)
        {
            return File.Exists(m_path + a_path);
        }

        public bool Load (string a_path, out byte[] a_asset)
        {
            a_asset = null;

            string fullPath = m_path + a_path;

            if (!File.Exists(fullPath))
            {
                return false;
            }

            a_asset = File.ReadAllBytes(fullPath);

            if (a_asset != null)
            {
                return true;
            }

            return false;
        }

        public bool Load (string a_path, out Stream a_stream)
        {
            a_stream = null;

            string fullPath = m_path + a_path;

            if (!File.Exists(fullPath))
            {
                return false;
            }

            a_stream = File.OpenRead(fullPath);

            if (a_stream != null)
            {
                return true;
            }

            return false;
        }

        void CreateDirectories (string a_path)
        {
            string[] parts = (m_path + a_path).Split(new[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);

            string dir = string.Empty;

            for (int i = 0; i < parts.Length; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    dir += parts[j] + "/";
                }
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public void Save (string a_path, byte[] a_asset)
        {
            string file = m_path + a_path;

            CreateDirectories(a_path);

            if (!File.Exists(file))
            {
                FileStream stream = File.Create(file);

                stream.Close();
            }

            File.WriteAllBytes(file, a_asset);
        }
        public void Save (string a_path, Stream a_stream)
        {
            CreateDirectories(a_path);

            // Lazy mode engage
            a_stream.CopyTo(File.OpenWrite(m_path + a_path));
        }
    }
}
