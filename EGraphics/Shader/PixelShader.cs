using Erde.Application;
using Erde.Graphics.Internal.Shader;
using System;

namespace Erde.Graphics.Shader
{
    public class PixelShader : IGraphicsObject
    {
        IGraphicsObject m_internalObject;

        Pipeline        m_pipeline;

        public IGraphicsObject InternalObject
        {
            get
            {
                return m_internalObject;
            }
        }

        public PixelShader (string a_source, Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;
            
            if (m_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKPixelShader(a_source, m_pipeline);
            }
            else
            {
                m_internalObject = new NativePixelShader(a_source, m_pipeline);
            }

            m_pipeline.AddObject(this);
        }

        public void ModifyObject ()
        {
            m_internalObject.ModifyObject();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);

            m_internalObject.Dispose();
        }

        ~PixelShader ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void DisposeObject ()
        {
            m_internalObject.DisposeObject();
        }
    }
}