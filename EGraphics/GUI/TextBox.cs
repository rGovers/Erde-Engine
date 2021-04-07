using Erde;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using Erde.IO;
using OpenTK;
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

            m_texture = new Texture(a_width, a_height, e_PixelFormat.BGRA, e_InternalPixelFormat.RGBA, m_pipeline);
        }

        protected static void ExtractData (XmlNode a_node, out string a_text, out string a_fontFamily, out float a_fontSize, out int a_width, out int a_height, out Brush a_brush)
        {
            a_text = string.Empty;
            a_fontFamily = string.Empty;
            a_fontSize = 12;
            a_width = 100;
            a_height = 100;
            a_brush = Brushes.White;

            string translationTag = null;
            string fontTag = null;

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
                case "translationtag":
                    {
                        translationTag = att.Value;

                        break;
                    }
                case "fonttag":
                    {
                        fontTag = att.Value;

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

            if (!string.IsNullOrEmpty(translationTag))
            {
                string text = translationTag.Translate();

                if (!string.IsNullOrEmpty(text))
                {
                    a_text = text;
                }
            }
            if (!string.IsNullOrEmpty(fontTag))
            {
                string text = Translation.GetFontName(fontTag);

                if (!string.IsNullOrEmpty(text))
                {
                    a_fontFamily = text;
                }
            }
        }
        protected static Font CompileFontData (string a_fontFamily, float a_fontSize)
        {
            if (!string.IsNullOrEmpty(a_fontFamily))
            {
                FontFamily fontFamily = FontCollection.GetFontFamily(a_fontFamily);

                if (fontFamily != null)
                {
                    return new Font(fontFamily, a_fontSize, GraphicsUnit.Pixel);
                }

                InstalledFontCollection fontCollection = new InstalledFontCollection();

                FontFamily[] fontFamilies = fontCollection.Families;

                foreach (FontFamily font in fontFamilies)
                {
                    if (font.Name == a_fontFamily)
                    {
                        return new Font(font, a_fontSize, GraphicsUnit.Pixel);
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
            if (!m_texture.Initialized)
            {
                return;
            }

            int width = m_texture.Width;
            int height = m_texture.Height;

            Rectangle rect = new Rectangle(0, 0, width, height);

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.Clip = new Region(rect);

            graphics.DrawString(m_text, m_font, m_brush, 0.0f, 0.0f);
            graphics.Flush();

            BitmapData data = bitmap.LockBits(rect,
            ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GraphicsCommand.UpdateTextureRGBA(m_texture, data.Scan0);

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

            Program program = Shaders.TRANSFORM_IMAGE_SHADER_INVERTED;

            GraphicsCommand.BindProgram(program);

            GraphicsCommand.BindMatrix4(program, 0, transform);
            GraphicsCommand.BindTexture(program, 1, m_texture, 0);

            GraphicsCommand.Draw();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_texture.Dispose();
        }

        public virtual void Dispose ()
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