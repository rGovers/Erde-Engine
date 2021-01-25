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

        public abstract Matrix4 View
        {
            get;
        }

        public abstract Matrix4 Projection
        {
            get;
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
#if DEBUG
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

        public abstract void BindShadowMap (BindableContainer a_bindableContainer);

        public abstract void BindShadowDrawing ();

        public static LinkedList<Light> LightList
        {
            get
            {
                return m_lights;
            }
        }
    }
}