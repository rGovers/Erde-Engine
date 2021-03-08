using Erde.Graphics.Internal.Shader;
using Erde.Graphics.Internal.Variables;
using Erde.Graphics.Lights;
using Erde.Graphics.Rendering;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Internal
{
    public class OpenTKGraphics : IGraphics
    {
        Graphics                       m_graphics;

        LinkedList<DrawingContainer>   m_drawingObjects;
        LinkedList<Renderer>           m_updateRenderers;

        ConcurrentQueue<IRenderObject> m_inputQueue;
        ConcurrentQueue<IRenderObject> m_disposalQueue;

        Program                        m_defferedShader;

        GizmoRenderer                  m_gizmoRenderer;

        MultiRenderTexture             m_renderTarget;
        DrawBuffersEnum[]              m_drawBuffers;

        public MultiRenderTexture DefferedOutput
        {
            get
            {
                return m_renderTarget;
            }
        }

        public OpenTKGraphics(Graphics a_graphics)
        {
            m_graphics = a_graphics;

            m_drawingObjects = new LinkedList<DrawingContainer>();
            m_updateRenderers = new LinkedList<Renderer>();

            m_inputQueue = new ConcurrentQueue<IRenderObject>();
            m_disposalQueue = new ConcurrentQueue<IRenderObject>();
        }

        public void AddObject(IRenderObject a_object)
        {
            m_inputQueue.Enqueue(a_object);
        }
        public void RemoveObject(IRenderObject a_object)
        {
            m_disposalQueue.Enqueue(a_object);
        }

        public void Init()
        {
            Pipeline pipeline = m_graphics.Pipeline;

            m_gizmoRenderer = new GizmoRenderer(pipeline);

            ModelVertexInfo[] vertexLayout = ModelVertexInfo.GetVertexInfo<Vertex>();
            int vertexSize = Marshal.SizeOf<Vertex>();

            m_defferedShader = new Program
            (
                new PixelShader(Shaders.DEFFERED_PIXEL, pipeline),
                new VertexShader(Shaders.QUAD_VERTEX, pipeline),
                vertexLayout,
                vertexSize,
                false,
                e_CullingMode.Back,
                pipeline
            );

            m_defferedShader.VertexShader.Dispose();
            m_defferedShader.PixelShader.Dispose();

            const int buffCount = 3;

            m_renderTarget = new MultiRenderTexture(buffCount, 1920, 1080, pipeline);

            m_drawBuffers = new DrawBuffersEnum[buffCount];
            for (int i = 0; i < buffCount; ++i)
            {
                m_drawBuffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
        }

        void Input ()
        {
            while (!m_inputQueue.IsEmpty)
            {
                IRenderObject obj;

                if (!m_inputQueue.TryDequeue(out obj))
                {
                    InternalConsole.AddMessage("Graphics: Input Dequeue Failed", InternalConsole.e_Alert.Error);

                    return;
                }

                obj.AddObject(m_drawingObjects, m_updateRenderers);

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

                obj.RemoveObject(m_drawingObjects, m_updateRenderers);

#if DEBUG_INFO
                Pipeline.GLError("Graphics: Disposal: ");
#endif
            }
        }

        Frustum SetCameraBuffer (Camera a_camera)
        {
            lock(a_camera)
            {
                if (a_camera.Transform == null)
                {
                    return null;
                }

                Graphics.CameraContainer camContainer = new Graphics.CameraContainer
                {
                    Transform = a_camera.Transform.ToMatrix(),
                    Projection = a_camera.Projection
                };

                camContainer.View = camContainer.Transform.Inverted();

                Matrix4 viewProjection = camContainer.View * camContainer.Projection;
                camContainer.ViewProjection = viewProjection;

                UniformBufferObject ubo = m_graphics.CameraBufferObject;

                ubo.UpdateData(camContainer);
                ubo.UpdateBuffer();

                return new Frustum(viewProjection);
            }   
        }

        void BindCamera ()
        {
            UniformBufferObject ubo = m_graphics.CameraBufferObject;

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.CameraUBOIndex, ((OpenTKUniformBufferObject)ubo.InternalObject).Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Camera: ");
#endif
        }

        void BindTime ()
        {
            UniformBufferObject ubo = m_graphics.TimeBufferObject;

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.TimeUBOIndex, ((OpenTKUniformBufferObject)ubo.InternalObject).Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Time: ");
#endif
        }

        void BindTransform (Matrix4 a_transform, Matrix3 a_rotationMatrix)
        {
            UniformBufferObject ubo = m_graphics.TransformBufferObject;

            Graphics.TransformContainer transContainer = new Graphics.TransformContainer
            {
                Transform = a_transform,
                RotationMatrix = a_rotationMatrix
            };

            ubo.UpdateData(transContainer);
            ubo.UpdateBuffer();

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.TransformUBOIndex, ((OpenTKUniformBufferObject)ubo.InternalObject).Handle);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Transform: ");
#endif
        }

        void BindTransform (Transform a_transform, ref int a_ubo)
        {
            if ((a_transform.StaticState & 0x1 << 1) == 0)
            {
                if (a_ubo == -1)
                {
                    a_ubo = GL.GenBuffer();
                }

                GL.BindBuffer(BufferTarget.UniformBuffer, a_ubo);

                Graphics.TransformContainer transform = new Graphics.TransformContainer()
                {
                    Transform = a_transform.ToMatrix(),
                    RotationMatrix = a_transform.RotationMatrix
                };

                int size = Marshal.SizeOf(transform);

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(transform, ptr, false);

                GL.BufferData(BufferTarget.UniformBuffer, size, ptr, BufferUsageHint.StaticDraw);

                Marshal.FreeHGlobal(ptr);

                a_transform.StaticState |= 0x1 << 1;

#if DEBUG_INFO
                Pipeline.GLError("Graphics: Create Static Transform: ");
#endif
            }

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.TransformUBOIndex, a_ubo);
        }

        BindableContainer BindMaterial (Material a_material)
        {
            BindableContainer cont = new BindableContainer();

            foreach (Binding bind in a_material.Bindings)
            {
                bind.Target.Bind(cont, bind);
            }

            return cont;
        }

        void ShadowPass ()
        {
            Frustum frustrum;

            LinkedList<Light> lights;
            lock (Light.LightList)
            {
                lights = Light.LightList;
            }

            foreach (Light light in lights)
            {
                if (light.ShadowMapped)
                {
                    light.BindShadowDrawing();

#if DEBUG_INFO
                    Pipeline.GLError("Graphics: Binding Shadow Drawing: ");
#endif

                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    frustrum = new Frustum(light.View * light.Projection);

                    foreach (DrawingContainer draw in m_drawingObjects)
                    {
                        foreach (DrawingContainer.RenderingContainer rend in draw.Renderers)
                        {
                            Renderer renderer = rend.Renderer;
                            if (renderer.Visible)
                            {
                                Transform transform = renderer.Transform;
                                if (transform != null)
                                {
                                    lock (transform.GameObject)
                                    {
                                        Vector3 translation = transform.Translation;
                                        float radius = renderer.Radius;

                                        if (radius != -1 && !frustrum.CompareSphere(translation, radius))
                                        {
                                            continue;
                                        }

                                        Matrix4 transformMat = transform.ToMatrix();
                                        GL.UniformMatrix4(1, false, ref transformMat);

                                        renderer.DrawShadow(light);
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Drawing Shadow: ");
#endif
        }

        void SkyBoxPass ()
        {
            Material material = m_graphics.Skybox.Material;  
            GL.Disable(EnableCap.DepthTest);

            GL.UseProgram(((OpenTKProgram)material.Program.InternalObject).Handle);

            BindCamera();

            BindMaterial(material);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            GL.Enable(EnableCap.DepthTest);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Drawing Skybox: ");
#endif
        }

        void DeferredPass (Vector3 a_cameraPosition, Vector3 a_cameraForward, Frustum a_cameraFrutrum, Camera a_camera)
        {
            foreach (DrawingContainer draw in m_drawingObjects)
            {
                Material material = draw.Material;
                int progHandle = ((OpenTKProgram)material.Program.InternalObject).Handle;

                GL.UseProgram(progHandle);

                BindMaterial(material);
                
                BindCamera();
                BindTime();

                foreach (DrawingContainer.RenderingContainer rend in draw.Renderers)
                {
                    Renderer renderer = rend.Renderer;

                    if (renderer.Visible)
                    {
                        Transform transform = renderer.Transform;
                        if (transform != null)
                        {
                            lock (transform.GameObject)
                            {
                                Vector3 translation = transform.Translation;

                                if (!a_cameraFrutrum.CompareSphere(translation, renderer.Radius))
                                {
                                    continue;
                                }

                                if (!transform.Static)
                                {
                                    BindTransform(transform.ToMatrix(), transform.RotationMatrix);
                                }
                                else
                                {
                                    int ubo = rend.TransformBuffer;

                                    BindTransform(transform, ref ubo);

                                    rend.TransformBuffer = ubo;
                                }
                            }
                        }

                        renderer.Draw(a_camera);

#if DEBUG_INFO
                        Pipeline.GLError("Graphics: Drawing: ");
#endif
                    }
                }
            }
        }

        void MergePass ()
        {
            int progHandle = ((OpenTKProgram)m_defferedShader.InternalObject).Handle;
            GL.UseProgram(progHandle);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, m_renderTarget.RenderTextures[0].Handle);
            GL.Uniform1(1, 0);

            // I do not need the vertex data however I apparently need data bound so just have an empty object
            OpenTKPipeline pipeline = (OpenTKPipeline)m_graphics.Pipeline.InternalPipeline;
            GL.BindVertexArray(pipeline.StaticVAO);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Merge Binding: ");
#endif

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Merging Deffered: ");
#endif
        }
        
        void LightingPass ()
        {
            GL.Disable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            foreach (Light light in Light.LightList)
            {
                if (light.Draw)
                {
                    UniformBufferObject ubo = m_graphics.LightBufferObject;

                    Material material = light.BindLightDrawing();
                    ubo.UpdateData(light.GetLightData());

                    BindCamera();
                    BindTransform(light.Transform.ToMatrix(), light.Transform.RotationMatrix);

                    BindableContainer cont = BindMaterial(material);

                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, m_renderTarget.RenderTextures[1].Handle);
                    GL.Uniform1(0, cont.Textures++);
                
                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, m_renderTarget.RenderTextures[2].Handle);
                    GL.Uniform1(1, cont.Textures++);

                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, m_renderTarget.DepthBuffer.Handle);
                    GL.Uniform1(2, cont.Textures++);

                    ubo.UpdateBuffer();

                    GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Material.LightUBOIndex, ((OpenTKUniformBufferObject)ubo.InternalObject).Handle);

                    light.BindShadowMap(cont);

                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

#if DEBUG_INFO
                    Pipeline.GLError("Graphics: Lighting: ");
#endif
                }
            }

            GL.Disable(EnableCap.Blend);

            GL.Enable(EnableCap.DepthTest);
        }

        void Draw ()
        {
            if (Light.LightList != null)
            {
                ShadowPass();
            }

            List<Camera> cameraList = Camera.CameraList;
            if (cameraList != null)
            {
                lock (cameraList)
                {
                    foreach (Camera cam in cameraList)
                    {
                        lock (cam)
                        {
                            Transform transform = cam.Transform;

                            if (transform == null)
                            {
                                continue;
                            }

                            GL.Viewport(0, 0, m_renderTarget.Width, m_renderTarget.Height);

                            int renderTargetHandle = m_renderTarget.BufferHandle;

                            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, renderTargetHandle);
                            GL.DrawBuffers(m_drawBuffers.Length, m_drawBuffers);

                            GL.ClearColor(cam.ClearColor);
                            GL.Clear(cam.ClearFlags);

                            Vector3 camPos = transform.Translation;
                            Vector3 camForward = transform.Forward;

                            Frustum frustrum = SetCameraBuffer(cam);
                            
                            if (cam.DrawSkybox)
                            {
                                SkyBoxPass();
                            }

                            DeferredPass(camPos, camForward, frustrum, cam);

                            GL.Viewport(cam.Viewport);

                            RenderTexture renderTexture = cam.RenderTexture;

                            if (renderTexture != null)
                            {
                                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, renderTexture.RenderBuffer);
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

                            m_gizmoRenderer.Update();

                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.Back);

                            if (cam.PostProcessing != null)
                            {
                                cam.PostProcessing.Effect(renderTexture, cam, m_renderTarget.RenderTextures[1], m_renderTarget.RenderTextures[2], m_renderTarget.DepthBuffer);
                            }   
                        }
                    }
                }
            }
        }

        public void Update ()
        {
            Input();
            Disposal();

            foreach (Renderer renderer in m_updateRenderers)
            {
                renderer.Update();
            }

            Draw();
        }

        public void Dispose()
        {
            m_gizmoRenderer.Dispose();

            m_defferedShader.Dispose();

            m_renderTarget.Dispose();
        }
    }
}