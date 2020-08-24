using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Erde
{
    public class AssetManager : IDisposable
    {
        static AssetManager         m_assetManager = null;

        Dictionary<string, ZipFile> m_packages;

        public static AssetManager Active
        {
            get
            {
                return m_assetManager;
            }
        }

        public string[] Packages
        {
            get
            {
                return m_packages.Keys.ToArray();
            }
        }

        public AssetManager ()
        {
            if (m_assetManager == null)
            {
                m_assetManager = this;
            }

            m_packages = new Dictionary<string, ZipFile>();
        }

        public void LoadPackage (string a_filePath, string a_password = null)
        {
            ZipFile zipFile = new ZipFile(a_filePath)
            {
                Password = a_password
            };

            m_packages.Add(a_filePath, zipFile);
        }
        public void UnloadPackage (string a_filePath)
        {
            m_packages.Remove(a_filePath);
        }

        public Stream GetAssetFile (string a_name)
        {
            foreach (ZipFile zip in m_packages.Values)
            {
                ZipEntry entry = zip.GetEntry(a_name);

                if (entry != null)
                {
                    return zip.GetInputStream(entry);
                }
            }

            return null;
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            if (m_assetManager == this)
            {
                m_assetManager = null;
            }
        }
        ~AssetManager ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
