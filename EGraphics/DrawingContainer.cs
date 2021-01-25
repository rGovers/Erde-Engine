using Erde.Graphics.Rendering;
using System.Collections.Generic;

namespace Erde.Graphics
{
    public class DrawingContainer
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
}