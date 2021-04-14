using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using Erde.IO;
using OpenTK;
using System;
using System.Xml;

namespace Erde.Graphics.GUI
{
    public class Image : Element, IDisposable
    {
        Texture m_texture;

        public Texture Texture
        {
            get
            {
                return m_texture;
            }
            set
            {
                m_texture = value;
            }
        }

        public Image (Texture a_texture, Pipeline a_pipeline) : base()
        {
            m_texture = a_texture;
        }

        internal static Image Create (XmlNode a_node, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            Image image = new Image(null, a_pipeline);

            foreach (XmlAttribute att in a_node.Attributes)
            {
                switch (att.Name.ToLower())
                {
                case "path":
                {
                    image.Texture = Texture.FromFile(att.Value, a_fileSystem, a_pipeline);

                    break;
                }
                }
            }

            return image;
        }

        internal override Vector2 Draw (Vector2 a_resolution, Vector2 a_trueResolution)
        {
            if (m_texture != null && m_texture.Initialized)
            {
                CalculateTrueTransform();
                Matrix4 transform = ToMatrix(a_resolution, a_trueResolution);

                Program program = Shaders.TRANSFORM_IMAGE_SHADER_INVERTED;

                GraphicsCommand.BindProgram(program);

                GraphicsCommand.BindMatrix4(program, 0, transform);
                GraphicsCommand.BindTexture(program, 1, m_texture, 0);

                GraphicsCommand.Draw();
            }

            return a_resolution;
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            if (m_texture != null)
            {
                m_texture.Dispose();
            }
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Image ()
        {
            Dispose(false);
        }
    }
}