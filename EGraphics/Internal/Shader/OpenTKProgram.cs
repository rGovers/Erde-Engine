using Erde.Graphics;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK.Graphics.OpenGL;
using System;

namespace Erde.Graphics.Internal.Shader
{
    public class OpenTKProgram : IProgram
    {
        Program        m_program;

        int            m_handle;

        internal int Handle
        {
            get
            {
                return m_handle;
            }
        }

        public bool Initialized
        {
            get
            {
                return m_handle != -1;
            }
        }

        public OpenTKProgram(Program a_program)
        {
            m_program = a_program;

            m_handle = -1;
        }

        public void Dispose ()
        {
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            m_handle = GL.CreateProgram();

            PixelShader pixelShader = m_program.PixelShader;
            GeometryShader geometryShader = m_program.GeometryShader;
            VertexShader vertexShader = m_program.VertexShader;

            if (pixelShader != null)
            {
                GL.AttachShader(m_handle, ((OpenTKPixelShader)pixelShader.InternalObject).Handle);
            }
            if (geometryShader != null)
            {
                GL.AttachShader(m_handle, ((OpenTKGeometryShader)geometryShader.InternalObject).Handle);
            }
            if (vertexShader != null)
            {
                GL.AttachShader(m_handle, ((OpenTKVertexShader)vertexShader.InternalObject).Handle);
            }

            GL.LinkProgram(m_handle);

#if DEBUG_INFO
            string info;
            GL.GetProgramInfoLog(m_handle, out info);

            if (!string.IsNullOrEmpty(info))
            {
                InternalConsole.AddMessage(info, InternalConsole.e_Alert.Warning);
            }
#endif
        }

        public void DisposeObject ()
        {
            GL.DeleteProgram(m_handle);
        }
    }
}