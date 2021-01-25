using Erde.Graphics;
using Erde.Graphics.Lights;
using Erde.Graphics.Rendering;
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
        public struct Vertex
        {
            public Vector4 Position;
            public Vector3 Normal;
            public uint Color;

            public static bool operator != (Vertex a_vertexA, Vertex a_vertexB)
            {
                return !(a_vertexA == a_vertexB);
            }
            public static bool operator == (Vertex a_vertexA, Vertex a_vertexB)
            {
                return a_vertexA.Position == a_vertexB.Position;
            }

            public override bool Equals (object obj)
            {
                if (!(obj is Vertex))
                {
                    return false;
                }

                Vertex vertex = (Vertex)obj;
                return Position.Equals(vertex.Position);
            }
            public override int GetHashCode ()
            {
                int hashCode = -1823519222;
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(Position);
                return hashCode;
            }
        }

        internal class MeshGenerator : IGraphicsObject
        {
            Chunk        m_voxelObject;
            List<Vertex> m_verticies;
            List<uint>   m_indices;

            public List<Vertex> Verticies
            {
                get
                {
                    return m_verticies;
                }
            }
            
            public List<uint> Indicies
            {
                get
                {
                    return m_indices;
                }
            }

            public Chunk Object
            {
                get
                {
                    return m_voxelObject;
                }
            }

            public MeshGenerator (Chunk a_voxelObject)
            {
                m_voxelObject = a_voxelObject;
                m_verticies = new List<Vertex>();
                m_indices = new List<uint>();
            }

            public void ModifyObject ()
            {
                m_voxelObject.PopulateBuffers(this);
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }
        }

        DistanceField<Voxel> m_distanceField;

        byte                 m_update;

        int                  m_vbo;
        int                  m_ibo;

        int                  m_vao;

        uint                 m_indices;

        Pipeline             m_pipeline;

        float                m_radius;

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
                return m_radius;
            }
        }

        public override uint Indices
        {
            get
            {
                return m_indices;
            }
        }

        public void InitialiseVoxelObject (int a_size, float a_spacing, Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            m_distanceField = new DistanceField<Voxel>(a_size, a_size, a_size, a_spacing);

            m_update = 0;

            m_pipeline.AddObject(this);
        }

        MeshGenerator UpdateMesh ()
        {
            int width = m_distanceField.Width;
            int height = m_distanceField.Height;
            int depth = m_distanceField.Depth;

            int halfWidth = width / 2;
            int halfHeight = height / 2;
            int halfDepth = depth / 2;

            MeshGenerator gen = new MeshGenerator(this);

            DistanceField<Voxel>.Cell[] voxels = new DistanceField<Voxel>.Cell[8];

            DistanceField<Voxel>.Cell[] cells = m_distanceField.Cells;

            List<Vertex> vertices = new List<Vertex>();

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
                        
                        MarchingCubes.Polygonise(m_distanceField, voxels, x - halfWidth, y - halfHeight, z - halfDepth, 0.0f, vertices);
                    }
                }
            }

            int count = vertices.Count;

            gen.Indicies.Capacity = count;
            gen.Verticies.Capacity = count / 3;

            Dictionary<Vertex, uint> values = new Dictionary<Vertex, uint>();

            for (int i = 0; i < count; i += 3)
            {
                Vertex vertA = vertices[i];
                Vertex vertB = vertices[i + 1];
                Vertex vertC = vertices[i + 2];

                Vector3 v1 = vertC.Position.Xyz - vertA.Position.Xyz;
                Vector3 v2 = vertB.Position.Xyz - vertA.Position.Xyz;

                Vector3 normal = Vector3.Cross(v2, v1);
                
                uint index;
                if (values.TryGetValue(vertA, out index))
                {
                    Vertex vertex = gen.Verticies[(int)index];
                    vertex.Normal += normal;
                    gen.Verticies[(int)index] = vertex;
                    gen.Indicies.Add(index);
                }
                else
                {
                    index = (uint)gen.Verticies.Count;
                    vertA.Normal = normal;
                    gen.Indicies.Add(index);
                    values.Add(vertA, index);
                    gen.Verticies.Add(vertA);
                }
                if (values.TryGetValue(vertB, out index))
                {
                    Vertex vertex = gen.Verticies[(int)index];
                    vertex.Normal += normal;
                    gen.Verticies[(int)index] = vertex;
                    gen.Indicies.Add(index);
                }
                else
                {
                    index = (uint)gen.Verticies.Count;
                    vertB.Normal = normal;
                    gen.Indicies.Add(index);
                    values.Add(vertB, index);
                    gen.Verticies.Add(vertB);
                }
                if (values.TryGetValue(vertC, out index))
                {
                    Vertex vertex = gen.Verticies[(int)index];
                    vertex.Normal += normal;
                    gen.Verticies[(int)index] = vertex;
                    gen.Indicies.Add(index);
                }
                else
                {
                    index = (uint)gen.Verticies.Count;
                    vertC.Normal = normal;
                    gen.Indicies.Add(index);
                    values.Add(vertC, index);
                    gen.Verticies.Add(vertC);
                }
            }

            float maxRadius = 0.0f;
            for (int i = 0; i < gen.Verticies.Count; ++i)
            {
                Vertex vert = gen.Verticies[i];
                vert.Normal = vert.Normal.Normalized();
                gen.Verticies[i] = vert;

                float distSqr = vert.Position.LengthSquared;
                if (distSqr > maxRadius)
                {
                    maxRadius = distSqr;
                }
            }

            Dictionary<Vertex, uint> vertLookup = new Dictionary<Vertex, uint>();

            m_radius = (float)Math.Sqrt(maxRadius);
            
            return gen;
        }

        public void UpdateData ()
        {
            m_pipeline.AddObject(UpdateMesh());
        }

        // Takes the data from UpdateMesh and sends it over to the gpu
        internal void PopulateBuffers (MeshGenerator a_generated)
        {
            // Updates the number of indicies in the mesh
            m_indices = (uint)a_generated.Indicies.Count;

            // Checks if there is suffient indicies in the mesh
            if (m_indices != 0)
            {   
                GL.BindVertexArray(m_vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
                int verticiesSize = a_generated.Verticies.Count * Marshal.SizeOf<Vertex>();
                // Sends the updated vertex data over to the gpu
                GL.BufferData(BufferTarget.ArrayBuffer, verticiesSize, a_generated.Verticies.ToArray(), BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_ibo);
                int indiciesSize = (int)m_indices * sizeof(uint);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indiciesSize, a_generated.Indicies.ToArray(), BufferUsageHint.StaticDraw);
            }
        }

        public override void Draw (Camera a_camera)
        {
            GL.BindVertexArray(m_vao);

            GL.DrawElements(PrimitiveType.Triangles, (int)m_indices, DrawElementsType.UnsignedInt, 0);
        }

        public override void DrawShadow (Light a_light)
        {
            GL.BindVertexArray(m_vao);

            GL.DrawElements(PrimitiveType.Triangles, (int)m_indices, DrawElementsType.UnsignedInt, 0);
        }

        public void ModifyObject ()
        {
            m_vbo = GL.GenBuffer();
            m_ibo = GL.GenBuffer();
            m_vao = GL.GenVertexArray();

            GL.BindVertexArray(m_vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_ibo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);

            int sizeOfVertex = Marshal.SizeOf<Vertex>();
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, sizeOfVertex, Marshal.OffsetOf<Vertex>("Position"));
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeOfVertex, Marshal.OffsetOf<Vertex>("Normal"));
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, sizeOfVertex, Marshal.OffsetOf<Vertex>("Color"));
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);
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
            GL.DeleteBuffer(m_vbo);
            GL.DeleteVertexArray(m_vao);
        }
    }
}