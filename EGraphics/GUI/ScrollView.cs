using Erde.IO;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using System.Drawing;
using System.Xml;

namespace Erde.Graphics.GUI
{
    public class ScrollView : Element
    {
        Vector2       m_scrollPos;

        int           m_width;
        int           m_height;
    
        int           m_internalWidth;
        int           m_internalHeight;

        public Vector2 ScrollPos
        {
            get
            {
                return m_scrollPos;
            }
            set
            {
                m_scrollPos = value;
            }
        }

        public ScrollView(int a_width, int a_height, int a_internalWidth, int a_internalHeight, Pipeline a_pipeline)
        {
            m_scrollPos = Vector2.Zero;

            m_width = a_width;
            m_height = a_height;

            m_internalWidth = a_internalWidth;
            m_internalHeight = a_internalHeight;

            Size = new Vector2(m_width, m_height);
        }

        public void Resize(int a_width, int a_height, int a_internalWidth, int a_internalHeight)
        {
            m_width = a_width;
            m_height = a_height;

            m_internalWidth = a_internalWidth;
            m_internalHeight = a_internalHeight;
        }

        internal static ScrollView Create (XmlNode a_node, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            int width = 100;
            int height = 100;
            int internalWidth = 100;
            int internalHeight = 100;

            foreach (XmlAttribute att in a_node.Attributes)
            {
                switch (att.Name.ToLower())
                {
                case "width":
                {
                    width = int.Parse(att.Value);

                    break;
                }
                case "height":
                {
                    height = int.Parse(att.Value);

                    break;
                }
                case "internalwidth":
                {
                    internalWidth = int.Parse(att.Value);

                    break;
                }
                case "internalheight":
                {
                    internalHeight = int.Parse(att.Value);

                    break;
                }
                }
            }

            return new ScrollView(width, height, internalWidth, internalHeight, a_pipeline);
        }

        internal override Rectangle GetActiveRect(Vector2 a_resolution)
        {
            Element parent = Parent;

            Vector2 trueSize = GetTrueSize();
            Vector2 truePos = GetTruePosition();
            Vector2 tPos = (truePos + a_resolution) * 0.5f;
            Vector2 halfSize = trueSize * 0.5f;

            return new Rectangle((int)tPos.X, (int)tPos.Y, (int)halfSize.X, (int)halfSize.Y);
        }

        internal override Vector2 Draw(Vector2 a_resolution, Vector2 a_trueResolution)
        {
            CalculateTrueTransform();

            int widthDiff = m_internalWidth - m_width;
            int heightDiff = m_internalHeight - m_height;

            TrueDrawingPosition = new Vector2(m_scrollPos.X * widthDiff, m_scrollPos.Y * heightDiff);
            TruePosition = new Vector2(TruePosition.X - m_scrollPos.X * widthDiff, TruePosition.Y + m_scrollPos.Y * heightDiff);

            GraphicsCommand.SetViewport(GetActiveRect(a_resolution));

            return TrueSize;
        }

        internal override void PostDraw(Vector2 a_resolution, Vector2 a_trueResolution)
        {
            GraphicsCommand.SetViewport(new Rectangle(0, 0, (int)a_resolution.X, (int)a_resolution.Y));
        }
    }
}