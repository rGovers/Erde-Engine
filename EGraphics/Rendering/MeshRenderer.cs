using Erde.Graphics.Lights;
using Erde.Graphics.Variables;
using OpenTK.Graphics.OpenGL;

namespace Erde.Graphics.Rendering
{
    public class MeshRenderer : Renderer
    {
        // Stores the mesh for drawing
        Model m_model;

        public Model Model
        {
            get
            {
                return m_model;
            }
            set
            {
                m_model = value;
            }
        }

        public override uint Indices
        {
            get
            {
                return m_model.Indices;
            }
        }

        public override float Radius
        {
            get
            {
                return m_model.Radius;
            }
        }

        public override void Draw (Camera a_camera)
        {
            m_model.Bind();

            GL.DrawElements(BeginMode.Triangles, (int)m_model.Indices, DrawElementsType.UnsignedInt, 0);
        }

        public override void DrawShadow (Light a_light)
        {
            m_model.Bind();

            GL.DrawElements(BeginMode.Triangles, (int)m_model.Indices, DrawElementsType.UnsignedInt, 0);
        }

        public MeshRenderer ()
            : base()
        {
        }
    }
}