using Erde.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Shader
{
    internal class NativePixelShader : IGraphicsObject
    {
        class PixelShaderCompiler : IGraphicsObject
        {
            NativePixelShader m_shader;

            string            m_source;

            public PixelShaderCompiler(string a_source, NativePixelShader a_shader)
            {
                m_shader = a_shader;

                m_source = a_source;
            }

            public void ModifyObject()
            {
                m_shader.Handle = PixelShader_new(m_source, m_shader.Pipeline.Handle);
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
        static extern IntPtr PixelShader_new(string a_source, IntPtr a_pipeline);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void PixelShader_delete(IntPtr a_ptr);

        public NativePixelShader(string a_source, Pipeline a_pipeline)
        {
            m_pipeline = (NativePipeline)a_pipeline.InternalPipeline;

            a_pipeline.AddObject(new PixelShaderCompiler(a_source, this));
        }   

        public void ModifyObject()
        {
            
        }

        public void DisposeObject()
        {
            PixelShader_delete(m_handle);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}