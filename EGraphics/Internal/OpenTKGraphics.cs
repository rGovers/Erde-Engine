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
        MultiRenderTexture             m_transparentRenderTarget;
        DrawBuffersEnum[]              m_drawBuffers;

        public MultiRenderTexture DefferedOutput
        {
            get
            {
                return m_renderTarget;
            }
        }

        public MultiRenderTexture TransparentDefferedOutput
        {
            get
            {
                return m_transparentRenderTarget;
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
            m_transparentRenderTarget = new MultiRenderTexture(buffCount, 1920, 1080, pipeline);

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
            if (a_ubo == -1 || (a_transform.StaticState & 0x1 << 1) == 0)
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

#if DEBUG_INFO
            Pipeline.GLError("Graphics: Binding Static Transform: ");
#endif
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

        void ShadowPass (Camera a_cam)
        {
            Frustum frustrum;

            LinkedList<Light> lights = Light.LightList;

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            foreach (Light light in lights)
            {
                if (light.ShadowMapped)
                {
                    light.CalculateSplits(a_cam);

                    int mapCount = light.MapCount;
                    for (int i = 0; i < mapCount; ++i)
                    {
                        frustrum = light.BindShadowDrawing(i, a_cam);

                        if (frustrum == null)
                        {
                            continue;
                        }

                        GL.Clear(ClearBufferMask.DepthBufferBit);

                        foreach (DrawingContainer draw in m_drawingObjects)
                        {
                            foreach (DrawingContainer.RenderingContainer rend in draw.Renderers)
                            {
                                Renderer renderer = rend.Renderer;
                                if (renderer.ShadowDraw && renderer.Visible)
                                {
                                    Transform transform = renderer.Transform;
                                    if (transform != null)
                                    {
                                        lock (transform.GameObject)
                                        {
                                            Vector3 translation = transform.Translation;
                                            Vector3 scale = transform.Scale;

                                            float max = (float)Math.Max(scale.X, Math.Max(scale.Y, scale.Z));

                                            float radius = renderer.Radius;
                                            float finalRadius = radius * max;

                                            if (radius != -1 && !frustrum.CompareSphere(translation, finalRadius))
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

        void DeferredPass (Vector3 a_cameraPosition, Vector3 a_cameraForward, Frustum a_cameraFrutrum, Camera a_camera, e_TransparencyMode a_transparencyMode)
        {
            foreach (DrawingContainer draw in m_drawingObjects)
            {
                Material material = draw.Material;
                if ((material.Transparency & a_transparencyMode) == 0)
                {
                    continue;
                }
                
                Program program = material.Program;
                int progHandle = ((OpenTKProgram)program.InternalObject).Handle;

                GL.UseProgram(progHandle);

                if (program.DepthTest)
                {
                    GL.Enable(EnableCap.DepthTest);
                }
                else
                {
                    GL.Disable(EnableCap.DepthTest);
                }

                switch (program.CullingMode)
                {
                    case e_CullingMode.None:
                    {
                        GL.Disable(EnableCap.CullFace);

                        break;
                    }
                    case e_CullingMode.Front:
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.Front);

                        break;
                    }
                    case e_CullingMode.Back:
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.Back);

                        break;
                    }
                    case e_CullingMode.FrontAndBack:
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.FrontAndBack);

                        break;
                    }
                }

                BindableContainer cont = BindMaterial(material);
                
                BindCamera();
                BindTime();

                if ((a_transparencyMode & e_TransparencyMode.Transparent) != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, ((OpenTKTexture)m_renderTarget.DepthBuffer.InternalObject).Handle);
                    GL.Uniform1(19, cont.Textures++);
                }

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
                                Vector3 scale = transform.Scale;

                                float max = (float)Math.Max(scale.X, Math.Max(scale.Y, scale.Z));

                                float radius = renderer.Radius;
                                float finalRadius = radius * max;

                                if (radius != -1 && !a_cameraFrutrum.CompareSphere(translation, finalRadius))
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

        void MergePass (MultiRenderTexture a_renderTexture)
        {
            int progHandle = ((OpenTKProgram)m_defferedShader.InternalObject).Handle;
            GL.UseProgram(progHandle);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ((OpenTKTexture)a_renderTexture.RenderTextures[0].InternalObject).Handle);
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
        
        void LightingPass (MultiRenderTexture a_renderTexture)
        {
            GL.Disable(EnableCap.DepthTest);
            // GL.Disable(EnableCap.Blend);

            GL.Enable(EnableCap.Blend);
            // GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

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
                    GL.BindTexture(TextureTarget.Texture2D, ((OpenTKTexture)a_renderTexture.RenderTextures[0].InternalObject).Handle);
                    GL.Uniform1(0, cont.Textures++);

                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, ((OpenTKTexture)a_renderTexture.RenderTextures[1].InternalObject).Handle);
                    GL.Uniform1(1, cont.Textures++);
                
                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, ((OpenTKTexture)a_renderTexture.RenderTextures[2].InternalObject).Handle);
                    GL.Uniform1(2, cont.Textures++);

                    GL.ActiveTexture(TextureUnit.Texture0 + cont.Textures);
                    GL.BindTexture(TextureTarget.Texture2D, ((OpenTKTexture)m_renderTarget.DepthBuffer.InternalObject).Handle);
                    GL.Uniform1(3, cont.Textures++);

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

                            if (Light.LightList != null)
                            {
                                ShadowPass(cam);
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

                            DeferredPass(camPos, camForward, frustrum, cam, e_TransparencyMode.Opaque);

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

                            MergePass(m_renderTarget);

                            LinkedList<Light> lightList = Light.LightList;

                            if (lightList != null)
                            {
                                LightingPass(m_renderTarget);
                            }

                            Texture depthBuffer = m_renderTarget.DepthBuffer;

                            int transparentRenderTargetHandle = m_transparentRenderTarget.BufferHandle;
                            int depthTextureHandle = ((OpenTKTexture)depthBuffer.InternalObject).Handle;

                            GL.DepthMask(false);

                            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, transparentRenderTargetHandle);
                            GL.DrawBuffers(m_drawBuffers.Length, m_drawBuffers);

                            // Bind other render texture depth buffer to transparent render texture
                            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, depthTextureHandle, 0);

                            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
                            GL.Clear(ClearBufferMask.ColorBufferBit);

                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                            DeferredPass(camPos, camForward, frustrum, cam, e_TransparencyMode.Transparent);

                            if (renderTexture != null)
                            {
                                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, renderTexture.RenderBuffer);
                            }
                            else
                            {
                                GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
                            }

                            GL.DepthMask(true);

                            GL.Disable(EnableCap.DepthTest);

                            MergePass(m_transparentRenderTarget);

                            if (lightList != null)
                            {
                                LightingPass(m_transparentRenderTarget);
                            }

                            m_gizmoRenderer.Update();

                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.Back);

                            if (cam.PostProcessing != null)
                            {
                                cam.PostProcessing.Effect(renderTexture, cam, m_renderTarget.RenderTextures[1], m_renderTarget.RenderTextures[2], depthBuffer);
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