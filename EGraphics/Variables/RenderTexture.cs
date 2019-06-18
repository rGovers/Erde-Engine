using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Variables
{
    public class RenderTexture : IGLObject
    {
        int      m_bufferHandle;

        Texture  m_colorBuffer;
        Texture  m_depthBuffer;

        int      m_width;
        int      m_height;

        Pipeline m_pipeline;

        struct Resize : IGLObject
        {
            private RenderTexture m_renderTexture;

            public void ModifyObject ()
            {
                GL.BindTexture(TextureTarget.Texture2D, m_renderTexture.ColorBuffer.Handle);
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    m_renderTexture.Width, m_renderTexture.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    IntPtr.Zero
                );

                GL.BindTexture(TextureTarget.Texture2D, m_renderTexture.DepthBuffer.Handle);
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.DepthComponent,
                    m_renderTexture.Width, m_renderTexture.Height,
                    0,
                    PixelFormat.DepthComponent,
                    PixelType.UnsignedByte,
                    IntPtr.Zero
                );
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }

            public Resize (RenderTexture a_renderTexture)
            {
                m_renderTexture = a_renderTexture;
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

        public RenderTexture (int a_width, int a_height, PixelFormat a_pixelFormat, PixelInternalFormat a_pixelInternalFormat, Pipeline a_pipeline)
        {
            m_colorBuffer = new Texture(a_width, a_height, a_pixelFormat, a_pixelInternalFormat, a_pipeline);
            m_depthBuffer = new Texture(a_width, a_height, PixelFormat.DepthComponent, PixelInternalFormat.DepthComponent, a_pipeline);

            m_width = a_width;
            m_height = a_height;
            m_pipeline = a_pipeline;

            m_pipeline.InputQueue.Enqueue(this);
        }

        public void ResizeTexture (int a_width, int a_height)
        {
            m_width = a_width;
            m_height = a_height;

            m_pipeline.InputQueue.Enqueue(new Resize(this));
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

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            m_pipeline.DisposalQueue.Enqueue(this);

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