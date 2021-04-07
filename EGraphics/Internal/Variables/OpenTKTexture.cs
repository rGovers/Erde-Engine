using Erde.Graphics.Variables;
using OpenTK.Graphics.OpenGL;
using System;

namespace Erde.Graphics.Internal.Variables
{
    class OpenTKTexture : ITexture
    {
        int     m_handle;

        Texture m_texture;

        public int Handle
        {
            get 
            {
                return m_handle;
            }
        }

        public bool Initialized
        {
            get 
            {
                return m_handle != -1;
            }
        }

        public OpenTKTexture(Texture a_texture)
        {
            m_handle = -1;

            m_texture = a_texture;
        }

        OpenTK.Graphics.OpenGL.PixelFormat GetOpenTKPixelFormat(e_PixelFormat a_pixelFormat)
        {
            switch (a_pixelFormat)
            {
                case e_PixelFormat.Alpha:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Alpha;
                }
                case e_PixelFormat.Red:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Red;
                }
                case e_PixelFormat.Green:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Green;
                }
                case e_PixelFormat.Blue:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Blue;
                }
                case e_PixelFormat.Depth:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent;
                }
                case e_PixelFormat.RG:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Rg;
                }
                case e_PixelFormat.RGB:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Rgb;
                }
                case e_PixelFormat.RGBA:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                }
                case e_PixelFormat.BGR:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
                }
                case e_PixelFormat.BGRA:
                {
                    return OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                }
            }

            return OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
        }

        PixelInternalFormat GetOpenTKInternalPixelFormat(e_InternalPixelFormat a_pixelFormat)
        {
            switch (a_pixelFormat)
            {
                case e_InternalPixelFormat.Depth:
                {
                    return PixelInternalFormat.DepthComponent;
                }
                case e_InternalPixelFormat.Alpha:
                {
                    return PixelInternalFormat.Alpha;
                }
                case e_InternalPixelFormat.RGB:
                {
                    return PixelInternalFormat.Rgb;
                }
                case e_InternalPixelFormat.RGBA:
                {
                    return PixelInternalFormat.Rgba;
                }
            }

            return PixelInternalFormat.Rgba;
        }

        PixelType GetOpenTKPixelType(e_PixelType a_pixelType)
        {
            switch (a_pixelType)
            {
                case e_PixelType.Byte:
                {
                    return PixelType.Byte;
                }
                case e_PixelType.UnsignedByte:
                {
                    return PixelType.UnsignedByte;
                }
                case e_PixelType.Int:
                {
                    return PixelType.Int;
                }
                case e_PixelType.UnsignedInt:
                {
                    return PixelType.UnsignedInt;
                }
                case e_PixelType.Float:
                {
                    return PixelType.Float;
                }
            }

            return PixelType.UnsignedByte;
        }

        public void WriteData(IntPtr a_ptr, e_PixelType a_pixelType)
        {
            if (m_handle == -1)
            {
                m_handle = GL.GenTexture();
            }

            int width = m_texture.Width;
            int height = m_texture.Height;

            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat = GetOpenTKPixelFormat(m_texture.PixelFormat);
            PixelInternalFormat internalFormat = GetOpenTKInternalPixelFormat(m_texture.InternalPixelFormat);
            PixelType pixelType = GetOpenTKPixelType(a_pixelType);

            GL.BindTexture(TextureTarget.Texture2D, m_handle);

            GL.TexImage2D
            (
                TextureTarget.Texture2D,
                0,
                internalFormat,
                width, height,
                0,
                pixelFormat,
                pixelType,
                a_ptr
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

#if DEBUG_INFO
            Pipeline.GLError("Texture: Writing: ");
#endif
        }

        public void Dispose ()
        {
            GC.SuppressFinalize(this);
        }

        public void ModifyObject()
        {
            WriteData(IntPtr.Zero, e_PixelType.UnsignedByte);
        }

        public void DisposeObject()
        {
            if (m_handle != -1)
            {
                GL.DeleteTexture(m_handle);
            }
        }

        public void Bind(int a_slot, int a_index)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + a_slot);
            GL.BindTexture(TextureTarget.Texture2D, m_handle);
            GL.Uniform1(a_index, a_slot);

#if DEBUG_INFO
            Pipeline.GLError("Texture: Binding: ");
#endif
        }
    }
}