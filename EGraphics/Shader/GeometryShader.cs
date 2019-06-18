using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Shader
{
    public class GeometryShader : IGLObject
    {
        int      m_shader;
        string   m_source;

        Pipeline m_pipeline;

        internal int Handle
        {
            get
            {
                return m_shader;
            }
        }

        public string Source
        {
            get
            {
                return m_source;
            }
        }

        public GeometryShader (string a_source, Pipeline a_pipeline)
        {
            m_source = a_source;

            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }

        // public GeometryShader (string a_filename, AssetManager a_assetManager, Pipeline a_pipeline) : this(Program.LoadSource(a_filename, a_assetManager), a_pipeline) { }

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            // Queues the destruction of the geometry shader on the GPU
            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~GeometryShader ()
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
            m_shader = GL.CreateShader(ShaderType.GeometryShader);
            // Sets the source code for the shader to compile from
            GL.ShaderSource(m_shader, m_source);
            // Compiles the shader from the source code
            GL.CompileShader(m_shader);

#if DEBUG_INFO
            // Error checking
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