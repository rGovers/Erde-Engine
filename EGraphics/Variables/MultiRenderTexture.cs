using Erde.Graphics.Internal.Variables;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Variables
{
    public class MultiRenderTexture : IGraphicsObject
    {
        class Resize : IGraphicsObject
        {
            int       m_width;
            int       m_height;

            MultiRenderTexture m_renderTexture;

            public Resize (MultiRenderTexture a_mrt, int a_width, int a_height)
            {
                m_renderTexture = a_mrt;

                m_width = a_width;
                m_height = a_height;
            }

            public void ModifyObject ()
            {
                foreach (Texture texture in m_renderTexture.RenderTextures)
                {
                    texture.WriteData(m_width, m_height, e_PixelFormat.RGBA, e_InternalPixelFormat.RGBA, e_PixelType.UnsignedByte, IntPtr.Zero);
                }

                Texture depthBuffer = m_renderTexture.DepthBuffer;

                depthBuffer.WriteData(m_width, m_height, e_PixelFormat.Depth, e_InternalPixelFormat.Depth, e_PixelType.Byte, IntPtr.Zero);
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

        Pipeline  m_pipeline;

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
                return m_depthTexture.Width;
            }
        }

        public int Height
        {
            get
            {
                return m_depthTexture.Height;
            }
        }

        public MultiRenderTexture (int a_buffers, int a_width, int a_height, Pipeline a_pipeline)
        {
            m_pipeline = a_pipeline;

            m_renderTextures = new Texture[a_buffers];
            for (int i = 0; i < a_buffers; ++i)
            {
                m_renderTextures[i] = new Texture(a_width, a_height, e_PixelFormat.RGBA, e_InternalPixelFormat.RGBA, m_pipeline);
            }

            m_depthTexture = new Texture(a_width, a_height, e_PixelFormat.Depth, e_InternalPixelFormat.Depth, m_pipeline);

            m_pipeline.AddObject(this);
        }

        public void ResizeTextures (int a_width, int a_height)
        {
            m_pipeline.AddObject(new Resize(this, a_width, a_height));
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            foreach (Texture texture in m_renderTextures)
            {
                texture.Dispose();
            }

            m_depthTexture.Dispose();

            m_pipeline.RemoveObject(this);
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
                GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext + i, TextureTarget.Texture2D, ((OpenTKTexture)m_renderTextures[i].InternalObject).Handle, 0);
            }

            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, ((OpenTKTexture)m_depthTexture.InternalObject).Handle, 0);
        }

        public void DisposeObject ()
        {
            GL.DeleteFramebuffer(m_bufferHandle);
        }
    }
}