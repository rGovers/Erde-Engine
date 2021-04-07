using Erde.Application;
using Erde.Graphics.Internal.Variables;
using Erde.Graphics.Rendering;
using Erde.IO;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Variables
{
    public enum e_PixelFormat : int
    {
        Alpha = 0,
        Red,
        Green,
        Blue,
        Depth,
        RG,
        RGB,
        RGBA,
        BGR,
        BGRA
    }

    public enum e_InternalPixelFormat : int
    {
        Depth = 0,
        Alpha,
        RGB,
        RGBA,
    }

    public enum e_PixelType : int
    {
        Byte = 0,
        UnsignedByte,
        Int,
        UnsignedInt,
        Float
    }

    public class Texture : IGraphicsObject, IMaterialBindable
    {
        class ColorGenerator : IGraphicsObject
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
                int count = 16 * 4;

                float[] colors = new float[count];

                for (int i = 0; i < count; i += 4)
                {
                    colors[i + 0] = m_color.R;
                    colors[i + 1] = m_color.G;
                    colors[i + 2] = m_color.B;
                    colors[i + 3] = m_color.A;
                }

                IntPtr data = Marshal.UnsafeAddrOfPinnedArrayElement(colors, 0);

                m_texture.WriteData(4, 4, e_PixelFormat.RGBA, e_InternalPixelFormat.RGBA, e_PixelType.Float, data);

                m_texture.InternalObject.WriteData(data, e_PixelType.Float);
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }
        }

        class StreamGenerator : IGraphicsObject
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

                int width = bitmap.Width;
                int height = bitmap.Height;

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                m_texture.WriteData(width, height, m_texture.PixelFormat, m_texture.InternalPixelFormat, e_PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);
                bitmap.Dispose();
            }
        }

        class FileGenerator : IGraphicsObject
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
                Stream stream;
                if (m_fileSystem.Load(m_filePath, out stream))
                {
                    Bitmap map = new Bitmap(stream);

                    stream.Dispose();

                    int width = map.Width;
                    int height = map.Height;

                    BitmapData data = map.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    m_texture.WriteData(width, height, e_PixelFormat.BGRA, e_InternalPixelFormat.RGBA, e_PixelType.UnsignedByte, data.Scan0);

                    map.UnlockBits(data);
                    map.Dispose();
                }
            }
        }

        ITexture              m_internalObject;

        int                   m_width;
        int                   m_height;

        Pipeline              m_pipeline;

        e_PixelFormat         m_pixelFormat;
        e_InternalPixelFormat m_pixelInternalFormat;

        public ITexture InternalObject
        {
            get 
            {
                return m_internalObject;
            }
        }

        internal e_PixelFormat PixelFormat
        {
            get
            {
                return m_pixelFormat;
            }
            set
            {
                m_pixelFormat = value;
            }
        }

        internal e_InternalPixelFormat InternalPixelFormat
        {
            get
            {
                return m_pixelInternalFormat;
            }
            set
            {
                m_pixelInternalFormat = value;
            }
        }

        public int Width
        {
            get
            {
                return m_width;
            }
        }

        public int Height
        {
            get
            {
                return m_height;
            }
        }

        public bool Initialized
        {
            get
            {
                return m_internalObject.Initialized;
            }
        }

        Texture (Pipeline a_pipeline)
        {
            m_width = 0;
            m_height = 0;

            m_pixelFormat = e_PixelFormat.BGRA;
            m_pixelInternalFormat = e_InternalPixelFormat.RGBA;

            m_pipeline = a_pipeline;

            if (a_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKTexture(this);
            }

            m_pipeline.AddObject(this);
        }
        public Texture (int a_width, int a_height, e_PixelFormat a_pixelFormat, e_InternalPixelFormat a_pixelInternalFormat, Pipeline a_pipeline)
        {
            m_width = a_width;
            m_height = a_height;

            m_pipeline = a_pipeline;

            m_pixelFormat = a_pixelFormat;
            m_pixelInternalFormat = a_pixelInternalFormat;

            if (a_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKTexture(this);
            }

            m_pipeline.AddObject(this);
        }

        public void ModifyObject ()
        {
            m_internalObject.ModifyObject();
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);
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

        internal void WriteData(int a_width, int a_height, e_PixelFormat a_pixelFormat, e_InternalPixelFormat a_internalPixelFormat, e_PixelType a_pixelType, IntPtr a_data)
        {
            m_width = a_width;
            m_height = a_height;

            m_pixelFormat = a_pixelFormat;
            m_pixelInternalFormat = a_internalPixelFormat;

            m_internalObject.WriteData(a_data, a_pixelType);
        }

        public void Bind (BindableContainer a_container, Binding a_binding)
        {
            m_internalObject.Bind(a_container.Textures++, a_binding.Handle);
        }

        public void DisposeObject ()
        {
            m_internalObject.DisposeObject();
        }

        public static Texture FromColor (Color a_color, Pipeline a_pipeline)
        {
            Texture texture = new Texture(a_pipeline);

            a_pipeline.AddObject(new ColorGenerator(a_color, texture));

            return texture;
        }
        public static Texture FromStream (Stream a_stream, Pipeline a_pipeline)
        {
            Texture texture = new Texture(a_pipeline);

            a_pipeline.AddObject(new StreamGenerator(a_stream, texture));

            return texture;
        }
        public static Texture FromFile (string a_filePath, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            Texture texture = new Texture(a_pipeline);

            a_pipeline.AddObject(new FileGenerator(a_filePath, a_fileSystem, texture));

            return texture;
        }
    }
}