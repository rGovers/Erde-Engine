using System.IO;

namespace Erde.IO
{
    public interface IFileSystem
    {
        bool Load (string a_path, out byte[] a_asset);
        bool Load (string a_path, out Stream a_stream);

        void Save (string a_path, byte[] a_asset);
        void Save (string a_path, Stream a_stream);

        bool Exists (string a_path);
    }
}
