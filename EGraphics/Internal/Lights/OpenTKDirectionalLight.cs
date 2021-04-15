using Erde.Graphics;
using Erde.Graphics.Internal.Shader;
using Erde.Graphics.Lights;
using Erde.Graphics.Rendering;
using Erde.Graphics.Shader;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace Erde.Graphics.Internal.Lights
{
    public class OpenTKDirectionalLight : ILight
    {
        DirectionalLight m_light;

        int[]            m_shadowBuffer;
        int[]            m_shadowMap;

        float[]          m_splits;

        public OpenTKDirectionalLight(DirectionalLight a_light)
        {
            m_light = a_light;

            m_shadowMap = null;
            m_shadowBuffer = null;

            m_splits = null;
        }
        
        public void CalculateSplits(Camera a_camera)
        {
            const float lamba = 0.3f;

            float near = a_camera.Near;
            float far = a_camera.Far;

            float diff = far - near;
            float ratio = far / near;

            int mapCount = m_light.MapCount;
            int splits = mapCount;

            m_splits = new float[mapCount + 1];

            for (int i = 0; i < splits; ++i)
            {
                float si = (float)i / splits;

                m_splits[i] = lamba * (near * (float)Math.Pow(ratio, si)) + (1 - lamba) * (near + diff * si);
            }
            m_splits[mapCount] = far;
        }

        public void BindShadowMap (BindableContainer a_bindableContainer)
        {
            if (m_shadowBuffer != null)
            {                
                int mapCount = m_light.MapCount;
                for (int i = 0; i < mapCount; ++i)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + a_bindableContainer.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, m_shadowMap[i]);
                    GL.Uniform1(16 + i, a_bindableContainer.Textures++);

                    Matrix4 view = m_light.GetView(i);
                    Matrix4 proj = m_light.GetProjection(i);

                    Matrix4 viewProj = view * proj;

                    GL.UniformMatrix4(32 + i, false, ref viewProj);

                    GL.Uniform1(48 + i, m_splits[i + 1]);
                }
            }

#if DEBUG_INFO
            Pipeline.GLError("Directional Light: Bind Shadow Map: ");
#endif
        }
        public Frustum BindShadowDrawing (int a_index, Camera a_camera)
        {
            if (m_shadowBuffer == null)
            {
                return null;
            }
                    
            Transform transform = m_light.Transform;

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

            Matrix4 viewInv = Matrix4.Identity;
            Matrix4 camProj = Matrix4.Identity;
            lock (a_camera)
            {
                viewInv = a_camera.Transform.ToMatrix();
                camProj = Matrix4.CreatePerspectiveFieldOfView(a_camera.FOV, a_camera.Width / (float)a_camera.Height, m_splits[a_index], m_splits[a_index + 1]);
            }
            Matrix4 projInv = Matrix4.Invert(camProj);

            Vector3 position = Vector3.Zero;

            Vector3 min = new Vector3(float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity);

            Matrix3 rot3 = transform.RotationMatrix;
            for (int i = 0; i < 8; ++i)
            {
                corners[i] = corners[i] * projInv;
                corners[i] /= corners[i].W;
                corners[i] = corners[i] * viewInv;

                position += corners[i].Xyz * 0.125f;   
            }

            Matrix4 transformMat = Matrix4.CreateFromQuaternion(transform.Quaternion) * Matrix4.CreateTranslation(position);
            Matrix4 view = Matrix4.Invert(transformMat);

            Vector4[] endCorners = new Vector4[8];
            for (int i = 0; i < 8; ++i)
            {
                endCorners[i] = corners[i] * view;

                min.X = Math.Min(endCorners[i].X, min.X);
                min.Y = Math.Min(endCorners[i].Y, min.Y);
                min.Z = Math.Min(endCorners[i].Z, min.Z);

                max.X = Math.Max(endCorners[i].X, max.X);
                max.Y = Math.Max(endCorners[i].Y, max.Y);
                max.Z = Math.Max(endCorners[i].Z, max.Z);
            }

            Vector3 extent = max - min;

            Matrix4 proj = Matrix4.CreateOrthographic(extent.X * 2, extent.Y * 2, -extent.Z * 2, extent.Z);

            Matrix4 viewProj = view * proj;

            m_light.SetViewProjection(view, proj, a_index);

            OpenTKProgram program = (OpenTKProgram)m_light.ShadowProgram.InternalObject;
            int handle = program.Handle;

            GL.UseProgram(handle);

            GL.Viewport(0, 0, DirectionalLight.MapResolution, DirectionalLight.MapResolution);

            GL.UniformMatrix4(0, false, ref viewProj);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_shadowBuffer[a_index]);

#if DEBUG_INFO
            Pipeline.GLError("Directional Light: Bind Shadow Drawing: ");
#endif

            return new Frustum(viewProj);
        }

        public Material BindLightDrawing()
        {
            Material mat = DirectionalLight.LightMaterial;

            OpenTKProgram program = (OpenTKProgram)mat.Program.InternalObject;
            int handle = program.Handle;

            GL.UseProgram(handle);

            return mat;
        }
        public Graphics.LightContainer GetLightData()
        {
            Color color = m_light.Color;
            Vector4 forward = new Vector4(m_light.Transform.Forward, 0);
            float far = m_light.Far;

            return new Graphics.LightContainer()
            {
                Color = new Vector4(color.R, color.G, color.B, 1.0f),
                Direction = forward,
                Far = far
            };
        }

        public void ModifyObject()
        {
            int count = m_light.MapCount;

            m_shadowMap = new int[count];
            GL.GenTextures(count, m_shadowMap);
            m_shadowBuffer = new int[count];
            GL.GenFramebuffers(count, m_shadowBuffer);
            
            for (int i = 0; i < count; ++i)
            {
                GL.BindTexture(TextureTarget.Texture2D, m_shadowMap[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, DirectionalLight.MapResolution, DirectionalLight.MapResolution, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_shadowBuffer[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, m_shadowMap[i], 0);
            }
        }

        public void DisposeObject()
        {
            int count = m_light.MapCount;

            GL.DeleteTextures(count, m_shadowMap);
            GL.DeleteFramebuffers(count, m_shadowBuffer);
        }

        public void Dispose()
        {

        }
    }
}