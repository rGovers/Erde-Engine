using Erde.Graphics.Rendering;
using Erde.IO;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Erde.Graphics.Variables
{
    public class CubeMap : IGLObject, IMaterialBindable
    {
        IFileSystem  m_fileSystem;

        string[]     m_filePaths;

        int          m_texture;

        int          m_width;
        int          m_height;

        Pipeline     m_pipeline;

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
        }

        public int Height
        {
            get
            {
                return m_height;
            }
        }

        public CubeMap (string[] a_filePaths, Pipeline a_pipeline) : this(a_filePaths, null, a_pipeline) { }

        public CubeMap (string[] a_filePaths, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            m_filePaths = a_filePaths;
            m_fileSystem = a_fileSystem;
            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~CubeMap ()
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
            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
            GL.Uniform1(a_binding.Handle, a_container.Textures++);
        }

        public void ModifyObject ()
        {
            m_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, m_texture);

            for (int i = 0; i < 6; ++i)
            {
                Bitmap map = null;

                if (m_fileSystem != null)
                {
                    Stream stream;

                    if (m_fileSystem.Load(m_filePaths[i], out stream))
                    {
                        map = new Bitmap(stream);
                    }
                }
                else
                {
                    map = new Bitmap(m_filePaths[i]);
                }

                BitmapData data = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                GL.TexImage2D
                (
                    TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.Rgba,
                    map.Width, map.Height,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0
                );

                m_width = map.Width;
                m_height = map.Height;

                map.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
        }

        public void DisposeObject ()
        {
            GL.DeleteTexture(m_texture);
        }
    }
}