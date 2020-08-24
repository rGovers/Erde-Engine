using Erde.Graphics.Variables;
using Erde.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Xml;

namespace Erde.Graphics.GUI
{
    public class TextBox : Element, IDisposable
    {
        bool                    m_updateString;
        
        Pipeline                m_pipeline;

        Texture                 m_texture;

        string                  m_text;

        Font                    m_font;
        Brush                   m_brush;

        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    m_updateString = true;
                }
            }
        }
        public Font Font
        {
            get
            {
                return m_font;
            }
            set
            {
                if (m_font != value)
                {
                    m_font = value;
                    m_updateString = true;
                }
            }
        }
        public Brush Brush
        {
            get
            {
                return m_brush;
            }
            set
            {
                if (m_brush != value)
                {
                    m_brush = value;
                    m_updateString = true;
                }
            }
        }

        public TextBox (Brush a_brush, Font a_font, int a_width, int a_height, Pipeline a_pipeline) : base()
        {
            Size = new Vector2(a_width, a_height);
            
            m_brush = a_brush;
            m_font = a_font;

            m_pipeline = a_pipeline;

            m_updateString = true;

            m_texture = new Texture(a_width, a_height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelInternalFormat.Rgba, m_pipeline);
        }

        protected static void ExtractData (XmlNode a_node, out string a_text, out string a_fontFamily, out float a_fontSize, out int a_width, out int a_height, out Brush a_brush)
        {
            a_text = string.Empty;
            a_fontFamily = string.Empty;
            a_fontSize = 12;
            a_width = 100;
            a_height = 100;
            a_brush = Brushes.White;

            foreach (XmlAttribute att in a_node.Attributes)
            {
                string attName = att.Name.ToLower();
                switch (attName)
                {
                case "text":
                    {
                        a_text = att.Value;

                        break;
                    }
                case "width":
                    {
                        a_width = int.Parse(att.Value);

                        break;
                    }
                case "height":
                    {
                        a_height = int.Parse(att.Value);

                        break;
                    }
                case "font":
                    {
                        a_fontFamily = att.Value;

                        break;
                    }
                case "fontsize":
                    {
                        a_fontSize = float.Parse(att.Value);

                        break;
                    }
                case "color":
                    {
                        if (att.Value.StartsWith("0x"))
                        {
                            a_brush = new SolidBrush(Color.FromArgb(int.Parse(att.Value.Substring(2), NumberStyles.HexNumber)));
                        }

                        break;
                    }
                }
            }
        }
        protected static Font CompileFontData (string a_fontFamily, float a_fontSize)
        {
            if (!string.IsNullOrEmpty(a_fontFamily))
            {
                InstalledFontCollection fontCollection = new InstalledFontCollection();

                FontFamily[] fontFamilies = fontCollection.Families;

                foreach (FontFamily font in fontFamilies)
                {
                    if (font.Name == a_fontFamily)
                    {
                        return new Font(font, a_fontSize);
                    }
                }
            }

            return new Font(SystemFonts.DefaultFont.FontFamily, a_fontSize);
        }

        internal static TextBox Create (XmlNode a_node, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            string text;
            string fontFamily;
            float fontSize;
            int width;
            int height;

            Brush brush;

            ExtractData(a_node, out text, out fontFamily, out fontSize, out width, out height, out brush);
            Font font = CompileFontData(fontFamily, fontSize);

            return new TextBox(brush, font, width, height, a_pipeline)
            {
                Text = text
            };
        }

        void UpdateString ()
        {
            int width = m_texture.Width;
            int height = m_texture.Height;

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.Clip = new Region(new Rectangle(0, 0, width, height));

            graphics.DrawString(m_text, m_font, m_brush, 0.0f, 0.0f);
            graphics.Flush();

            int handle = m_texture.Handle;

            GL.BindTexture(TextureTarget.Texture2D, handle);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            width, height,
            0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0);

            bitmap.UnlockBits(data);

            m_updateString = false;

            bitmap.Dispose();
            graphics.Dispose();
        }

        internal override void Repaint ()
        {
            UpdateString();
        }
        internal override void Draw (Vector2 a_resolution)
        {
            if (m_updateString)
            {
                UpdateString();
            }

            CalculateTrueTransform();
            Matrix4 transform = ToMatrix(a_resolution);

            GLCommand.Blit(m_texture, transform, true);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_texture.Dispose();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TextBox ()
        {
            Dispose(false);
        }
    }
}