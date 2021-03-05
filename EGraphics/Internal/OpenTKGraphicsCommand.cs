using Erde.Graphics;
using Erde.Graphics.Internal.Shader;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace Erde.Graphics.Internal
{
    public class OpenTKGraphicsCommand : IGraphicsCommand
    {
        OpenTKPipeline m_pipeline;

        public OpenTKGraphicsCommand(Pipeline a_pipeline)
        {
            m_pipeline = (OpenTKPipeline)a_pipeline.InternalPipeline;
        }

        public void SetViewport(Rectangle a_rect)
        {
            GL.Viewport(a_rect);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Set Viewport: ");
#endif
        }

        public void BindProgram(Program a_program)
        {
            if (a_program.DepthTest)
            {
                GL.Enable(EnableCap.DepthTest);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }

            OpenTKProgram program = (OpenTKProgram)a_program.InternalObject;
            int handle = program.Handle;

            GL.UseProgram(handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Bind Program: ");
#endif
        }

        public void BindRenderTexture(RenderTexture a_renderTexture)
        {
            if (a_renderTexture != null)
            {
                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, a_renderTexture.RenderBuffer);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            }

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Bind Render Texture: ");
#endif
        }

        public void BindTexture(Program a_program, int a_binding, Texture a_texture, int a_index)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + a_index);
            GL.BindTexture(TextureTarget.Texture2D, a_texture.Handle);
            GL.Uniform1(a_binding, a_index);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Bind Texture: ");
#endif
        }

        public void BindMatrix4(Program a_program, int a_binding, Matrix4 a_matrix)
        {
            GL.UniformMatrix4(a_binding, false, ref a_matrix);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Bind Matrix4: ");
#endif
        }
        public void BindMatrix4(Program a_program, int a_binding, Matrix4[] a_data)
        {
            // For some reason no way to pass arrays that I am aware of.
            // The filthy way it is.
            // There are better ways but involve vodoo trickery to the extent of my knowledge
            // NOTE: Keep track of traffic if it becomes an issue vodoo it
            int count = a_data.Length;
            for (int i = 0; i < count; ++i)
            {
                GL.UniformMatrix4(a_binding + i, false, ref a_data[i]);
            }

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Bind Matrix4: ");
#endif
        }

        public void UpdateTextureRGBA(Texture a_texture, IntPtr a_data)
        {
            GL.BindTexture(TextureTarget.Texture2D, a_texture.Handle);

            GL.TexImage2D(TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            a_texture.Width, a_texture.Height,
            0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            a_data);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Update Texture RGBA: ");
#endif
        }

        public void Draw()
        {
            // A dummy vao to suppress error data is not needed
            GL.BindVertexArray(m_pipeline.StaticVAO);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Draw: ");
#endif
        }

        public void DrawElements(uint a_indices)
        {
            GL.DrawElements(PrimitiveType.Triangles, (int)a_indices, DrawElementsType.UnsignedShort, 0);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: DrawElements: ");
#endif
        }

        public void DrawElementsUInt(uint a_indices)
        {
            GL.DrawElements(PrimitiveType.Triangles, (int)a_indices, DrawElementsType.UnsignedInt, 0);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: DrawElementsUInt: ");
#endif
        }
    }
}