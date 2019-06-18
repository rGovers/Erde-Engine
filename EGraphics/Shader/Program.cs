using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Erde.Graphics.Shader
{
    public class Program : IGLObject
    {
        int            m_shader = -1;

        Pipeline       m_pipeline;

        PixelShader    m_pixelShader;
        GeometryShader m_geometryShader;
        VertexShader   m_vertexShader;

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

        internal int Handle
        {
            get
            {
                return m_shader;
            }
        }

        public Program (PixelShader a_pixelShader, VertexShader a_vertexShader, Pipeline a_pipeline)
            : this(a_pixelShader, null, a_vertexShader, a_pipeline)
        {
        }

        public Program (PixelShader a_pixelShader, GeometryShader a_geometryShader, VertexShader a_vertexShader, Pipeline a_pipeline)
        {
            m_pixelShader = a_pixelShader;
            m_geometryShader = a_geometryShader;
            m_vertexShader = a_vertexShader;

            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            m_pipeline.DisposalQueue.Enqueue(this);
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

        //public static string LoadSource (string a_fileName)
        //{
        //    FileStream stream = File.Open(a_fileName, FileMode.Open);

        //    byte[] bytes = new byte[stream.Length];

        //    stream.Read(bytes, 0, bytes.Length);

        //    stream.Close();

        //    return Encoding.UTF8.GetString(bytes);
        //}

        //internal static string LoadSource (string a_fileName, AssetManager a_assetManager)
        //{
        //    Stream stream = a_assetManager.GetAssetFile(a_fileName);
        //    byte[] bytes = new byte[4096];
        //    stream.Read(bytes, 0, bytes.Length);

        //    return Encoding.UTF8.GetString(bytes);
        //}

        public void ModifyObject ()
        {
            m_shader = GL.CreateProgram();

            if (m_pixelShader != null)
            {
                GL.AttachShader(m_shader, m_pixelShader.Handle);
            }
            if (m_geometryShader != null)
            {
                GL.AttachShader(m_shader, m_geometryShader.Handle);
            }
            if (m_vertexShader != null)
            {
                GL.AttachShader(m_shader, m_vertexShader.Handle);
            }

            GL.LinkProgram(m_shader);

#if DEBUG_INFO
            string info;
            GL.GetProgramInfoLog(m_shader, out info);

            if (!string.IsNullOrEmpty(info))
            {
                InternalConsole.AddMessage(info, InternalConsole.e_Alert.Warning);
            }
#endif
        }

        public void DisposeObject ()
        {
            GL.DeleteProgram(m_shader);
            m_shader = -1;
        }
    }
}