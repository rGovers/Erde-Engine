using Erde.Application;
using Erde.Graphics.Internal.Lights;
using Erde.Graphics.Rendering;
using Erde.Graphics.Shader;
using Erde.Graphics.Variables;
using OpenTK;
using System;
using System.Runtime.InteropServices;

namespace Erde.Graphics.Lights
{
    public class DirectionalLight : Light, IGraphicsObject
    {
        static Material MaterialInstance;
        static Program  ProgramInstance;

        Pipeline        m_pipeline;

        Matrix4[]       m_view;
        Matrix4[]       m_projection;

        ILight          m_internalObject;

        public const int MapResolution = 2048;

        public static Material LightMaterial
        {
            get
            {
                return MaterialInstance;
            }
            set
            {
                MaterialInstance = value;
            }
        }

        public override int MapCount
        {
            get
            {
                return 8;
            }
        }

        public override Program ShadowProgram
        {
            get
            {
                return ProgramInstance;
            }
        }

        public override void CalculateSplits(Camera a_camera)
        {
            m_internalObject.CalculateSplits(a_camera);
        }

        public override void BindShadowMap (BindableContainer a_bindableContainer)
        {
            m_internalObject.BindShadowMap(a_bindableContainer);
        }
        public override Frustum BindShadowDrawing (int a_index, Camera a_camera)
        {
            return m_internalObject.BindShadowDrawing (a_index, a_camera);
        }

        public override Material BindLightDrawing()
        {
            return m_internalObject.BindLightDrawing();
        }
        public override Graphics.LightContainer GetLightData()
        {
            return m_internalObject.GetLightData();
        }

        public static void ClearAssets ()
        {
            if (ProgramInstance != null)
            {
                ProgramInstance.VertexShader.Dispose();

                ProgramInstance.Dispose();
                ProgramInstance = null;
            }
        }

        public DirectionalLight (bool a_shadowMap, Pipeline a_pipeline) : base(a_shadowMap)
        {
            m_pipeline = a_pipeline;

            int mapCount = MapCount;

            m_view = new Matrix4[mapCount];
            m_projection = new Matrix4[mapCount];

            if (a_pipeline.ApplicationType == e_ApplicationType.Managed)
            {
                m_internalObject = new OpenTKDirectionalLight(this);
            }
            else
            {
                
            }

            if (ShadowMapped)
            {
                if (ProgramInstance == null)
                {
                    ModelVertexInfo[] vertexLayout = ModelVertexInfo.GetVertexInfo<Vertex>();
                    int size = Marshal.SizeOf<Vertex>();

                    ProgramInstance = new Program(null, new VertexShader(Shaders.DIRECTIONAL_VERTEX, m_pipeline), vertexLayout, size, false, e_CullingMode.Back, m_pipeline);
                }

                Far = 65;

                m_pipeline.AddObject(this);
            }
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.RemoveObject(this);
        }

        ~DirectionalLight ()
        {
            Dispose(false);
        }

        public override void Dispose ()
        {
            base.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void SetViewProjection(Matrix4 a_view, Matrix4 a_proj, int a_index)
        {
            m_view[a_index] = a_view;
            m_projection[a_index] = a_proj;
        }

        public override Matrix4 GetView(int a_index)
        {
            return m_view[a_index];
        }
        public override Matrix4 GetProjection(int a_index)
        {
            return m_projection[a_index];
        }

        public void ModifyObject ()
        {
            m_internalObject.ModifyObject();
        }

        public void DisposeObject ()
        {
            m_internalObject.DisposeObject();
        }
    }
}