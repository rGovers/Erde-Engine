using Erde;
using Erde.IO;
using Erde.Graphics.Variables;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Erde.Graphics.IO
{
    public class OBJLoader
    {
        List<Vertex> m_vertices;
        List<uint> m_indices;

        float        m_length;

        public Vertex[] Vertices
        {
            get
            {
                return m_vertices.ToArray();
            }
        }
        public uint[] Indices
        {
            get
            {
                return m_indices.ToArray();
            }
        }

        public float Length
        {
            get
            {
                return m_length;
            }
        }

        public OBJLoader(string a_fileName, IFileSystem a_fileSystem)
        {
            m_vertices = new List<Vertex>();
            m_indices = new List<uint>();

            byte[] bytes;
            if (a_fileSystem.Load(a_fileName, out bytes))
            {
                string[] lines = Encoding.UTF8.GetString(bytes).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                List<Vector3> vertexPosition = new List<Vector3>();
                List<Vector3> vertexNormal = new List<Vector3>();
                List<Vector2> vertexTextureCoords = new List<Vector2>();

                Dictionary<Vertex, uint> vertexLookup = new Dictionary<Vertex, uint>();

                float lengthSqr = 0.0f;

                foreach (string line in lines)
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

                                uint val = 0;

                                if (!vertexLookup.TryGetValue(vert, out val))
                                {
                                    m_vertices.Add(vert);
                                    val = (ushort)(m_vertices.Count - 1);
                                    vertexLookup.Add(vert, val);
                                }

                                m_indices.Add(val);
                            }

                            break;
                        }
                    }
                }

                m_length = (float)Math.Sqrt(lengthSqr);
            }
            else
            {
                InternalConsole.Warning("Failed to Load: " + a_fileName);
            }
        }
    }
}