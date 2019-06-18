using Erde.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Erde.Graphics.Rendering
{
    public enum e_PrimitiveType
    {
        Null,
        Cube
    }

    public class Model : IGLObject
    {
        public struct Vertex
        {
            public Vector4 position;
            public Vector3 normal;
            public Vector2 texCoords;

            public Vertex (Vector4 a_position, Vector3 a_normal, Vector2 a_texCoords)
            {
                position = a_position;
                normal = a_normal;
                texCoords = a_texCoords;
            }
        }

        int             m_VBO;
        int             m_IBO;
        int             m_VAO;

        ushort          m_indicies;

        float           m_radius;
        e_PrimitiveType m_primitive;

        string          m_fileName;
        IFileSystem     m_fileSystem;

        Pipeline        m_pipeline;

        public ushort Indicies
        {
            get
            {
                return m_indicies;
            }
        }

        public int VertexBufferObject
        {
            get
            {
                return m_VBO;
            }
        }

        public float Radius
        {
            get
            {
                return m_radius;
            }
        }

        public int IndicieBufferObject
        {
            get
            {
                return m_IBO;
            }
        }

        public int VertexArrayObject
        {
            get
            {
                return m_VAO;
            }
        }

        Model ()
        {
            m_primitive = e_PrimitiveType.Null;
        }

        void Dispose (bool a_state)
        {
            Tools.Verify(this, a_state);

            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~Model ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void SetData (Vertex[] a_vertices, ushort[] a_indices, float a_radius)
        {
            // Sets the number of indicies the mesh contains
            m_indicies = (ushort)a_indices.Length;

            // Sets the radius for frustrum culling
            m_radius = a_radius;

            // Generates the Vertex Buffer Object, Vertex Array Object and Indice Buffer Object
            m_VBO = GL.GenBuffer();
            m_IBO = GL.GenBuffer();
            m_VAO = GL.GenVertexArray();

            GL.BindVertexArray(m_VAO);

            // Binds the Vertex Buffer Object to the Vertex Array Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_VBO);
            // Sends the Vertex Information to the GPU
            GL.BufferData(BufferTarget.ArrayBuffer, a_vertices.Length * Marshal.SizeOf<Vertex>(), a_vertices, BufferUsageHint.StaticDraw);

            // Binds the Indicie Buffer Object to the Vertex Array Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_IBO);
            // Sends the Indice information to the GPU
            GL.BufferData(BufferTarget.ElementArrayBuffer, m_indicies * sizeof(ushort), a_indices, BufferUsageHint.StaticDraw);

            // Sets up the memory layout for shader inputs
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>("position"));
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>("normal"));
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>("texCoords"));
        }

        void GenerateCube ()
        {
            // Cubes vertex data contatining the position, normals and uvs
            Vertex[] vertices =
            {
                new Vertex(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), new Vector3(-1.0f, 0.0f,  0.0f),  new Vector2(1.0f, 1.0f)), // 0
				new Vertex(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), new Vector3(0.0f,  -1.0f, 0.0f),  new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), new Vector3(0.0f,  0.0f,  -1.0f), new Vector2(0.0f, 1.0f)),

                new Vertex(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector3(1.0f,  0.0f,  0.0f),  new Vector2(0.0f, 1.0f)), // 3
				new Vertex(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector3(0.0f,  -1.0f, 0.0f),  new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector3(0.0f,  0.0f,  -1.0f), new Vector2(1.0f, 1.0f)),

                new Vertex(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector3(-1.0f, 0.0f,  0.0f),  new Vector2(0.0f, 1.0f)), // 6
				new Vertex(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector3(0.0f,  -1.0f, 0.0f),  new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector3(0.0f,  0.0f,  1.0f),  new Vector2(1.0f, 1.0f)),

                new Vertex(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector3(1.0f,  0.0f,  0.0f),  new Vector2(1.0f, 1.0f)), // 9
				new Vertex(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector3(0.0f,  -1.0f, 0.0f),  new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector3(0.0f,  0.0f,  1.0f),  new Vector2(0.0f, 1.0f)),

                new Vertex(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector3(-1.0f, 0.0f,  0.0f),  new Vector2(1.0f, 0.0f)), // 12
				new Vertex(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector3(0.0f,  1.0f,  0.0f),  new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector3(0.0f,  0.0f,  -1.0f), new Vector2(0.0f, 0.0f)),

                new Vertex(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector3(1.0f,  0.0f,  0.0f),  new Vector2(0.0f, 0.0f)), // 15
				new Vertex(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector3(0.0f,  1.0f,  0.0f),  new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector3(0.0f,  0.0f,  -1.0f), new Vector2(1.0f, 0.0f)),

                new Vertex(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector3(-1.0f, 0.0f,  0.0f),  new Vector2(0.0f, 0.0f)), // 18
				new Vertex(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector3(0.0f,  1.0f,  0.0f),  new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector3(0.0f,  0.0f,  1.0f),  new Vector2(1.0f, 0.0f)),

                new Vertex(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector3(1.0f,  0.0f,  0.0f),  new Vector2(1.0f, 0.0f)), // 21
				new Vertex(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector3(0.0f,  1.0f,  0.0f),  new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector3(0.0f,  0.0f,  1.0f),  new Vector2(0.0f, 0.0f))
            };

            // Cubes indicie data
            ushort[] indicies =
            {
                0,  6,  18, 0,  18, 12,
                1,  4,  10, 1,  10, 7,
                2,  17, 5,  2,  14, 17,
                3,  21, 9,  3,  15, 21,
                8,  11, 23, 8,  23, 20,
                13, 22, 16, 13, 19, 22
            };

            SetData(vertices, indicies, Vector3.One.Length);
        }
        void LoadModel (string[] a_data)
        {
            List<Vector3> vertexPosition = new List<Vector3>();
            List<Vector3> vertexNormal = new List<Vector3>();
            List<Vector2> vertexTextureCoords = new List<Vector2>();

            List<Vertex> vertices = new List<Vertex>();
            List<ushort> indices = new List<ushort>();

            Dictionary<Vertex, ushort> vertexLookup = new Dictionary<Vertex, ushort>();

            float lengthSqr = 0.0f;

            foreach (string line in a_data)
            {
                string l = line.ToLower();

                int index = l.IndexOf(' ');

                string data = l.Substring(index + 1, l.Length - (index + 1));

                switch (l.Substring(0, index))
                {
                case "#":
                case "o":
                    {
                        break;
                    }
                case "v":
                    {
                        Vector3 vertex = Vector3.Zero;

                        string[] strings = data.Split(' ');

                        vertex.X = float.Parse(strings[0]);
                        vertex.Y = float.Parse(strings[1]);
                        vertex.Z = float.Parse(strings[2]);

                        lengthSqr = Math.Max(lengthSqr, vertex.LengthSquared);

                        vertexPosition.Add(vertex);

                        break;
                    }
                case "vn":
                    {
                        Vector3 vertex = Vector3.Zero;

                        string[] strings = data.Split(' ');
                        
                        vertex.X = float.Parse(strings[0]);
                        vertex.Y = float.Parse(strings[1]);
                        vertex.Z = float.Parse(strings[2]);

                        vertexNormal.Add(vertex);

                        break;
                    }
                case "vt":
                    {
                        Vector2 vertex = Vector2.Zero;

                        string[] strings = data.Split(' ');
                        
                        vertex.X = float.Parse(strings[0]);
                        vertex.Y = 1 - float.Parse(strings[1]);

                        vertexTextureCoords.Add(vertex);

                        break;
                    }
                case "f":
                    {
                        string[] strings = data.Split(' ');

                        foreach (string s in strings)
                        {
                            string[] ind = s.Split('/');

                            Vertex vert = new Vertex()
                            {
                                position = new Vector4(vertexPosition[int.Parse(ind[0]) - 1], 1)
                            };

                            if (ind.Length == 2)
                            {
                                if (s.Count(f => f == '/') == 2)
                                {
                                    vert.normal = vertexNormal[int.Parse(ind[1]) - 1];
                                }
                                else
                                {
                                    vert.texCoords = vertexTextureCoords[int.Parse(ind[1]) - 1];
                                }
                            }
                            else
                            {
                                vert.texCoords = vertexTextureCoords[int.Parse(ind[1]) - 1];
                                vert.normal = vertexNormal[int.Parse(ind[2]) - 1];
                            }

                            ushort val = 0;

                            if (!vertexLookup.TryGetValue(vert, out val))
                            {
                                vertices.Add(vert);
                                val = (ushort)(vertices.Count - 1);
                                vertexLookup.Add(vert, val);
                            }
                            
                            indices.Add(val);
                        }

                        break;
                    }
                }
            }

            SetData(vertices.ToArray(), indices.ToArray(), (float)Math.Sqrt(lengthSqr));
        }
        
        public static Model CreatePrimitive (e_PrimitiveType a_primitive, Pipeline a_pipeline)
        {
            Model model = new Model();
            model.m_primitive = a_primitive;
            model.m_fileName = string.Empty;
            model.m_pipeline = a_pipeline;

            a_pipeline.InputQueue.Enqueue(model);

            return model;
        }

        public static Model LoadModel (string a_fileName, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            Model model = new Model();
            model.m_primitive = e_PrimitiveType.Null;
            model.m_fileName = a_fileName;
            model.m_fileSystem = a_fileSystem;
            model.m_pipeline = a_pipeline;

            a_pipeline.InputQueue.Enqueue(model);

            return model;
        }

        public void ModifyObject ()
        {
            switch (m_primitive)
            {
            case e_PrimitiveType.Null:
                {
                    // LoadModel();
                    if (m_fileSystem != null)
                    {
                        byte[] bytes;
                        if (m_fileSystem.Load(m_fileName, out bytes))
                        {
                            string[] lines = Encoding.UTF8.GetString(bytes).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                            LoadModel(lines);
                        }
                    }

                    break;
                }
            case e_PrimitiveType.Cube:
                {
                    GenerateCube();

                    break;
                }
            default:
                {
                    InternalConsole.AddMessage("Model: Invalid Primitive Type", InternalConsole.e_Alert.Error);

                    break;
                }
            }
        }

        public void DisposeObject ()
        {
            GL.DeleteBuffer(m_VBO);
            GL.DeleteBuffer(m_IBO);
            GL.DeleteVertexArray(m_VAO);
        }
    }
}