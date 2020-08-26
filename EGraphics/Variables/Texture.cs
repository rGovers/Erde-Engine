using Erde.Graphics.Rendering;
using Erde.IO;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Erde.Graphics.Variables
{
    public class Texture : IGLObject, IMaterialBindable
    {
        class ColorGenerator : IGLObject
        {
            Color   m_color;
            Texture m_texture;

            internal ColorGenerator (Color a_color, Texture a_texture)
            {
                m_color = a_color;
                m_texture = a_texture;
            }

            public void ModifyObject ()
            {
                float[] colors = new float[16 * sizeof(float)];

                for (int i = 0; i < colors.Length; i += 4)
                {
                    colors[i + 0] = m_color.R;
                    colors[i + 1] = m_color.G;
                    colors[i + 2] = m_color.B;
                    colors[i + 3] = m_color.A;
                }

                int handle = m_texture.Handle;

                GL.BindTexture(TextureTarget.Texture2D, handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 4, 4, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, colors);
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }
        }

        class StreamGenerator : IGLObject
        {
            Stream  m_stream;
            Texture m_texture;

            internal StreamGenerator (Stream a_stream, Texture a_texture)
            {
                m_stream = a_stream;
                m_texture = a_texture;
            }

            public void Dispose ()
            {

            }
            public void DisposeObject ()
            {

            }

            public void ModifyObject ()
            {
                Bitmap bitmap = new Bitmap(m_stream);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                GL.BindTexture(TextureTarget.Texture2D, m_texture.Handle);
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    m_texture.m_pixelInternalFormat,
                    bitmap.Width, bitmap.Height,
                    0,
                    m_texture.m_pixelFormat,
                    PixelType.UnsignedByte,
                    data.Scan0
                );

                bitmap.UnlockBits(data);

                m_texture.Width = bitmap.Width;
                m_texture.Height = bitmap.Height;
            }
        }

        class FileGenerator : IGLObject
        {
            IFileSystem m_fileSystem;
            string      m_filePath;

            Texture     m_texture;

            internal FileGenerator (string a_filePath, IFileSystem a_fileSystem, Texture a_texture)
            {
                m_filePath = a_filePath;
                m_fileSystem = a_fileSystem;

                m_texture = a_texture;
            }

            public void Dispose ()
            {

            }

            public void DisposeObject ()
            {

            }

            public void ModifyObject ()
            {
                Bitmap map = null;
                Stream stream;
                if (m_fileSystem.Load(m_filePath, out stream))
                {
                    map = new Bitmap(stream);
                }

                BitmapData data = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                m_texture.m_pixelInternalFormat = PixelInternalFormat.Rgba;
                m_texture.m_pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

                GL.BindTexture(TextureTarget.Texture2D, m_texture.m_texture);

                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    map.Width, map.Height,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0
                );

                m_texture.m_width = map.Width;
                m_texture.m_height = map.Height;

                map.UnlockBits(data);
                map.Dispose();
            }
        }

        int                                m_texture;

        int                                m_width;
        int                                m_height;

        Pipeline                           m_pipeline;

        OpenTK.Graphics.OpenGL.PixelFormat m_pixelFormat;
        PixelInternalFormat                m_pixelInternalFormat;

        internal int Handle
        {
            get
            {
                return m_texture;
            }
        }

        public int Width
        {
            get
            {
                return m_width;
            }
            internal set
            {
                m_width = value;
            }
        }

        public int Height
        {
            get
            {
                return m_height;
            }
            internal set
            {
                m_height = value;
            }
        }

        public void ModifyObject ()
        {
            m_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_texture);

            GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    m_pixelInternalFormat,
                    m_width, m_height,
                    0,
                    m_pixelFormat,
                    PixelType.UnsignedByte,
                    IntPtr.Zero
                );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~Texture ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Bind (BindableContainer a_container, Material.Binding a_binding)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + a_container.Textures);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.Uniform1(a_binding.Handle, a_container.Textures++);
        }

        public void DisposeObject ()
        {
            GL.DeleteTexture(m_texture);
        }

        public static Texture FromColor (Color a_color, Pipeline a_pipeline)
        {
            Texture texture = new Texture(a_pipeline);

            a_pipeline.InputQueue.Enqueue(new ColorGenerator(a_color, texture));

            return texture;
        }
        public static Texture FromStream (Stream a_stream, Pipeline a_pipeline)
        {
            Texture texture = new Texture(a_pipeline);

            a_pipeline.InputQueue.Enqueue(new StreamGenerator(a_stream, texture));

            return texture;
        }
        public static Texture FromFile (string a_filePath, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            Texture texture = new Texture(a_pipeline);

            a_pipeline.InputQueue.Enqueue(new FileGenerator(a_filePath, a_fileSystem, texture));

            return texture;
        }

        Texture (Pipeline a_pipeline)
        {
            m_width = 0;
            m_height = 0;

            m_pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
            m_pixelInternalFormat = PixelInternalFormat.Rgba;

            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }
        public Texture (int a_width, int a_height, OpenTK.Graphics.OpenGL.PixelFormat a_pixelFormat, PixelInternalFormat a_pixelInternalFormat, Pipeline a_pipeline)
        {
            m_width = a_width;
            m_height = a_height;

            m_pipeline = a_pipeline;

            m_pixelFormat = a_pixelFormat;
            m_pixelInternalFormat = a_pixelInternalFormat;

            m_pipeline.InputQueue.Enqueue(this);
        }
    }
}