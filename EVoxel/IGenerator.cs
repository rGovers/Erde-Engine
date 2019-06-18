namespace Erde.Voxel
{
    public interface IGenerator
    {
        void Generate (Chunk a_voxelObject);
        void PostCreate (Chunk a_chunk);

        void LoadGenerator (byte[] a_bytes);

        byte[] SaveGenerator ();

        void SetVoxelManager (VoxelManager a_voxelManager);
    }
}