using Erde.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal.Shader
{
    public class NativeProgram : IProgram
    {
        class ProgramInitializer : IGraphicsObject
        {
            NativePipeline    m_pipeline;
      
            NativeProgram     m_program;

            ModelVertexInfo[] m_vertexLayout;
            int               m_vertexSize;

            public ProgramInitializer(NativeProgram a_program, ModelVertexInfo[] a_vertexLayout, int a_vertexSize, NativePipeline a_pipeline)
            {
                m_pipeline = a_pipeline;

                m_program = a_program;

                m_vertexLayout = a_vertexLayout;
                m_vertexSize = a_vertexSize;
            }

            public void Dispose ()
            {
                
            }

            public void ModifyObject ()
            {
                IntPtr vertexShaderHandle = IntPtr.Zero;
                IntPtr geometryShaderHandle = IntPtr.Zero;
                IntPtr pixelShaderHandle = IntPtr.Zero;

                Program program = m_program.Program;

                VertexShader vertexShader = program.VertexShader;
                GeometryShader geometryShader = program.GeometryShader;
                PixelShader pixelShader = program.PixelShader;

                if (vertexShader != null)
                {
                    vertexShaderHandle = ((NativeVertexShader)vertexShader.InternalObject).Handle;
                }
                if (geometryShader != null)
                {
                    geometryShaderHandle = ((NativeGeometryShader)geometryShader.InternalObject).Handle;
                }
                if (pixelShader != null)
                {
                    pixelShaderHandle = ((NativePixelShader)pixelShader.InternalObject).Handle;
                }

                IntPtr vertexLayout = Marshal.UnsafeAddrOfPinnedArrayElement(m_vertexLayout, 0);

                m_program.Handle = Program_new(vertexShaderHandle, geometryShaderHandle, pixelShaderHandle, vertexLayout, m_vertexLayout.Length, m_vertexSize, m_pipeline.Handle);
            }

            public void DisposeObject ()
            {
                
            }
        }

        Program           m_program;
       
        IntPtr            m_handle;

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

        public Program Program
        {
            get
            {
                return m_program;
            }
        }

        public bool Initialized
        {
            get
            {
                return m_handle != IntPtr.Zero;
            }
        }

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr Program_new(IntPtr a_vertexShader, IntPtr a_geometryShader, IntPtr a_pixelShader, IntPtr a_vertexInfo, int a_vertexInfoCount, int a_vertexSize, IntPtr a_pipeline);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void Program_delete(IntPtr a_ptr);

        public NativeProgram(Program a_program, ModelVertexInfo[] a_vertexLayout, int a_vertexSize, Pipeline a_pipeline)
        {
            m_handle = IntPtr.Zero;

            m_program = a_program;

            a_pipeline.AddObject(new ProgramInitializer(this, a_vertexLayout, a_vertexSize, (NativePipeline)a_pipeline.InternalPipeline));
        }

        public void Dispose ()
        {
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            
        }

        public void DisposeObject ()
        {
            Program_delete(m_handle);
        }
    }
}