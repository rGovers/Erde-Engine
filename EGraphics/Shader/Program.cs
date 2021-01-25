using Erde.Application;
using Erde.Graphics.Internal.Shader;
using System;

namespace Erde.Graphics.Shader
{
    public class Program : IGraphicsObject
    {
        IProgram        m_internalObject;

        Pipeline        m_pipeline;

        PixelShader     m_pixelShader;
        GeometryShader  m_geometryShader;
        VertexShader    m_vertexShader;

        bool            m_depthTest;

        public PixelShader PixelShader
        {
            get
            {
                return m_pixelShader;
            }
        }

        public GeometryShader GeometryShader
        {
            get
            {
                return m_geometryShader;
            }
        }

        public VertexShader VertexShader
        {
            get
            {
                return m_vertexShader;
            }
        }

        public IProgram InternalObject
        {
            get
            {
                return m_internalObject;
            }
        }        

        public bool DepthTest
        {
            get
            {
                return m_depthTest;
            }
        }

        public bool Initialized
        {
            get
            {
                return m_internalObject.Initialized;
            }
        }

        public Program(VertexShader a_vertexShader, ModelVertexInfo[] a_vertLayout, int a_vertexSize, bool a_depthTest, Pipeline a_pipeline) 
            : this(null, null, a_vertexShader, a_vertLayout, a_vertexSize, a_depthTest, a_pipeline)
        {
        }

        public Program (PixelShader a_pixelShader, VertexShader a_vertexShader, ModelVertexInfo[] a_vertLayout, int a_vertexSize, bool a_depthTest, Pipeline a_pipeline)
            : this(a_pixelShader, null, a_vertexShader, a_vertLayout, a_vertexSize, a_depthTest, a_pipeline)
        {
        }

        public Program (PixelShader a_pixelShader, GeometryShader a_geometryShader, VertexShader a_vertexShader, ModelVertexInfo[] a_vertLayout, int a_vertexSize, bool a_depthTest, Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;
            
            m_pixelShader = a_pixelShader;
            m_geometryShader = a_geometryShader;
            m_vertexShader = a_vertexShader;

            m_depthTest = a_depthTest;

            if (a_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKProgram(this);
            }
            else
            {
                m_internalObject = new NativeProgram(this, a_vertLayout, a_vertexSize, a_pipeline);
            }

            m_pipeline.AddObject(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);

            m_internalObject.Dispose();
        }

        ~Program ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
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