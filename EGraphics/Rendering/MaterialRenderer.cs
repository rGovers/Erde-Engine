using Erde.Graphics.Lights;
using Erde.Graphics.Variables;

namespace Erde.Graphics.Rendering
{
    public enum e_MaterialDrawingMode
    {
        Triangles,
        TriangleStrip
    };

    public class MaterialRenderer : Renderer
    {
        uint                  m_indices;
        float                 m_radius;

        e_MaterialDrawingMode m_drawingMode;

        public override uint Indices
        {
            get
            {
                return m_indices;
            }
        }

        public e_MaterialDrawingMode DrawingMode
        {
            get
            {
                return m_drawingMode;
            }
            set
            {
                m_drawingMode = value;
            }
        }

        public override float Radius
        {
            get
            {
                return m_radius;
            }
        }
        public void SetRadius(float a_radius)
        {
            m_radius = a_radius;
        }

        public void SetIndices(uint a_indices)
        {
            m_indices = a_indices;
        }

        public MaterialRenderer ()
            : base()
        {
            m_indices = 4;
            m_radius = -1;
        }

        public override void Draw (Camera a_camera)
        {
            switch (m_drawingMode)
            {
                case e_MaterialDrawingMode.Triangles:
                {
                    GraphicsCommand.DrawTriangles(m_indices);

                    break;
                }
                default:
                {
                    GraphicsCommand.Draw(m_indices);

                    break;
                }
            }
        }

        public override void DrawShadow (Light a_light)
        {
            switch (m_drawingMode)
            {
                case e_MaterialDrawingMode.Triangles:
                {
                    GraphicsCommand.DrawTriangles(m_indices);

                    break;
                }
                default:
                {
                    GraphicsCommand.Draw(m_indices);

                    break;
                }
            }
        }
    }
}