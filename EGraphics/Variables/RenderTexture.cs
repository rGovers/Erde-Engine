using Erde;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Variables
{
    public class RenderTexture : IGraphicsObject
    {
        int      m_bufferHandle;

        Texture  m_colorBuffer;
        Texture  m_depthBuffer;

        Pipeline m_pipeline;

        struct Resize : IGraphicsObject
        {
            RenderTexture m_renderTexture;

            int           m_width;
            int           m_height;

            public void ModifyObject ()
            {
                Texture colorBuffer = m_renderTexture.ColorBuffer;
                GL.BindTexture(TextureTarget.Texture2D, colorBuffer.Handle);
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    m_width, m_height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    IntPtr.Zero
                );
                colorBuffer.Width = m_width;
                colorBuffer.Height = m_height;

                Texture depthBuffer = m_renderTexture.DepthBuffer;
                GL.BindTexture(TextureTarget.Texture2D, depthBuffer.Handle);
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.DepthComponent,
                    m_width, m_height,
                    0,
                    PixelFormat.DepthComponent,
                    PixelType.UnsignedByte,
                    IntPtr.Zero
                );               
                depthBuffer.Width = m_width;
                depthBuffer.Height = m_height;
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }

            public Resize (RenderTexture a_renderTexture, int a_width, int a_height)
            {
                m_renderTexture = a_renderTexture;

                m_width = a_width;
                m_height = a_height;
            }
        }

        internal int RenderBuffer
        {
            get
            {
                return m_bufferHandle;
            }
        }

        public Texture ColorBuffer
        {
            get
            {
                return m_colorBuffer;
            }
        }

        public Texture DepthBuffer
        {
            get
            {
                return m_depthBuffer;
            }
        }

        public int Width
        {
            get
            {
                return m_colorBuffer.Width;
            }
        }

        public int Height
        {
            get
            {
                return m_colorBuffer.Height;
            }
        }

        public RenderTexture (int a_width, int a_height, PixelFormat a_pixelFormat, PixelInternalFormat a_pixelInternalFormat, Pipeline a_pipeline)
        {
            m_colorBuffer = new Texture(a_width, a_height, a_pixelFormat, a_pixelInternalFormat, a_pipeline);
            m_depthBuffer = new Texture(a_width, a_height, PixelFormat.DepthComponent, PixelInternalFormat.DepthComponent, a_pipeline);

            m_pipeline = a_pipeline;

            m_pipeline.AddObject(this);
        }

        public void ResizeTexture (int a_width, int a_height)
        {
            m_pipeline.AddObject(new Resize(this, a_width, a_height));
        }

        public void ModifyObject ()
        {
            m_bufferHandle = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_bufferHandle);
            GL.FramebufferTexture2D
            (
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                m_colorBuffer.Handle,
                0
            );
            GL.FramebufferTexture2D
            (
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                m_depthBuffer.Handle,
                0
            );
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);

            m_colorBuffer.Dispose();
            m_depthBuffer.Dispose();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DisposeObject ()
        {
            GL.DeleteFramebuffer(m_bufferHandle);
        }

        ~RenderTexture ()
        {
            Dispose(false);
        }
    }
}