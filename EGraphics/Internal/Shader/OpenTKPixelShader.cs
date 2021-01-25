using Erde.Graphics.Shader;
using OpenTK.Graphics.OpenGL;
using System;

namespace Erde.Graphics.Internal.Shader
{
    internal class OpenTKPixelShader : IGraphicsObject
    {
        class PixelShaderCompiler : IGraphicsObject
        {
            OpenTKPixelShader m_shader;

            string            m_source;

            public PixelShaderCompiler(string a_source, OpenTKPixelShader a_shader)
            {
                m_shader = a_shader;

                m_source = a_source;
            }

            public void ModifyObject()
            {
                int shader = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(shader, m_source);
                GL.CompileShader(shader);

                m_shader.Handle = shader;
#if DEBUG_INFO
                string info = GL.GetShaderInfoLog(shader);

                if (info != string.Empty)
                {
                    InternalConsole.AddMessage(info, InternalConsole.e_Alert.Warning);
                }
#endif
            }

            public void DisposeObject()
            {

            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }

        int          m_shader;
        
        internal int Handle
        {
            get
            {
                return m_shader;
            }
            set
            {
                m_shader = value;
            }
        }

        public OpenTKPixelShader(string a_source, Pipeline a_pipeline)
        {
            a_pipeline.AddObject(new PixelShaderCompiler(a_source, this));
        }

        public void ModifyObject()
        {

        }

        public void DisposeObject()
        {
            GL.DeleteShader(m_shader);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}