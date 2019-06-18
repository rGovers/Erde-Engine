using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Erde.Graphics
{
    public static class GLCommand
    {
        public static void BindProgram (Program a_program)
        {
            GL.UseProgram(a_program.Handle);
        }

        public static void BindTexture (Program a_program, string a_name, Texture a_texture, int a_slot = 0)
        {
            int location = GL.GetUniformLocation(a_program.Handle, a_name);
            if (location != -1)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + a_slot);
                GL.BindTexture(TextureTarget.Texture2D, a_texture.Handle);
                GL.Uniform1(location, a_slot);
            }
        }
        public static void BindTexture (Program a_program, string a_name, CubeMap a_cubeMap, int a_slot = 0)
        {
            int location = GL.GetUniformLocation(a_program.Handle, a_name);
            if (location != -1)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + a_slot);
                GL.BindTexture(TextureTarget.TextureCubeMap, a_cubeMap.Handle);
                GL.Uniform1(location, a_slot);
            }
        }

        public static void BindMatrix4 (Program a_program, string a_name, Matrix4 a_matrix)
        {
            int location = GL.GetUniformLocation(a_program.Handle, a_name);
            if (location != -1)
            {
                GL.UniformMatrix4(location, false, ref a_matrix);
            }
        }

        public static void BindRenderTexture (RenderTexture a_renderTexture)
        {
            if (a_renderTexture != null)
            {
                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, a_renderTexture.RenderBuffer);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            }
        }

        public static void Blit (Texture a_texture, Vector2 a_position, Vector2 a_size, bool a_invert = false)
        {
            Matrix4 transform = Matrix4.CreateTranslation(a_position.X, a_position.Y, 0.0f) * Matrix4.CreateScale(a_size.X, a_size.Y, 1.0f);

            Blit(a_texture, transform, a_invert);
        }

        public static void Blit (Texture a_texture, Matrix4 a_tranform, bool a_invert = false)
        {
            int handle = 0;

            if (!a_invert)
            {
                handle = Shaders.TRANSFORM_IMAGE_SHADER.Handle;
            }
            else
            {
                handle = Shaders.TRANSFORM_IMAGE_SHADER_INVERTED.Handle;
            }

            GL.UseProgram(handle);

            int textureLocation = GL.GetUniformLocation(handle, "diffuse");
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, a_texture.Handle);
            GL.Uniform1(textureLocation, 0);

            int transformLocation = GL.GetUniformLocation(handle, "transform");
            GL.UniformMatrix4(transformLocation, false, ref a_tranform);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}
