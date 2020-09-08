using Erde.Graphics;
using Erde.Graphics.Rendering;
using Erde.IO;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Erde.Voxel
{
    public class VoxelManager : IDisposable
    {
        // Stores the chunks that are being used by the program
        // The key is the position of the chunk on the grid
        ConcurrentDictionary<Vector3, Chunk> m_chunks;

        // The thread that the [this] is running on
        Thread                               m_voxelThread;

        // The number of voxels in a chunk cbrt
        int                                  m_voxelWidth;

        int                                  m_stackSize;

        // The spacing between each voxel
        float                                m_voxelSize;

        // How many chunk itterations from the loading position should be loaded
        int                                  m_scanDistance;

        // How far away from the loading position a chunk can get before it is destroyed
        float                                m_destroyDistance;

        // Threading shutdown variables
        bool                                 m_shutDown;

        bool                                 m_join;

        // The material the chunks shold use to draw with
        Material                             m_material;

        // The GPU pipeline the chunks should use
        Pipeline                             m_pipeline;

        // The drawing object the chunks should use
        Graphics.Graphics                    m_graphics;

        // Where loading should be centred around
        Vector2                              m_loadPos;

        // The object that is in charge of generating the world
        IGenerator                           m_worldGenerator;

        IFileSystem                          m_fileSystem;

        // Should the chunk thread save
        bool                                 m_save;

        bool                                 m_threaded;

        uint                                 m_maxThreads;
        List<Thread>                         m_updateThreads;
        ConcurrentQueue<Chunk>               m_updateChunk;

        public ConcurrentDictionary<Vector3, Chunk> Chunks
        {
            get
            {
                return m_chunks;
            }
        }

        public int ChunkThreads
        {
            get
            {
                return m_updateThreads.Count;
            }
        }

        public const string VoxelManagerFileName = "vm.eVMan";

        public int ScanDistance
        {
            get
            {
                return m_scanDistance;
            }
            set
            {
                m_scanDistance = value;
            }
        }

        public Vector2 LoadingPosition
        {
            get
            {
                return m_loadPos;
            }
            set
            {
                m_loadPos = value;
            }
        }

        public float DestroyDistance
        {
            get
            {
                return m_destroyDistance;
            }
            set
            {
                m_destroyDistance = value;
            }
        }

        public int GridDepth
        {
            get
            {
                return m_voxelWidth;
            }
        }

        public int Size
        {
            get
            {
                return GridDepth * GridDepth * GridDepth;
            }
        }

        public float VoxelSize
        {
            get
            {
                return m_voxelSize;
            }
        }

        public int StackSize
        {
            get
            {
                return m_stackSize;
            }
        }

        public float VoxelObjectSize
        {
            get
            {
                return VoxelSize * (GridDepth - 1);
            }
        }

        public IFileSystem FileSystem
        {
            get
            {
                return m_fileSystem;
            }
            set
            {
                m_fileSystem = value;

                if (m_fileSystem != null)
                {
                    Save();
                }
            }
        }

        public bool Threaded
        {
            get
            {
                return m_threaded;
            }
        }

        public int UpdateQueueSize
        {
            get
            {
                return m_updateChunk.Count;
            }
        }

        VoxelManager ()
        {
            m_chunks = new ConcurrentDictionary<Vector3, Chunk>();

            m_shutDown = false;

            m_save = false;

            m_loadPos = Vector2.Zero;

            m_maxThreads = (uint)Math.Ceiling(Environment.ProcessorCount * 0.5f);
            m_updateThreads = new List<Thread>();
            m_updateChunk = new ConcurrentQueue<Chunk>();
        }

        public VoxelManager (int a_pow2, int a_stackSize, float a_voxelSize, int a_scanDistance, float a_destroyDistance, IGenerator a_worldGenerator, Material a_material, bool a_threaded, Pipeline a_pipeline, Graphics.Graphics a_graphics) : this()
        {
            m_voxelWidth = (int)Math.Pow(2, a_pow2) + 1;

            m_voxelSize = a_voxelSize;

            m_scanDistance = a_scanDistance;
            m_destroyDistance = a_destroyDistance;

            m_threaded = a_threaded;

            m_worldGenerator = a_worldGenerator;

            m_material = a_material;

            m_pipeline = a_pipeline;
            m_graphics = a_graphics;

            m_stackSize = a_stackSize;

            m_worldGenerator.SetVoxelManager(this);

            if (m_threaded)
            {
                m_voxelThread = new Thread(VoxelThread)
                {
                    Name = "Voxel Manager"
                };

                m_voxelThread.Start();
            }
        }

        void UpdateChunks ()
        {
            Chunk chunk;

            int count = 0;

            while (!m_shutDown)
            {
                if (m_updateChunk.TryDequeue(out chunk))
                {
                    count = 0;
                    chunk.UpdateFlag = e_UpdateFlag.Updating;
                    chunk.UpdateData();
                    chunk.UpdateFlag = e_UpdateFlag.Finished;
                }
                else
                {
                    if (count++ > 5)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        void AddVoxelObject (Vector3 a_position)
        {
            GameObject obj = new GameObject();
            obj.Transform.Translation = a_position * VoxelObjectSize;
            obj.Transform.SetStatic();

            Chunk chunk = obj.AddComponent<Chunk>();
            chunk.InitialiseVoxelObject(GridDepth, VoxelSize, m_pipeline);
            chunk.SetMaterial(m_material, m_graphics);

            byte[] bytes = null;

            string filename = "obj/" + GetFileName(chunk);

            if (m_fileSystem != null && m_fileSystem.Exists(filename))
            {
                if (!m_fileSystem.Load(filename, out bytes))
                {
                    bytes = null;
                }
            }

            if (bytes != null)
            {
                LoadObject(bytes, chunk);
            }
            else
            {
                m_worldGenerator.Generate(chunk);
            }

            m_worldGenerator.PostCreate(chunk);

            chunk.UpdateFlag = e_UpdateFlag.Pending;

            m_chunks.TryAdd(a_position, chunk);
        }

        bool VoxelObjectComp (Vector2 a_keyPos)
        {
            for (int i = 0; i < m_stackSize; ++i)
            {
                Vector3 pos = new Vector3(a_keyPos.X, i, a_keyPos.Y);

                if (!m_chunks.ContainsKey(pos))
                {
                    AddVoxelObject(pos);

                    return true;
                }
            }

            return false;
        }

        bool InnerLine (int a_increment, Vector2 a_position)
        {
            Vector2 keyPos = Vector2.Zero;

            keyPos = a_position + new Vector2(0, a_increment);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(a_increment, 0);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(0, -a_increment);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_increment, 0);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            return false;
        }
        bool Edge (int a_i, int a_j, Vector2 a_position)
        {
            Vector2 keyPos = Vector2.Zero;

            keyPos = a_position + new Vector2(a_j, a_i);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_j, a_i);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(a_i, a_j);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(a_i, -a_j);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_i, a_j);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_i, -a_j);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(a_j, -a_i);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_j, -a_i);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            return false;
        }
        bool Corner (int a_increment, Vector2 a_position)
        {
            Vector2 keyPos = Vector2.Zero;

            keyPos = a_position + new Vector2(a_increment, a_increment);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_increment, a_increment);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(a_increment, -a_increment);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            keyPos = a_position + new Vector2(-a_increment, -a_increment);
            if (VoxelObjectComp(keyPos))
            {
                return true;
            }

            return false;
        }

        string GetFileName (Chunk a_chunk)
        {
            Vector3 translation = a_chunk.Transform.Translation / VoxelObjectSize;

            long fileHash = 0;
            fileHash |= (long)translation.X << 0;
            fileHash |= (long)translation.Z << 24;
            fileHash |= (long)translation.Y << 48;

            return string.Format("{0}.eVox", fileHash);
        }

        void SaveObject (Chunk a_chunk)
        {
            if (m_fileSystem != null)
            {
                if (a_chunk.Updated)
                {
                    string fileName = "obj/" + GetFileName(a_chunk);

                    DistanceField<Chunk.Voxel> distance = a_chunk.DistanceField;
                    // Work around for issues getting the size of generics
                    int size = DistanceField<Chunk.Voxel>.Cell.DataSize;
                    int len = distance.Cells.Length;

                    byte[] bytes = new byte[len * size];

                    for (int i = 0; i < distance.Cells.Length; ++i)
                    {
                        Array.Copy(distance.Cells[i].GetBytes(), 0, bytes, i * size, size);
                    }

                    m_fileSystem.Save(fileName, bytes);
                }
            }
        }

        void LoadObject (byte[] a_bytes, Chunk a_chunk)
        {
            DistanceField<Chunk.Voxel> distField = a_chunk.DistanceField;

            int cellSize = DistanceField<Chunk.Voxel>.Cell.DataSize;

            for (int i = 0; i < distField.Cells.Length; ++i)
            {
                distField.Cells[i] = DistanceField<Chunk.Voxel>.Cell.FromBytes(a_bytes, i * cellSize);
            }
        }

        void RemoveObject (Chunk a_chunk)
        {
            m_worldGenerator.DestroyChunk(a_chunk);

            Vector3 key = (a_chunk.Transform.Translation / VoxelObjectSize);

            a_chunk.Transform.Translation = Vector3.One * float.PositiveInfinity;

            Chunk temp = null;
            m_chunks.TryRemove(key, out temp);

            a_chunk.GameObject.Dispose();
        }

        bool VoxelInit ()
        {
            Vector2 position = new Vector2((float)Math.Floor(m_loadPos.X / VoxelObjectSize), (float)Math.Floor(m_loadPos.Y / VoxelObjectSize));

            for (int i = 0; i < m_scanDistance; ++i)
            {
                if (InnerLine(i, position))
                {
                    return true;
                }

                for (int j = 1; j < i; ++j)
                {
                    if (Edge(i, j, position))
                    {
                        return true;
                    }
                }

                if (Corner(i, position))
                {
                    return true;
                }
            }

            return false;
        }

        bool VoxelUpdate ()
        {
            ICollection<Chunk> vals = m_chunks.Values;

            bool fnd = false;

            for (IEnumerator<Chunk> iter = vals.GetEnumerator(); iter.MoveNext();)
            {
                Chunk chunk = iter.Current;

                if ((chunk.Transform.Translation.Xz - m_loadPos).LengthSquared >= (m_destroyDistance * m_destroyDistance))
                {
                    SaveObject(chunk);

                    RemoveObject(chunk);

                    fnd = true;
                }
                else if (chunk.Update)
                {
                    fnd = true;

                    m_updateChunk.Enqueue(chunk);
                    chunk.UpdateFlag = e_UpdateFlag.Queued;
                }
            }

            return fnd;
        }

        bool SaveAll ()
        {
            if (m_save)
            {
                if (m_fileSystem != null)
                {
                    SaveManager();

                    foreach (Chunk vox in m_chunks.Values)
                    {
                        SaveObject(vox);
                    }
                }

                return true;
            }

            return false;
        }

        public void ClearAll ()
        {
            foreach (Chunk chunk in m_chunks.Values)
            {
                RemoveObject(chunk);
            }
        }

        public void Update ()
        {
            int trig = 0;

            if (!VoxelInit())
            {
                ++trig;
            }
            if (!VoxelUpdate())
            {
                ++trig;
            }
            if (!SaveAll())
            {
                ++trig;
            }

            int count = m_updateThreads.Count;

            for (int i = 0; i < count; ++i)
            {
                Thread thread = m_updateThreads[i];

                if (!thread.IsAlive)
                {
                    m_updateThreads.RemoveAt(i);
                    --count;

                    break;
                }
            }

            if (count < m_maxThreads && count * 3 <= m_updateChunk.Count)
            {
                Thread thread = new Thread(UpdateChunks)
                {
                    Name = "Chunk Update"
                };

                thread.Start();
                m_updateThreads.Add(thread);
            }
        }

        void VoxelThread ()
        {
            m_join = false;
            m_shutDown = false;

            while (!m_shutDown)
            {
                Update();
            }

            m_join = true;
        }

        void SaveManager ()
        {
            if (m_fileSystem != null)
            {
                byte[] bytes = new byte[20];

                Array.Copy(BitConverter.GetBytes(GridDepth), 0, bytes, 0, 4);
                Array.Copy(BitConverter.GetBytes(StackSize), 0, bytes, 4, 4);
                Array.Copy(BitConverter.GetBytes(VoxelSize), 0, bytes, 8, 4);
                Array.Copy(BitConverter.GetBytes(ScanDistance), 0, bytes, 12, 4);
                Array.Copy(BitConverter.GetBytes(DestroyDistance), 0, bytes, 16, 4);

                byte[] world = m_worldGenerator.SaveGenerator();

                byte[] final = new byte[bytes.Length + world.Length];
                Array.Copy(bytes, 0, final, 0, bytes.Length);
                Array.Copy(world, 0, final, bytes.Length, world.Length);

                m_fileSystem.Save(VoxelManagerFileName, final);
            }
        }

        public void Save ()
        {
            m_save = true;
        }

        public static VoxelManager Load (IFileSystem a_fileSystem, bool a_threaded, Material a_material, IGenerator a_generator, Pipeline a_pipeline, Graphics.Graphics a_grapics)
        {
            byte[] bytes = null;

            if (a_fileSystem != null)
            {
                if (!a_fileSystem.Load(VoxelManagerFileName, out bytes))
                {
                    bytes = null;
                }
            }

            if (bytes != null)
            {
                VoxelManager voxelManager = new VoxelManager
                {
                    m_pipeline = a_pipeline,
                    m_graphics = a_grapics,

                    m_worldGenerator = a_generator,

                    m_material = a_material
                };

                int off = 0;

                voxelManager.m_voxelWidth = BitConverter.ToInt32(bytes, off);
                off += sizeof(int);

                voxelManager.m_stackSize = BitConverter.ToInt32(bytes, off);
                off += sizeof(int);

                voxelManager.m_voxelSize = BitConverter.ToSingle(bytes, off);
                off += sizeof(float);

                voxelManager.m_scanDistance = BitConverter.ToInt32(bytes, off);
                off += sizeof(int);

                voxelManager.m_destroyDistance = BitConverter.ToSingle(bytes, off);
                off += sizeof(float);

                byte[] genBytes = new byte[bytes.Length - off];

                Array.Copy(bytes, off, genBytes, 0, genBytes.Length);

                a_generator.SetVoxelManager(voxelManager);

                voxelManager.m_worldGenerator.LoadGenerator(genBytes);
                voxelManager.m_threaded = a_threaded;

                if (a_threaded)
                {
                    voxelManager.m_voxelThread = new Thread(voxelManager.VoxelThread)
                    {
                        Name = "Voxel Manager"
                    };

                    voxelManager.m_voxelThread.Start();
                }

                voxelManager.m_fileSystem = a_fileSystem;

                return voxelManager;
            }

            return null;
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_shutDown = true;

            if (m_threaded)
            {
                while (!m_join)
                {
                    Thread.Yield();
                }

                if (m_voxelThread != null)
                {
                    m_voxelThread.Join();
                }
            }

            ClearAll();
        }

        ~VoxelManager ()
        {
            Dispose(false);
        }

        public Chunk GetChunk (Vector3 a_position)
        {
            int gridDepth = GridDepth;
            int halfDepth = gridDepth / 2;

            float voxelSize = VoxelSize;
            float chunkWidth = voxelSize * gridDepth;

            int x = (int)(a_position.X / voxelSize) + halfDepth;
            int y = (int)(a_position.Y / voxelSize) + halfDepth;
            int z = (int)(a_position.Z / voxelSize) + halfDepth;

            Vector3 chunkIndex = new Vector3((float)Math.Floor(x / (float)gridDepth), (float)Math.Floor(y / (float)gridDepth), (float)Math.Floor(z / (float)gridDepth));

            if (m_chunks.ContainsKey(chunkIndex))
            {
                return m_chunks[chunkIndex];
            }

            return null;
        }
        public float GetDistance (Vector3 a_position)
        {
            Vector3 chunkIndex;
            int x, y, z;
            SnapToVoxelGrid(a_position, out chunkIndex, out x, out y, out z);

            if (m_chunks.ContainsKey(chunkIndex))
            {
                return m_chunks[chunkIndex].DistanceField.GetCell(x, y, z).Distance;
            }

            return float.NaN;
        }
        public void SnapToVoxelGrid (Vector3 a_position, out Vector3 a_chunkIndex, out int a_x, out int a_y, out int a_z)
        {
            int gridDepth = GridDepth;
            int halfDepth = gridDepth / 2;

            float voxelSize = VoxelSize;
            float chunkWidth = voxelSize * gridDepth;

            int chunkSize = gridDepth - 1;

            // Moves the voxel space
            int x = (int)(a_position.X / voxelSize) + halfDepth;
            int y = (int)(a_position.Y / voxelSize) + halfDepth;
            int z = (int)(a_position.Z / voxelSize) + halfDepth;

            // Moves to chunk space
            a_chunkIndex = new Vector3((float)Math.Floor(x / (float)gridDepth), (float)Math.Floor(y / (float)gridDepth), (float)Math.Floor(z / (float)gridDepth));

            // Correction for negative coordinates
            int xMulti = (int)-Math.Ceiling((Math.Sign(x) - 1) / 2.0);
            int yMulti = (int)-Math.Ceiling((Math.Sign(y) - 1) / 2.0);
            int zMulti = (int)-Math.Ceiling((Math.Sign(z) - 1) / 2.0);

            // Moves the local voxel space
            a_x = (xMulti * chunkSize) + (x % gridDepth);
            a_y = (yMulti * chunkSize) + (y % gridDepth);
            a_z = (zMulti * chunkSize) + (z % gridDepth);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}