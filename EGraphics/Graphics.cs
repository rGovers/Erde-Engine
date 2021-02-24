using Erde.Application;
using Erde.Graphics.Internal;
using Erde.Graphics.Rendering;
using Erde.Graphics.Variables;
using OpenTK;
using System;
using System.Diagnostics;

namespace Erde.Graphics
{
    public class Graphics : IDisposable
    {
        internal struct TransformContainer
        {
            public Matrix4 Transform;

            public Vector4 RotationMatrixRow1;
            public Vector4 RotationMatrixRow2;
            public Vector4 RotationMatrixRow3;

            public Matrix3 RotationMatrix
            {
                set
                {
                    RotationMatrixRow1 = new Vector4(value.Row0, 0.0f);
                    RotationMatrixRow2 = new Vector4(value.Row1, 0.0f);
                    RotationMatrixRow3 = new Vector4(value.Row2, 0.0f);
                }
                get
                {
                    return new Matrix3(RotationMatrixRow1.Xyz, RotationMatrixRow2.Xyz, RotationMatrixRow3.Xyz);
                }
            }
        }

        internal struct CameraContainer
        {
            public Matrix4 View;
            public Matrix4 Projection;
            public Matrix4 Transform;
            public Matrix4 ViewProjection;
        }

        public struct LightContainer
        {
            public Vector4 Color;
            public Vector4 Position;
            public Vector4 Direction;
            public float Far;
        }

        internal struct TimeContainer
        {
            public float TimePassed;
            public float DeltaTime;
        }

        static Graphics Instance;

        Pipeline                       m_pipeline;

        UniformBufferObject            m_lightBuffer;
        UniformBufferObject            m_cameraBuffer;
        UniformBufferObject            m_transformBuffer;
        UniformBufferObject            m_timeBuffer;

        Skybox                         m_skybox;

        IGraphics                      m_internalGraphics;

        bool                           m_init = false;

        public Pipeline Pipeline
        {
            get
            {
                return m_pipeline;
            }
        }

        internal IGraphics InternalGraphics
        {
            get
            {
                return m_internalGraphics;
            }
        }

        public MultiRenderTexture DefferedOutput
        {
            get
            {
                return m_internalGraphics.DefferedOutput;
            }
        }

        internal UniformBufferObject LightBufferObject
        {
            get
            {
                return m_lightBuffer;
            }
        }
        internal UniformBufferObject CameraBufferObject
        {
            get
            {
                return m_cameraBuffer;
            }
        }
        internal UniformBufferObject TransformBufferObject
        {
            get
            {
                return m_transformBuffer;
            }
        }
        internal UniformBufferObject TimeBufferObject
        {
            get
            {
                return m_timeBuffer;
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
                return Instance;
            }
        }

        public bool IsInitialised
        {
            get
            {
                return m_init;
            }
        }

        internal Graphics (Pipeline a_pipeline)
        {
            Debug.Assert(Instance == null);
            Instance = this;

            if (a_pipeline.Application.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalGraphics = new OpenTKGraphics(this);
            }
            else
            {
                m_internalGraphics = new NativeGraphics(this);
            }

            m_pipeline = a_pipeline;
        }

        internal void Init()
        {
            m_transformBuffer = new UniformBufferObject(new TransformContainer(), 0, m_pipeline);
            m_cameraBuffer = new UniformBufferObject(new CameraContainer(), 1, m_pipeline);
            m_lightBuffer = new UniformBufferObject(new LightContainer(), 2, m_pipeline);
            m_timeBuffer = new UniformBufferObject(new TimeContainer(), 3, m_pipeline);

            m_internalGraphics.Init();

            m_init = true;
        }

        public void AddObject(IRenderObject a_renderObject)
        {
            m_internalGraphics.AddObject(a_renderObject);
        }
        public void RemoveObject(IRenderObject a_renderObject)
        {
            m_internalGraphics.RemoveObject(a_renderObject);
        }

        internal void Update()
        {
            TimeContainer timeContainer = new TimeContainer()
            {
                DeltaTime = (float)PipelineTime.DeltaTime,
                TimePassed = (float)PipelineTime.TimePassed
            };
            m_timeBuffer.UpdateData(timeContainer);
            m_timeBuffer.UpdateBuffer();

            m_internalGraphics.Update();
        }

        public void Dispose ()
        {
            m_internalGraphics.Dispose();

            m_transformBuffer.Dispose();
            m_cameraBuffer.Dispose();
            m_lightBuffer.Dispose();
            m_timeBuffer.Dispose();
        }
    }
}