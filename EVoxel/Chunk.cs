using Erde.Graphics;
using Erde.Graphics.Lights;
using Erde.Graphics.Rendering;
using Erde.Graphics.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Erde.Voxel
{
    public enum e_UpdateFlag
    {
        Pending,
        Queued,
        Updating,
        Finished
    };

    public class Chunk : Renderer, IGraphicsObject
    {
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Voxel
        {
            uint m_color;

            public Color Color
            {
                get
                {
                    return Color.FromArgb(255, (int)(m_color >> 0) & 0xFF, (int)(m_color >> 8) & 0xFF, (int)(m_color >> 16) & 0xFF);
                }
                set
                {
                    m_color = (uint)value.R << 0 | (uint)value.G << 8 | (uint)value.B << 16;
                }
            }

            public uint UColor
            {
                get
                {
                    return m_color;
                }
            }

            public uint Specular
            {
                get
                {
                    return (m_color >> 24) & 0xFF;
                }
                set
                {
                    m_color |= value << 24;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ChunkVertex
        {
            public Vector4 Position;
            public Vector3 Normal;
            public uint Color;

            public static bool operator != (ChunkVertex a_vertexA, ChunkVertex a_vertexB)
            {
                return !(a_vertexA == a_vertexB);
            }
            public static bool operator == (ChunkVertex a_vertexA, ChunkVertex a_vertexB)
            {
                return a_vertexA.Position == a_vertexB.Position;
            }

            public override bool Equals (object obj)
            {
                if (!(obj is ChunkVertex))
                {
                    return false;
                }

                ChunkVertex vertex = (ChunkVertex)obj;
                return Position.Equals(vertex.Position);
            }
            public override int GetHashCode ()
            {
                int hashCode = -1823519222;
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(Position);
                return hashCode;
            }
        }

        DistanceField<Voxel> m_distanceField;

        byte                 m_update;

        Model                m_model;

        Pipeline             m_pipeline;

        e_UpdateFlag         m_updateFlag;

        public int Size
        {
            get
            {
                return m_distanceField.Width * m_distanceField.Height * m_distanceField.Depth;
            }
        }

        public DistanceField<Voxel> DistanceField
        {
            get
            {
                return m_distanceField;
            }
        }

        public e_UpdateFlag UpdateFlag
        {
            get
            {
                return m_updateFlag;
            }
            internal set
            {
                m_updateFlag = value;
            }
        }

        public bool Update
        {
            get
            {
                return m_updateFlag == e_UpdateFlag.Pending;
            }
            set
            {
                if (value)
                {
                    m_updateFlag = e_UpdateFlag.Pending;
                }

                Updated = true;
            }
        }

        public bool Updated
        {
            get
            {
                return (m_update & (1 << 1)) != 0;
            }
            internal set
            {
                if (value == false)
                {
                    m_update = (byte)((m_update | (1 << 1)) ^ (1 << 1));
                }
                else
                {
                    m_update |= 1 << 1;
                }
            }
        }

        public bool Initialised
        {
            get
            {
                return m_distanceField != null;
            }
        }

        public override float Radius
        {
            get
            {
                return m_model.Radius;
            }
        }

        public override uint Indices
        {
            get
            {
                return m_model.Indices;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible && Indices > 0;
            }
        }

        public void InitialiseVoxelObject (int a_size, float a_spacing, Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            m_distanceField = new DistanceField<Voxel>(a_size, a_size, a_size, a_spacing);

            m_update = 0;

            m_model = new Model(m_pipeline);
        }

        public void UpdateData ()
        {
            int width = m_distanceField.Width;
            int height = m_distanceField.Height;
            int depth = m_distanceField.Depth;

            int halfWidth = width / 2;
            int halfHeight = height / 2;
            int halfDepth = depth / 2;

            DistanceField<Voxel>.Cell[] voxels = new DistanceField<Voxel>.Cell[8];
            DistanceField<Voxel>.Cell[] cells = m_distanceField.Cells;

            List<ChunkVertex> dirtyVerts = new List<ChunkVertex>();

            for (int x = 0; x < m_distanceField.Width - 1; ++x)
            {
                int x1 = x + 1;
                for (int y = 0; y < m_distanceField.Height - 1; ++y)
                {
                    int y1 = y + 1;
                    for (int z = 0; z < m_distanceField.Depth - 1; ++z)
                    {
                        int z1 = z + 1;

                        voxels[0] = cells[(z * width * height) + (y * width) + x];
                        voxels[1] = cells[(z * width * height) + (y * width) + x1];
                        voxels[2] = cells[(z1 * width * height) + (y * width) + x1];
                        voxels[3] = cells[(z1 * width * height) + (y * width) + x];
                        voxels[4] = cells[(z * width * height) + (y1 * width) + x];
                        voxels[5] = cells[(z * width * height) + (y1 * width) + x1];
                        voxels[6] = cells[(z1 * width * height) + (y1 * width) + x1];
                        voxels[7] = cells[(z1 * width * height) + (y1 * width) + x];
                        
                        MarchingCubes.Polygonise(m_distanceField, voxels, x - halfWidth, y - halfHeight, z - halfDepth, 0.0f, dirtyVerts);
                    }
                }
            }

            int count = dirtyVerts.Count;

            List<uint> indicies = new List<uint>();
            List<ChunkVertex> vertices = new List<ChunkVertex>();

            Dictionary<ChunkVertex, uint> values = new Dictionary<ChunkVertex, uint>();

            for (int i = 0; i < count; i += 3)
            {
                ChunkVertex vertA = dirtyVerts[i];
                ChunkVertex vertB = dirtyVerts[i + 1];
                ChunkVertex vertC = dirtyVerts[i + 2];

                Vector3 v1 = vertC.Position.Xyz - vertA.Position.Xyz;
                Vector3 v2 = vertB.Position.Xyz - vertA.Position.Xyz;

                Vector3 normal = Vector3.Cross(v2, v1);
                
                uint index;
                if (values.TryGetValue(vertA, out index))
                {
                    ChunkVertex vertex = vertices[(int)index];
                    vertex.Normal += normal;
                    vertices[(int)index] = vertex;
                    indicies.Add(index);
                }
                else
                {
                    index = (uint)vertices.Count;
                    vertA.Normal = normal;
                    indicies.Add(index);
                    values.Add(vertA, index);
                    vertices.Add(vertA);
                }

                if (values.TryGetValue(vertB, out index))
                {
                    ChunkVertex vertex = vertices[(int)index];
                    vertex.Normal += normal;
                    vertices[(int)index] = vertex;
                    indicies.Add(index);
                }
                else
                {
                    index = (uint)vertices.Count;
                    vertB.Normal = normal;
                    indicies.Add(index);
                    values.Add(vertB, index);
                    vertices.Add(vertB);
                }

                if (values.TryGetValue(vertC, out index))
                {
                    ChunkVertex vertex = vertices[(int)index];
                    vertex.Normal += normal;
                    vertices[(int)index] = vertex;
                    indicies.Add(index);
                }
                else
                {
                    index = (uint)vertices.Count;
                    vertC.Normal = normal;
                    indicies.Add(index);
                    values.Add(vertC, index);
                    vertices.Add(vertC);
                }
            }

            float maxRadius = 0.0f;
            int vertexCount = vertices.Count;

            for (int i = 0; i < vertexCount; ++i)
            {
                ChunkVertex vert = vertices[i];
                vert.Normal = vert.Normal.Normalized();
                vertices[i] = vert;

                float distSqr = vert.Position.LengthSquared;
                if (distSqr > maxRadius)
                {
                    maxRadius = distSqr;
                }
            }

            float radius = (float)Math.Sqrt(maxRadius);

            ModelVertexInfo[] vertexInfoCollection = new ModelVertexInfo[3];
            
            vertexInfoCollection[0].Offset = Marshal.OffsetOf<ChunkVertex>("Position");
            vertexInfoCollection[0].Count = 4;
            vertexInfoCollection[0].Type = e_FieldType.Float;
            vertexInfoCollection[0].Normalize = false;

            vertexInfoCollection[1].Offset = Marshal.OffsetOf<ChunkVertex>("Normal");
            vertexInfoCollection[1].Count = 3;
            vertexInfoCollection[1].Type = e_FieldType.Float;
            vertexInfoCollection[1].Normalize = false;

            vertexInfoCollection[2].Offset = Marshal.OffsetOf<ChunkVertex>("Color");
            vertexInfoCollection[2].Count = 4;
            vertexInfoCollection[2].Type = e_FieldType.UnsignedByte;
            vertexInfoCollection[2].Normalize = true;

            m_model.SetModelData<ChunkVertex>(vertices.ToArray(), indicies.ToArray(), radius, vertexInfoCollection);
        }

        public override void Draw (Camera a_camera)
        {
            if (m_model != null)
            {
                lock (this)
                {
                    GraphicsCommand.BindModel(m_model);

                    GraphicsCommand.DrawElementsUInt(m_model.Indices);
                }
            }
        }

        public override void DrawShadow (Light a_light)
        {
            if (m_model != null)
            {
                lock (this)
                {
                    GraphicsCommand.BindModel(m_model);

                    GraphicsCommand.DrawElementsUInt(m_model.Indices);
                }
            }
        }

        public void ModifyObject ()
        {

        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);

            if (m_model != null)
            {
                lock (this)
                {
                    m_model.Dispose();
                }
            }
        }

        ~Chunk ()
        {
            Dispose(false);
        }

        public override void Dispose ()
        {
            base.Dispose();   

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DisposeObject ()
        {
            
        }
    }
}