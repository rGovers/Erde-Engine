using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Shader
{
    public class VertexShader : IGLObject
    {
        Pipeline m_pipeline;

        int      m_shader;
        string   m_source;

        public VertexShader (string a_source, Pipeline a_pipeline)
        {
            m_source = a_source;

            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }

        // public VertexShader (string a_filename, AssetManager a_assetManager, Pipeline a_pipeline) : this(Program.LoadSource(a_filename, a_assetManager), a_pipeline) { }

        void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~VertexShader ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Source
        {
            get
            {
                return m_source;
            }
        }

        internal int Handle
        {
            get
            {
                return m_shader;
            }
        }

        public void ModifyObject ()
        {
            m_shader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(m_shader, m_source);
            GL.CompileShader(m_shader);
#if DEBUG_INFO
            string info = GL.GetShaderInfoLog(m_shader);

            if (info != string.Empty)
            {
                InternalConsole.AddMessage(info, InternalConsole.e_Alert.Warning);
            }
#endif
        }

        public void DisposeObject ()
        {
            GL.DeleteShader(m_shader);
        }
    }
}