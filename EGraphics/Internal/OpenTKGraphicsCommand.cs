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
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

#if DEBUG_INFO
            Pipeline.GLError("Graphics Command: Draw: ");
#endif
        }
    }
}