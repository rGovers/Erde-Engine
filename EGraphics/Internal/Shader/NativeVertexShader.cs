using Erde.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Shader
{
    internal class NativeVertexShader : IGraphicsObject
    {
        class VertexShaderCompiler : IGraphicsObject
        {
            NativeVertexShader m_shader;

            string             m_source;

            public VertexShaderCompiler(string a_source, NativeVertexShader a_shader)
            {
                m_shader = a_shader;

                m_source = a_source;
            }

            public void ModifyObject()
            {
                m_shader.Handle = VertexShader_new(m_source, m_shader.Pipeline.Handle);
            }

            public void DisposeObject()
            {
                
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }

        IntPtr         m_handle;

        NativePipeline m_pipeline;

        internal IntPtr Handle
        {
            get
            {
                return m_handle;
            }
            set
            {
                m_handle = value;
            }
        }

        internal NativePipeline Pipeline
        {
            get
            {
                return m_pipeline;
            }
        }

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr VertexShader_new(string a_source, IntPtr a_pipeline);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void VertexShader_delete(IntPtr a_ptr);

        public NativeVertexShader(string a_source, Pipeline a_pipeline)
        {
            m_pipeline = (NativePipeline)a_pipeline.InternalPipeline;

            a_pipeline.AddObject(new VertexShaderCompiler(a_source, this));
        }   

        public void ModifyObject()
        {
            
        }

        public void DisposeObject()
        {
            VertexShader_delete(m_handle);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}