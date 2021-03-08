using Erde;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Erde.Graphics
{
    public class GizmoRenderer : IDisposable
    {
        Model             m_model;
        Program           m_program;

        ModelVertexInfo[] m_vertexInfo;

        public GizmoRenderer(Pipeline a_pipeline)
        {
            m_model = new Model(a_pipeline);

            PixelShader pixelShader = new PixelShader(Shaders.GIZMO_PIXEL, a_pipeline);
            VertexShader vertexShader = new VertexShader(Shaders.GIZMO_VERTEX, a_pipeline);

            m_vertexInfo = ModelVertexInfo.GetVertexInfo<GizmoVertex>();

            m_program = new Program(pixelShader, vertexShader, m_vertexInfo, Marshal.SizeOf<GizmoVertex>(), false, e_CullingMode.None, a_pipeline);

            pixelShader.Dispose();
            vertexShader.Dispose(); 
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif    
            m_model.Dispose();
            m_program.Dispose();
        }

        ~GizmoRenderer ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        public void Update()
        {
            GizmoVertex[] dirtyVerts = Gizmos.GetVertices();

            if (dirtyVerts != null)
            {
                int count = dirtyVerts.Length;
                if (count > 0)
                {
                    List<GizmoVertex> vertices = new List<GizmoVertex>();
                    List<uint> indices = new List<uint>();
                    Dictionary<GizmoVertex, uint> vertexLookup = new Dictionary<GizmoVertex, uint>();

                    for (int i = 0; i < count; ++i)
                    {
                        GizmoVertex vert = dirtyVerts[i];

                        if (vertexLookup.ContainsKey(vert))
                        {
                            indices.Add(vertexLookup[vert]);

                            continue;
                        }

                        uint index = (uint)vertices.Count;
                        vertices.Add(vert);
                        indices.Add(index);
                        vertexLookup.Add(vert, index);
                    }

                    m_model.SetData(vertices.ToArray(), indices.ToArray(), 0.0f, m_vertexInfo);
                    m_model.Bind();

                    GraphicsCommand.BindProgram(m_program);

                    GraphicsCommand.DrawElementsUInt(m_model.Indices);
                }
            }
        }
    }
}