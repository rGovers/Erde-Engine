using Erde.Graphics.Rendering;
using Erde.Graphics.Shader;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Erde.Graphics.Lights
{
    public abstract class Light : GameObject
    {
        static LinkedList<Light> m_lights;

        Color                    m_color;

        bool                     m_shadowMap;

        float                    m_far;

        bool                     m_draw;

        public float Far
        {
            get
            {
                return m_far;
            }
            set
            {
                m_far = value;
            }
        }

        public abstract Program ShadowProgram
        {
            get;
        }

        public Matrix4 View
        {
            get
            {
                return GetView(0);
            }
        }

        public Matrix4 Projection
        {
            get
            {
                return GetProjection(0);
            }
        }

        public abstract int MapCount
        {
            get;
        }

        public static LinkedList<Light> LightList
        {
            get
            {
                return m_lights;
            }
        }

        public bool ShadowMapped
        {
            get
            {
                return m_shadowMap;
            }
        }

        public bool Draw
        {
            get
            {
                return m_draw;
            }
            set
            {
                m_draw = value;
            }
        }

        public Color Color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        public Light (bool a_shadowMap)
        {
            m_shadowMap = a_shadowMap;
            m_draw = true;

            if (m_lights == null)
            {
                m_lights = new LinkedList<Light>();
            }

            lock (m_lights)
            {
                m_lights.AddLast(this);
            }
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            lock (m_lights)
            {
                m_lights.Remove(this);
            }
        }

        ~Light()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            base.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void CalculateSplits(Camera a_camera);

        public abstract void BindShadowMap (BindableContainer a_bindableContainer);

        public abstract Frustum BindShadowDrawing (int a_index, Camera a_camera);

        public abstract Material BindLightDrawing ();
        public abstract Graphics.LightContainer GetLightData ();

        public abstract Matrix4 GetView(int a_index);
        public abstract Matrix4 GetProjection(int a_index);
    }
}