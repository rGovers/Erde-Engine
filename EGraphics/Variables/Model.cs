using Erde.Application;
using Erde.Graphics.Internal.Variables;
using Erde.IO;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Erde.Graphics.Variables
{
    public enum e_PrimitiveType
    {
        Cube
    }

    public struct Vertex
    {
        public Vector4 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;

        public Vertex (Vector4 a_position, Vector3 a_normal, Vector2 a_texCoords)
        {
            Position = a_position;
            Normal = a_normal;
            TexCoords = a_texCoords;
        }
    }

    public class Model : IGraphicsObject
    {
        class PrimitiveModelGenerator : IGraphicsObject
        {
            Model           m_model;
            e_PrimitiveType m_primitiveType;

            public PrimitiveModelGenerator (e_PrimitiveType a_primitiveType, Model a_model)
            {
                m_primitiveType = a_primitiveType;
                m_model = a_model;
            }

            public void Dispose ()
            {

            }

            public void DisposeObject ()
            {

            }

            public void ModifyObject ()
            {
                switch (m_primitiveType)
                {
                case e_PrimitiveType.Cube:
                {
                    m_model.GenerateCube();

                    break;
                }
                }
            }
        }

        class ModelLoader : IGraphicsObject
        {
            IFileSystem m_fileSystem;
            Model       m_model;
            string      m_fileName;

            public ModelLoader (string a_fileName, IFileSystem a_fileSystem, Model a_model)
            {
                m_fileSystem = a_fileSystem;
                m_fileName = a_fileName;
                m_model = a_model;
            }

            public void Dispose ()
            {
            }

            public void DisposeObject ()
            {
            }

            public void ModifyObject ()
            {
                if (m_fileSystem != null)
                {
                    byte[] bytes;
                    if (m_fileSystem.Load(m_fileName, out bytes))
                    {
                        string[] lines = Encoding.UTF8.GetString(bytes).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        m_model.LoadModel(lines);
                    }
                }
            }
        }

        class ModelData<T> : IGraphicsObject where T : struct
        {
            Model    m_model;

            T[]      m_vertex;
            ushort[] m_index;
            float    m_radius;

            public ModelData(Model a_model, T[] a_vertex, ushort[] a_index, float a_radius)
            {
                m_model = a_model;

                m_vertex = a_vertex;
                m_index = a_index;

                m_radius = a_radius;   
            }

            public void Dispose ()
            {
            }

            public void DisposeObject ()
            {
            }

            public void ModifyObject ()
            {
                m_model.SetData(m_vertex, m_index, m_radius);
            }
        }

        IModel   m_internalObject;

        uint     m_indices;

        float    m_radius;

        Pipeline m_pipeline;

        public uint Indices
        {
            get
            {
                return m_indices;
            }
        }

        public float Radius
        {
            get
            {
                return m_radius;
            }
        }

        public Model(Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            if (a_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKModel();
            }
            else
            {
                m_internalObject = new NativeModel(a_pipeline);
            }
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);

            m_internalObject.Dispose();        
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

        void SetData<T>(T[] a_vertices, ushort[] a_indices, float a_radius) where T : struct
        {
            m_indices = (uint)a_indices.LongLength;

            m_radius = a_radius;

            m_internalObject.SetData(a_vertices, a_indices);
        }

        public void Bind()
        {
            m_internalObject.Bind();
        }

        void GenerateCube ()
        {
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
                                Position = new Vector4(vertexPosition[int.Parse(ind[0]) - 1], 1)
                            };

                            if (ind.Length == 2)
                            {
                                if (s.Count(f => f == '/') == 2)
                                {
                                    vert.Normal = vertexNormal[int.Parse(ind[1]) - 1];
                                }
                                else
                                {
                                    vert.TexCoords = vertexTextureCoords[int.Parse(ind[1]) - 1];
                                }
                            }
                            else
                            {
                                vert.TexCoords = vertexTextureCoords[int.Parse(ind[1]) - 1];
                                vert.Normal = vertexNormal[int.Parse(ind[2]) - 1];
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
            Model model = new Model(a_pipeline);
            model.m_pipeline = a_pipeline;

            a_pipeline.AddObject(new PrimitiveModelGenerator(a_primitive, model));

            return model;
        }

        public static Model LoadModel (string a_fileName, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            Model model = new Model(a_pipeline);

            a_pipeline.AddObject(new ModelLoader(a_fileName, a_fileSystem, model));

            return model;
        }

        public void SetModelData<T>(T[] a_vertex, ushort[] a_index, float a_radius) where T : struct
        {
            m_pipeline.AddObject(new ModelData<T>(this, a_vertex, a_index, a_radius));
        }

        public void ModifyObject ()
        {
            m_internalObject.ModifyObject();
        }

        public void DisposeObject ()
        {
            m_internalObject.DisposeObject();
        }
    }
}