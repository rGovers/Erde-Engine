using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Variables
{
    public class MultiRenderTexture : IGLObject
    {
        struct Resize : IGLObject
        {
            MultiRenderTexture m_renderTexture;

            public Resize (MultiRenderTexture a_mrt)
            {
                m_renderTexture = a_mrt;
            }

            public void ModifyObject ()
            {
                int width = m_renderTexture.m_width;
                int height = m_renderTexture.m_height;

                foreach (Texture texture in m_renderTexture.RenderTextures)
                {
                    GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.Byte, IntPtr.Zero);

                    texture.Width = width;
                    texture.Height = height;
                }

                GL.BindTexture(TextureTarget.Texture2D, m_renderTexture.DepthBuffer.Handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Byte, IntPtr.Zero);
                m_renderTexture.DepthBuffer.Width = width;
                m_renderTexture.DepthBuffer.Height = height;
            }

            public void DisposeObject ()
            {
            }

            public void Dispose ()
            {
            }
        }

        int       m_bufferHandle;

        Texture[] m_renderTextures;
        Texture   m_depthTexture;

        int       m_width;
        int       m_height;

        public int BufferHandle
        {
            get
            {
                return m_bufferHandle;
            }
        }

        public Texture[] RenderTextures
        {
            get
            {
                return m_renderTextures;
            }
        }

        public Texture DepthBuffer
        {
            get
            {
                return m_depthTexture;
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

        private Pipeline m_pipeline;

        public MultiRenderTexture (int a_buffers, int a_width, int a_height, Pipeline a_pipeline)
        {
            m_width = a_width;
            m_height = a_height;

            m_pipeline = a_pipeline;

            m_renderTextures = new Texture[a_buffers];
            for (int i = 0; i < a_buffers; ++i)
            {
                m_renderTextures[i] = new Texture(a_width, a_height, PixelFormat.Rgba, PixelInternalFormat.Rgba8, m_pipeline);
            }

            m_depthTexture = new Texture(a_width, a_height, PixelFormat.DepthComponent, PixelInternalFormat.DepthComponent, m_pipeline);

            m_pipeline.InputQueue.Enqueue(this);
        }

        public void ResizeTextures (int a_width, int a_height)
        {
            m_width = a_width;
            m_height = a_height;
            
            m_pipeline.InputQueue.Enqueue(new Resize(this));
        }

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            foreach (Texture texture in m_renderTextures)
            {
                texture.Dispose();
            }

            m_depthTexture.Dispose();

            m_pipeline.InputQueue.Enqueue(this);
        }

        ~MultiRenderTexture ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            m_bufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, m_bufferHandle);
            
            for (int i = 0; i < m_renderTextures.Length; ++i)
            {
                GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext + i, TextureTarget.Texture2D, m_renderTextures[i].Handle, 0);
            }

            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, m_depthTexture.Handle, 0);
        }

        public void DisposeObject ()
        {
            GL.DeleteFramebuffer(m_bufferHandle);
        }
    }
}