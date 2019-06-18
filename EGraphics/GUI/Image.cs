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
                        image.Texture = new Texture(att.Value, a_fileSystem, a_pipeline);

                        break;
                    }
                }
            }

            return image;
        }

        internal override void Draw (Vector2 a_resolution)
        {
            if (m_texture != null )
            {
                CalculateTrueTransform();
                Matrix4 transform = ToMatrix(a_resolution);

                GLCommand.Blit(m_texture, transform, true);
            }
        }

        void Dispose (bool a_state)
        {
            Tools.Verify(this, a_state);

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