using Erde.Graphics.Rendering;
using Erde.Graphics.Shader;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Erde.Graphics.Lights
{
    public class DirectionalLight : Light, IGLObject
    {
        static Material m_material;
        static Program  m_shadowShader;

        int             m_shadowBuffer;
        int             m_shadowMap;

        Pipeline        m_pipeline;

        Matrix4         m_view;
        Matrix4         m_projection;

        public static int MapResolution
        {
            get
            {
                return 4096;
            }
        }

        public static Material LightMaterial
        {
            get
            {
                return m_material;
            }
            set
            {
                m_material = value;
            }
        }

        public override Matrix4 View
        {
            get
            {
                return m_view;
            }
        }

        public override Matrix4 Projection
        {
            get
            {
                return m_projection;
            }
        }

        public override Program ShadowProgram
        {
            get
            {
                return m_shadowShader;
            }
        }

        public override void BindShadowMap (BindableContainer a_bindableContainer)
        {
            if (m_shadowBuffer != -1)
            {
                int loc = GL.GetUniformLocation(m_material.Program.Handle, "shadow");
                GL.ActiveTexture(TextureUnit.Texture0 + a_bindableContainer.Textures);
                GL.BindTexture(TextureTarget.Texture2D, m_shadowMap);
                GL.Uniform1(loc, a_bindableContainer.Textures++);

                Matrix4 viewProj = m_view * m_projection;

                int vpLoc = GL.GetUniformLocation(m_material.Program.Handle, "lvp");
                GL.ProgramUniformMatrix4(m_material.Program.Handle, vpLoc, false, ref viewProj);
            }
        }

        public override void BindShadowDrawing ()
        {
            if (m_shadowBuffer != -1)
            {
                GL.Viewport(0, 0, MapResolution, MapResolution);

                GL.UseProgram(m_shadowShader.Handle);

                Camera cam = Camera.MainCamera;

                if (cam == null)
                {
                    m_projection = Matrix4.CreateOrthographic(Far * 2, Far * 2, -Far, Far);

                    m_view = Matrix4.LookAt(Vector3.Zero, -Transform.Forward, Vector3.UnitY);
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

                    Matrix3 rot3 = Transform.RotationMatrix;
                    for (int i = 0; i < corners.Length; ++i)
                    {
                        corners[i] = corners[i] * (projInv * viewInv);
                        corners[i] /= corners[i].W;

                        position += corners[i].Xyz /** rot3*/;
                        corners[i] = new Vector4(rot3 * corners[i].Xyz, 1);
                    }

                    position /= corners.Length;

                    m_view = Matrix4.LookAt(position, position - Transform.Forward, Vector3.UnitY);

                    Vector3 min = new Vector3(float.PositiveInfinity);
                    Vector3 max = new Vector3(float.NegativeInfinity);

                    for (int i = 0; i < corners.Length; ++i)
                    {
                        corners[i] = m_view * corners[i];

                        min.X = Math.Min(corners[i].X, min.X);
                        min.Y = Math.Min(corners[i].Y, min.Y);
                        min.Z = Math.Min(corners[i].Z, min.Z);
                        max.X = Math.Max(corners[i].X, max.X);
                        max.Y = Math.Max(corners[i].Y, max.Y);
                        max.Z = Math.Max(corners[i].Z, max.Z);
                    }

                    Vector3 halfExtent = 0.5f * (max - min);

                    Far = halfExtent.Z;

                    m_projection = Matrix4.CreateOrthographic(halfExtent.X * 2, halfExtent.Y * 2, -halfExtent.Z, halfExtent.Z);
                }

                Matrix4 viewProj = m_view * m_projection;

                int vpLoc = GL.GetUniformLocation(m_shadowShader.Handle, "lvp");
                GL.UniformMatrix4(vpLoc, false, ref viewProj);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_shadowBuffer);
            }
        }

        public static void ClearAssets ()
        {
            if (m_shadowShader != null)
            {
                m_shadowShader.VertexShader.Dispose();

                m_shadowShader.Dispose();
                m_shadowShader = null;
            }
        }

        public DirectionalLight (bool a_shadowMap, Pipeline a_pipeline) : base(a_shadowMap)
        {
            m_pipeline = a_pipeline;

            m_shadowMap = -1;
            m_shadowBuffer = -1;

            if (ShadowMapped)
            {
                if (m_shadowShader == null)
                {
                    m_shadowShader = new Program(null, new VertexShader(Shaders.DIRECTIONAL_VERTEX, m_pipeline), m_pipeline);
                }

                Far = 65;

                m_pipeline.InputQueue.Enqueue(this);
            }
        }

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            m_pipeline.DisposalQueue.Enqueue(this);
        }

        ~DirectionalLight ()
        {
            Dispose(false);
        }

        public new void Dispose ()
        {
            base.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ModifyObject ()
        {
            m_shadowMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_shadowMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, MapResolution, MapResolution, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

            m_shadowBuffer = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_shadowBuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, m_shadowMap, 0);
        }

        public void DisposeObject ()
        {
            GL.DeleteTexture(m_shadowMap);
            GL.DeleteFramebuffer(m_shadowBuffer);
        }
    }
}