using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace Erde.IO
{
    public class PackageFileSystem : IFileSystem
    {
        class PackageDataSource : IStaticDataSource
        {
            MemoryStream m_stream;

            public PackageDataSource (byte[] a_data)
            {
                m_stream = new MemoryStream(a_data);
            }

            public Stream GetSource ()
            {
                return m_stream;
            }
        }
        class PackageStreamSource : IStaticDataSource
        {
            Stream m_stream;

            public PackageStreamSource (Stream a_stream)
            {
                m_stream = a_stream;
            }

            public Stream GetSource ()
            {
                return m_stream;
            }
        }

        ZipFile m_package;

        public PackageFileSystem (string a_package, string a_password = null)
        {
            m_package = new ZipFile(a_package)
            {
                Password = a_password
            };
        }

        public bool Exists (string a_path)
        {
            return m_package.GetEntry(a_path) != null;
        }

        public bool Load (string a_path, out byte[] a_asset)
        {
            a_asset = null;

            Stream stream;

            if (Load(a_path, out stream))
            {
                MemoryStream memoryStream = new MemoryStream();

                stream.CopyTo(memoryStream);

                a_asset = memoryStream.ToArray();

                return true;
            }

            return false;
        }
        public bool Load (string a_path, out Stream a_stream)
        {
            a_stream = null;

            ZipEntry entry = m_package.GetEntry(a_path);

            if (entry != null)
            {
                a_stream = m_package.GetInputStream(entry);

                return true;
            }

            return false;
        }

        public void Save (string a_path, byte[] a_asset)
        {
            m_package.BeginUpdate();

            PackageDataSource packageDataSource = new PackageDataSource(a_asset);

            m_package.Add(packageDataSource, a_path);

            m_package.CommitUpdate();
        }
        public void Save (string a_path, Stream a_stream)
        {
            m_package.BeginUpdate();

            PackageStreamSource packageStreamSource = new PackageStreamSource(a_stream);

            m_package.Add(packageStreamSource, a_path);

            m_package.CommitUpdate();
        }
    }
}
