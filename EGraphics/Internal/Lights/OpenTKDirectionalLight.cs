using Erde.Graphics;
using Erde.Graphics.Internal.Shader;
using Erde.Graphics.Lights;
using Erde.Graphics.Shader;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Erde.Graphics.Internal.Lights
{
    public class OpenTKDirectionalLight : ILight
    {
        DirectionalLight m_light;

        int              m_shadowBuffer;
        int              m_shadowMap;

        public OpenTKDirectionalLight(DirectionalLight a_light)
        {
            m_light = a_light;

            m_shadowMap = -1;
            m_shadowBuffer = -1;
        }

        public void BindShadowMap (BindableContainer a_bindableContainer)
        {
            if (m_shadowBuffer != -1)
            {
                OpenTKProgram program = (OpenTKProgram)DirectionalLight.LightMaterial.Program.InternalObject;
                int handle = program.Handle;

                GL.ActiveTexture(TextureUnit.Texture0 + a_bindableContainer.Textures);
                GL.BindTexture(TextureTarget.Texture2D, m_shadowMap);
                GL.Uniform1(3, a_bindableContainer.Textures++);

                Matrix4 view = m_light.View;
                Matrix4 proj = m_light.Projection;

                Matrix4 viewProj = view * proj;

                GL.UniformMatrix4(4, false, ref viewProj);
            }
        }
        public void BindShadowDrawing ()
        {
            if (m_shadowBuffer != -1)
            {
                OpenTKProgram program = (OpenTKProgram)m_light.ShadowProgram.InternalObject;
                int handle = program.Handle;

                Transform transform = m_light.Transform;

                Matrix4 view = m_light.View;
                Matrix4 proj = m_light.Projection;

                float far = m_light.Far;

                GL.Viewport(0, 0, DirectionalLight.MapResolution, DirectionalLight.MapResolution);

                GL.UseProgram(handle);

                Camera cam = Camera.MainCamera;

                if (cam == null)
                {
                    proj = Matrix4.CreateOrthographic(far * 2, far * 2, -far, far);

                    view = Matrix4.LookAt(Vector3.Zero, -transform.Forward, Vector3.UnitY);
                }
                else
                {
                    Vector4[] corners = new Vector4[]
                    {
                        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        new Vector4(1.0f, 1.0f, -1.0f, 1.0f),
                        new Vector4(1.0f, -1.0f, 1.0f, 1.0f),
                        new Vector4(1.0f, -1.0f, -1.0f, 1.0f),
                        new Vector4(-1.0f, 1.0f, 1.0f, 1.0f),
                        new Vector4(-1.0f, 1.0f, -1.0f, 1.0f),
                        new Vector4(-1.0f, -1.0f, 1.0f, 1.0f),
                        new Vector4(-1.0f, -1.0f, -1.0f, 1.0f)
                    };
                    
                    Matrix4 viewInv = cam.Transform.ToMatrix();
                    Matrix4 projInv = Matrix4.Invert(cam.Projection);

                    Vector3 position = Vector3.Zero;

                    Matrix3 rot3 = transform.RotationMatrix;
                    for (int i = 0; i < corners.Length; ++i)
                    {
                        corners[i] = corners[i] * (projInv * viewInv);
                        corners[i] /= corners[i].W;

                        position += corners[i].Xyz /** rot3*/;
                        corners[i] = new Vector4(rot3 * corners[i].Xyz, 1);
                    }

                    position /= corners.Length;

                    view = Matrix4.LookAt(position, position - transform.Forward, Vector3.UnitY);

                    Vector3 min = new Vector3(float.PositiveInfinity);
                    Vector3 max = new Vector3(float.NegativeInfinity);

                    for (int i = 0; i < corners.Length; ++i)
                    {
                        corners[i] = view * corners[i];

                        min.X = Math.Min(corners[i].X, min.X);
                        min.Y = Math.Min(corners[i].Y, min.Y);
                        min.Z = Math.Min(corners[i].Z, min.Z);
                        max.X = Math.Max(corners[i].X, max.X);
                        max.Y = Math.Max(corners[i].Y, max.Y);
                        max.Z = Math.Max(corners[i].Z, max.Z);
                    }

                    Vector3 halfExtent = 0.5f * (max - min);

                    m_light.Far = halfExtent.Z;

                    proj = Matrix4.CreateOrthographic(halfExtent.X * 2, halfExtent.Y * 2, -halfExtent.Z, halfExtent.Z);
                }

                Matrix4 viewProj = view * proj;

                m_light.SetViewProjection(view, proj);

                GL.UniformMatrix4(0, false, ref viewProj);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_shadowBuffer);
            }
        }

        public void ModifyObject()
        {
            m_shadowMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_shadowMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, DirectionalLight.MapResolution, DirectionalLight.MapResolution, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

            m_shadowBuffer = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_shadowBuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, m_shadowMap, 0);
        }

        public void DisposeObject()
        {
            GL.DeleteTexture(m_shadowMap);
            GL.DeleteFramebuffer(m_shadowBuffer);
        }

        public void Dispose()
        {

        }
    }
}