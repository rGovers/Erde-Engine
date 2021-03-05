using Erde.Application;
using Erde.Graphics.Internal.Variables;
using Erde.Graphics.IO;
using Erde.IO;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Variables
{
    public enum e_PrimitiveType
    {
        Cube
    }

    [StructLayout(LayoutKind.Sequential)]
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

    [StructLayout(LayoutKind.Sequential)]
    public struct SkinnedVertex
    {
        public Vector4 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;

        // Bad smell but until I think of a more elegant way of doing it.
        public Vector4 Bones;
        public Vector4 Weights;

        public SkinnedVertex(Vector4 a_position, Vector3 a_normal, Vector2 a_texCoords, Vector4 a_bones, Vector4 a_weights)
        {
            Position = a_position;
            Normal = a_normal;
            TexCoords = a_texCoords;

            Bones = a_bones;
            Weights = a_weights;
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
                    string ext = Path.GetExtension(m_fileName);

                    switch (ext.ToLower())
                    {
                        case ".obj":
                        {
                            OBJLoader objLoader = new OBJLoader(m_fileName, m_fileSystem);

                            m_model.SetData(objLoader.Vertices, objLoader.Indices, objLoader.Length, ModelVertexInfo.GetVertexInfo<Vertex>());

                            break;
                        }
                        case ".dae":
                        {
                            ColladaLoader colladaLoader = new ColladaLoader(null, m_fileName, m_fileSystem);

                            int modelCount = colladaLoader.ModelCount;

                            Vertex[] vertices;
                            uint[] indicies;
                            float len;

                            float maxLen = 0.0f;
                            List<Vertex> finalVertices = new List<Vertex>();
                            List<uint> finalIndicies = new List<uint>();
                            Dictionary<Vertex, uint> vertexLookup = new Dictionary<Vertex, uint>();

                            for (int i = 0; i < modelCount; ++i)
                            {
                                colladaLoader.GenerateModelData(i, out vertices, out indicies, out len);

                                maxLen = Math.Max(maxLen, len);

                                uint indexCount = (uint)indicies.LongLength;

                                for (uint j = 0; j < indexCount; ++j)
                                {
                                    Vertex vertex = vertices[indicies[j]];

                                    if (vertexLookup.ContainsKey(vertex))
                                    {
                                        finalIndicies.Add(vertexLookup[vertex]);
                                    }
                                    else
                                    {
                                        uint index = (uint)finalVertices.Count;

                                        vertexLookup.Add(vertex, index);
                                        finalVertices.Add(vertex);
                                        finalIndicies.Add(index);
                                    }
                                }
                            }

                            vertices = finalVertices.ToArray();
                            indicies = finalIndicies.ToArray();

                            m_model.SetData(vertices, indicies, maxLen, ModelVertexInfo.GetVertexInfo<Vertex>());

                            break;
                        }
                    }
                }
            }
        }

        class ModelData<T> : IGraphicsObject where T : struct
        {
            Model             m_model;

            T[]               m_vertex;
            uint[]            m_index;
            float             m_radius;

            ModelVertexInfo[] m_vertexInfo;

            public ModelData(Model a_model, T[] a_vertex, uint[] a_index, float a_radius, ModelVertexInfo[] a_vertexInfo)
            {
                m_model = a_model;

                m_vertex = a_vertex;
                m_index = a_index;

                m_radius = a_radius;   

                m_vertexInfo = a_vertexInfo;
            }

            public void Dispose ()
            {
            }

            public void DisposeObject ()
            {
            }

            public void ModifyObject ()
            {
                m_model.SetData(m_vertex, m_index, m_radius, m_vertexInfo);
            }
        }

        IModel   m_internalObject;

        uint     m_indices;

        float    m_radius;

        Pipeline m_pipeline;

        string   m_name;

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

        public string Name
        {
            get
            {
                return m_name;
            }
            internal set
            {
                m_name = value;
            }
        }

        public Model(Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            m_radius = 0.0f;

            m_indices = 0;

            m_name = string.Empty;

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

        internal void SetData<T>(T[] a_vertices, uint[] a_indices, float a_radius, ModelVertexInfo[] a_vertexInfo) where T : struct
        {
            m_internalObject.SetData(a_vertices, a_indices, a_vertexInfo);

            m_indices = (uint)a_indices.LongLength;

            m_radius = a_radius;
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
            uint[] indicies =
            {
                0,  6,  18, 0,  18, 12,
                1,  4,  10, 1,  10, 7,
                2,  17, 5,  2,  14, 17,
                3,  21, 9,  3,  15, 21,
                8,  11, 23, 8,  23, 20,
                13, 22, 16, 13, 19, 22
            };

            SetData(vertices, indicies, Vector3.One.Length, ModelVertexInfo.GetVertexInfo<Vertex>());
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

        public void SetModelData<T>(T[] a_vertex, uint[] a_index, float a_radius) where T : struct
        {
            SetModelData<T>(a_vertex, a_index, a_radius, ModelVertexInfo.GetVertexInfo<T>());
        }
        public void SetModelData<T>(T[] a_vertex, uint[] a_index, float a_radius, ModelVertexInfo[] a_vertexInfo) where T : struct
        {
            m_pipeline.AddObject(new ModelData<T>(this, a_vertex, a_index, a_radius, a_vertexInfo));
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