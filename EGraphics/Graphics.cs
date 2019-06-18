using Erde.Graphics.Lights;
using Erde.Graphics.Rendering;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Erde.Graphics
{
    public class Graphics : IDisposable
    {
        public struct DrawingContainer
        {
            Material                       m_material;

            LinkedList<RenderingContainer> m_renderers;

            public class RenderingContainer
            {
                Renderer m_renderer;
                int      m_transformBuffer = -1;

                public Renderer Renderer
                {
                    get
                    {
                        return m_renderer;
                    }
                    set
                    {
                        m_renderer = value;
                    }
                }

                public int TransformBuffer
                {
                    get
                    {
                        return m_transformBuffer;
                    }
                    set
                    {
                        m_transformBuffer = value;
                    }
                }
            }

            public Material Material
            {
                get
                {
                    return m_material;
                }
            }

            public LinkedList<RenderingContainer> Renderers
            {
                get
                {
                    return m_renderers;
                }
            }

            public DrawingContainer (Material a_material)
            {
                m_material = a_material;
                m_renderers = new LinkedList<RenderingContainer>();
            }
        }

        struct TransformContainer
        {
            public Matrix4 transform;

            // Apparently mat3 is made up of 3 Vector4
            public Vector4 rotationMatrixRow1;

            public Vector4 rotationMatrixRow2;
            public Vector4 rotationMatrixRow3;

            public Matrix3 RotationMatrix
            {
                set
                {
                    rotationMatrixRow1 = new Vector4(value.Row0, 0.0f);
                    rotationMatrixRow2 = new Vector4(value.Row1, 0.0f);
                    rotationMatrixRow3 = new Vector4(value.Row2, 0.0f);
                }
                get
                {
                    return new Matrix3(rotationMatrixRow1.Xyz, rotationMatrixRow2.Xyz, rotationMatrixRow3.Xyz);
                }
            }
        }

        struct CameraContainer
        {
            public Matrix4 view;
            public Matrix4 projection;
            public Matrix4 transform;
            public Matrix4 viewProjection;
        }

        struct LightContainer
        {
            public Vector4 color;
            public Vector4 position;
            public Vector4 direction;
            public float far;
        }

        struct TimeContainer
        {
            public float timePassed;
            public float deltaTime;
        }

        static Graphics                m_graphics;

        LinkedList<DrawingContainer>   m_drawingObjects;

        ConcurrentQueue<IRenderObject> m_inputQueue;
        ConcurrentQueue<IRenderObject> m_disposalQueue;

        UniformBufferObject            m_lightBuffer;
        UniformBufferObject            m_cameraBuffer;
        UniformBufferObject            m_transformBuffer;
        UniformBufferObject            m_timeBuffer;

        Pipeline                       m_pipeline;

        Skybox                         m_skybox;

        MultiRenderTexture             m_renderTarget;
        DrawBuffersEnum[]              m_drawBuffers;

        Program                        m_defferedShader;

        #region DEBUG
#if DEBUG_INFO
        uint m_triCount;
        uint m_lastTris;

        uint m_drawCalls;
        uint m_lastDrawCalls;

        public uint TriangleCount
        {
            get
            {
                return m_lastTris;
            }
        }
        public uint DrawCalls
        {
            get
            {
                return m_lastDrawCalls;
            }
        }

        public void AddTriangles (int a_indicies, PrimitiveType a_primitiveType)
        {
            switch (a_primitiveType)
            {
            case PrimitiveType.Triangles:
                {
                    m_triCount += (uint)a_indicies / 3;

                    break;
                }
            case PrimitiveType.TriangleStrip:
                {
                    m_triCount += (uint)a_indicies - 2;

                    break;
                }
            }
        }
#endif
        #endregion

        public Pipeline Pipeline
        {
            get
            {
                return m_pipeline;
            }
        }

        public ConcurrentQueue<IRenderObject> InputQueue
        {
            get
            {
                return m_inputQueue;
            }
        }
        public ConcurrentQueue<IRenderObject> DisposalQueue
        {
            get
            {
                return m_disposalQueue;
            }
        }

        public MultiRenderTexture DefferedOutput
        {
            get
            {
                return m_renderTarget;
            }
        }

        public Skybox Skybox
        {
            get
            {
                return m_skybox;
            }
            set
            {
                m_skybox = value;
            }
        }

        public static Graphics Active
        {
            get
            {
                return m_graphics;
            }
        }

        internal Graphics (Pipeline a_pipeline)
        {
            m_drawingObjects = new LinkedList<DrawingContainer>();

            m_inputQueue = new ConcurrentQueue<IRenderObject>();
            m_disposalQueue = new ConcurrentQueue<IRenderObject>();

            m_pipeline = a_pipeline;

            m_transformBuffer = new UniformBufferObject(m_pipeline, new TransformContainer());
            m_cameraBuffer = new UniformBufferObject(m_pipeline, new CameraContainer());
            m_lightBuffer = new UniformBufferObject(m_pipeline, new LightContainer());
            m_timeBuffer = new UniformBufferObject(m_pipeline, new TimeContainer());

            const int buffCount = 3;

            // Deffered stage render target
            m_renderTarget = new MultiRenderTexture(buffCount, 3840, 2160, m_pipeline);

            // Specify the attachments for the deffered render buffer
            m_drawBuffers = new DrawBuffersEnum[buffCount];
            for (int i = 0; i < buffCount; ++i)
            {
                m_drawBuffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }

            // The shader responsible for transfering the deffered render data to the primary render texture
            m_defferedShader = new Program(
                new PixelShader(Shaders.DEFFERED_PIXEL, m_pipeline),
                new VertexShader(Shaders.QUAD_VERTEX, m_pipeline),
                m_pipeline);

            m_defferedShader.VertexShader.Dispose();
            m_defferedShader.PixelShader.Dispose();
        }

        public void SetActive ()
        {
            m_graphics = this;
        }

        void Input ()
        {
            // Sees if there is objects in the input queue
            while (!m_inputQueue.IsEmpty)
            {
                IRenderObject obj;

                // Removes the object from the input queue
                if (!m_inputQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Graphics: Input Dequeue Failed", InternalConsole.e_Alert.Error);

                    return;
                }

                obj.AddObject(m_drawingObjects);
#if DEBUG_INFO
                Pipeline.GLError("Graphics: Input: ");
#endif
            }
        }
        void Disposal ()
        {
            while (!m_disposalQueue.IsEmpty)
            {
                IRenderObject obj;

                if (!m_disposalQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Graphics: Diposal Dequeue Failed", InternalConsole.e_Alert.Error);

                    return;
                }

                obj.RemoveObject(m_drawingObjects);
#if DEBUG_INFO
                Pipeline.GLError("Graphics: Disposal: ");
#endif
            }
        }

        Frustrum SetCameraBuffer (Camera a_camera)
        {
            CameraContainer camContainer = new CameraContainer
            {
                transform = a_camera.Transform.ToMatrix(),
                projection = a_camera.Projection
            };
            camContainer.view = camContainer.transform.Inverted();
            Matrix4 viewProjection = camContainer.view * camContainer.projection;
            camContainer.viewProjection = viewProjection;

            // Updates the target data of the Uniform Buffer Object
            m_cameraBuffer.UpdateData(camContainer);
            // Sends the data over to the GPU
            m_cameraBuffer.UpdateBuffer();

            // returns the cameras frustrum
            return new Frustrum(viewProjection);
        }

        void BindCamera ()
        {
            // Binds the camera UBO to the shader
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.CameraUBOIndex, m_cameraBuffer.Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Camera: ");
#endif
        }

        void BindTime ()
        {
            // Binds the time UBO to the shader
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.TimeUBOIndex, m_timeBuffer.Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Time: ");
#endif
        }

        void BindTransform (Matrix4 a_transform, Matrix3 a_rotationMatrix)
        {
            TransformContainer transContainer = new TransformContainer
            {
                transform = a_transform,
                RotationMatrix = a_rotationMatrix
            };

            // Updates the target data of the Unifrom Buffer Object
            m_transformBuffer.UpdateData(transContainer);
            // Sends the data over to the GPU
            m_transformBuffer.UpdateBuffer();

            // Binds the transform UBO to the shader
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.TransformUBOIndex, m_transformBuffer.Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Transform: ");
#endif
        }

        void BindTransform (Transform a_transform, ref int a_ubo)
        {
            // Cant use binary in Mono aparently
            // Checks if the the the transform has been baked
            if ((a_transform.StaticState & 0x1 << 1) == 0)
            {
                // Check if a Uniform Buffer Object has been generated
                if (a_ubo == -1)
                {
                    a_ubo = GL.GenBuffer();
                }

                GL.BindBuffer(BufferTarget.UniformBuffer, a_ubo);

                // Sets the transforms for the UBO
                TransformContainer transform = new TransformContainer()
                {
                    transform = a_transform.ToMatrix(),
                    RotationMatrix = a_transform.RotationMatrix
                };

                // Finds the size of the Uniform Buffer Object
                int size = Marshal.SizeOf(transform);

                // Allocata a unmanaged pointer for OpenGL
                IntPtr ptr = Marshal.AllocHGlobal(size);
                // Copy the UBO data into the unmanaged pointer
                Marshal.StructureToPtr(transform, ptr, false);

                // Transfer data from the pointer to the GPU
                GL.BufferData(BufferTarget.UniformBuffer, size, ptr, BufferUsageHint.StaticDraw);

                // Frees the unmanged pointer
                Marshal.FreeHGlobal(ptr);

                // Sets the baked flag
                a_transform.StaticState |= 0x1 << 1;

#if DEBUG_INFO
                Pipeline.GLError("Graphics: Create Static Transform: ");
#endif
            }

            // Binds the transform UBO to the shader
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.TransformUBOIndex, a_ubo);
        }

        BindableContainer BindMaterial (Material a_material)
        {
            BindableContainer cont = new BindableContainer();

            foreach (Material.Binding bind in a_material.Bindings)
            {
                bind.Target.Bind(cont, bind);
            }

            return cont;
        }

        void ShadowPass ()
        {
            Frustrum frustrum;

            foreach (Light light in Light.LightList)
            {
                if (light.ShadowMapped)
                {
                    // Binds the shadow map for drawing
                    // NOTE: There is no color buffer so no pixel shader is necessary
                    // this causes drawing 3 - 4 objects equivelent to a single call of this
                    light.BindShadowDrawing();

                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    // Sets the frustrum to the light
                    frustrum = new Frustrum(light.View * light.Projection);

                    // Gets the transform location in the shader
                    int transformLoc = GL.GetUniformLocation(light.ShadowProgram.Handle, "world");

                    foreach (DrawingContainer draw in m_drawingObjects)
                    {
                        foreach (DrawingContainer.RenderingContainer rend in draw.Renderers)
                        {
                            if (rend.Renderer.Visible)
                            {
                                Renderer renderer = rend.Renderer;
                                Transform transform = renderer.Transform;

                                // Threading safety
                                if (transform != null)
                                {
                                    lock (transform)
                                    {
                                        // Checks if the object is within the lights frustrum
                                        if (frustrum.CompareSphere(transform.Translation, renderer.Radius))
                                        {
                                            // Binds the objects transform to the shader
                                            Matrix4 transformMat = transform.ToMatrix();
                                            GL.UniformMatrix4(transformLoc, false, ref transformMat);

                                            // Draws the object
                                            renderer.DrawShadow(light);
#if DEBUG_INFO
                                            ++m_drawCalls;
#endif
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
#if DEBUG_INFO
                Pipeline.GLError("Graphics: Drawing Light: ");
#endif
            }
        }

        void SkyBoxPass ()
        {
            GL.Disable(EnableCap.DepthTest);

            GL.UseProgram(m_skybox.Material.Program.Handle);

            if (m_skybox.Material.CameraBinding != -1)
            {
                BindCamera();
            }

            BindMaterial(m_skybox.Material);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            GL.Enable(EnableCap.DepthTest);

#if DEBUG_INFO
            ++m_drawCalls;
            AddTriangles(4, PrimitiveType.TriangleStrip);

            Pipeline.GLError("Graphics: Drawing Skybox: ");
#endif
        }

        void DeferredPass (Vector3 a_cameraPosition, Vector3 a_cameraForward, Frustrum a_cameraFrutrum, Camera a_camera)
        {
            foreach (DrawingContainer draw in m_drawingObjects)
            {
                int progHandle = draw.Material.Program.Handle;

                GL.UseProgram(progHandle);

                BindMaterial(draw.Material);

                if (draw.Material.CameraBinding != -1)
                {
                    BindCamera();
                }

                if (draw.Material.TimeBinding != -1)
                {
                    BindTime();
                }

                foreach (DrawingContainer.RenderingContainer rend in draw.Renderers)
                {
                    if (rend.Renderer.Visible)
                    {
                        if (rend.Renderer.Material.TransformBinding != -1)
                        {
                            if (rend.Renderer.Transform != null)
                            {
                                try
                                {
                                    lock (rend.Renderer.Transform)
                                    {
                                        Vector3 translation = rend.Renderer.Transform.Translation;

                                        if (Vector3.Dot(translation - a_cameraPosition, a_cameraForward) - rend.Renderer.Radius >= 0.0f)
                                        {
                                            continue;
                                        }

                                        if (!a_cameraFrutrum.CompareSphere(translation, rend.Renderer.Radius))
                                        {
                                            continue;
                                        }

                                        if (!rend.Renderer.Transform.Static)
                                        {
                                            BindTransform(rend.Renderer.Transform.ToMatrix(), rend.Renderer.Transform.RotationMatrix);
                                        }
                                        else
                                        {
                                            int ubo = rend.TransformBuffer;

                                            BindTransform(rend.Renderer.Transform, ref ubo);

                                            rend.TransformBuffer = ubo;
                                        }
                                    }
                                }
                                catch (NullReferenceException)
                                {
                                    InternalConsole.AddMessage("Graphics Transform: Unable to acquire lock", InternalConsole.e_Alert.Warning);
                                }
                            }
                        }

                        rend.Renderer.Draw(a_camera);

#if DEBUG_INFO
                        ++m_drawCalls;

                        Pipeline.GLError("Graphics: Drawing: ");
#endif
                    }
                }
            }
        }

        void MergePass ()
        {
            GL.UseProgram(m_defferedShader.Handle);

            GLCommand.BindTexture(m_defferedShader, "diffuse", m_renderTarget.RenderTextures[0]);

            Pipeline.GLError("Graphics: Merge Binding: ");

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

#if DEBUG_INFO
            ++m_drawCalls;
            AddTriangles(4, PrimitiveType.TriangleStrip);

            Pipeline.GLError("Graphics: Merging Deffered: ");
#endif
        }

        void LightingPass ()
        {
            GL.Disable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            Material material = DirectionalLight.LightMaterial;

            foreach (Light light in Light.LightList)
            {
                if (light.Draw)
                {
                    if (light is DirectionalLight)
                    {
                        if (material == null)
                        {
                            continue;
                        }

                        GL.UseProgram(material.Program.Handle);

                        DirectionalLight directionalLight = light as DirectionalLight;

                        m_lightBuffer.UpdateData(new LightContainer()
                        {
                            color = new Vector4(directionalLight.Color.R, directionalLight.Color.G, directionalLight.Color.B, 1.0f),
                            direction = new Vector4(directionalLight.Transform.Forward, 0.0f),
                            far = directionalLight.Far
                        });
                    }
                    else
                    {
                        continue;
                    }

                    if (material.CameraBinding != -1)
                    {
                        BindCamera();
                    }

                    if (material.TransformBinding != -1)
                    {
                        BindTransform(light.Transform.ToMatrix(), light.Transform.RotationMatrix);
                    }

                    BindableContainer cont = BindMaterial(material);

                    GLCommand.BindTexture(material.Program, "normal", m_renderTarget.RenderTextures[1], cont.Textures++);
                    GLCommand.BindTexture(material.Program, "specular", m_renderTarget.RenderTextures[2], cont.Textures++);
                    GLCommand.BindTexture(material.Program, "depth", m_renderTarget.DepthBuffer, cont.Textures++);

                    int lightLoc = GL.GetUniformBlockIndex(material.Program.Handle, "light");
                    if (lightLoc != -1)
                    {
                        m_lightBuffer.UpdateBuffer();

                        GL.UniformBlockBinding(material.Program.Handle, lightLoc, Material.LightUBOIndex);
                        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.LightUBOIndex, m_lightBuffer.Handle);
                    }

                    light.BindShadowMap(cont);

                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

#if DEBUG_INFO
                    ++m_drawCalls;
                    AddTriangles(4, PrimitiveType.TriangleStrip);

                    Pipeline.GLError("Graphics: Lighting: ");
#endif
                }
            }

            GL.Disable(EnableCap.Blend);

            GL.Enable(EnableCap.DepthTest);
        }

        void Draw ()
        {
            Vector3 camPos = Vector3.Zero;
            Vector3 camForward = Vector3.Zero;

            Frustrum frustrum = null;

            TimeContainer timeContainer = new TimeContainer()
            {
                deltaTime = (float)PipelineTime.DeltaTime,
                timePassed = (float)PipelineTime.TimePassed
            };
            m_timeBuffer.UpdateData(timeContainer);
            m_timeBuffer.UpdateBuffer();

            if (Light.LightList != null)
            {
                ShadowPass();
            }

            if (Camera.CameraList != null)
            {
                foreach (Camera cam in Camera.CameraList)
                {
                    GL.Viewport(0, 0, m_renderTarget.Width, m_renderTarget.Height);

                    GL.BindFramebuffer(FramebufferTarget.FramebufferExt, m_renderTarget.BufferHandle);
                    GL.DrawBuffers(m_drawBuffers.Length, m_drawBuffers);

                    GL.Clear(cam.ClearFlags);

                    camPos = cam.Transform.Translation;
                    camForward = cam.Transform.Forward;

                    frustrum = SetCameraBuffer(cam);

                    if (cam.DrawSkybox)
                    {
                        SkyBoxPass();
                    }

                    DeferredPass(camPos, camForward, frustrum, cam);

                    GL.Viewport(cam.Viewport);

                    if (cam.RenderTexture != null)
                    {
                        GL.BindFramebuffer(FramebufferTarget.FramebufferExt, cam.RenderTexture.RenderBuffer);
                    }
                    else
                    {
                        GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
                    }

                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    MergePass();

                    if (Light.LightList != null)
                    {
                        LightingPass();
                    }

                    if (cam.PostProcessing != null)
                    {
                        cam.PostProcessing.Effect(cam.RenderTexture, cam, m_renderTarget.RenderTextures[1], m_renderTarget.RenderTextures[2], m_renderTarget.DepthBuffer);
                    }
                }
            }
        }

        internal void Update ()
        {
#if DEBUG_INFO
            m_lastTris = m_triCount;
            m_triCount = 0;

            m_lastDrawCalls = m_drawCalls;
            m_drawCalls = 0;
#endif

            Input();
            Disposal();

            Draw();
        }

        public void Dispose ()
        {
            m_defferedShader.Dispose();

            m_renderTarget.Dispose();

            m_transformBuffer.Dispose();
            m_cameraBuffer.Dispose();
            m_lightBuffer.Dispose();
            m_timeBuffer.Dispose();
        }
    }
}