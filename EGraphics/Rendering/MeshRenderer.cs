using Erde.Graphics.Lights;
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

        public override uint Indicies
        {
            get
            {
                return m_model.Indicies;
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
            GL.BindVertexArray(m_model.VertexArrayObject);
            // Draws the mesh
            GL.DrawElements(BeginMode.Triangles, m_model.Indicies, DrawElementsType.UnsignedShort, 0);

#if DEBUG_INFO
            Graphics.AddTriangles(m_model.Indicies, PrimitiveType.Triangles);
#endif
        }

        public override void DrawShadow (Light a_light)
        {
            GL.BindVertexArray(m_model.VertexArrayObject);
            // Draws the mesh
            GL.DrawElements(BeginMode.Triangles, m_model.Indicies, DrawElementsType.UnsignedShort, 0);

#if DEBUG_INFO
            Graphics.AddTriangles(m_model.Indicies, PrimitiveType.Triangles);
#endif
        }

        public MeshRenderer ()
            : base()
        {
        }

        public MeshRenderer (Model a_model, Material a_material, Graphics a_graphics)
            : this(a_model, a_material, null, a_graphics)
        {
        }

        public MeshRenderer (Model a_model, Material a_material, Transform a_anchor, Graphics a_graphics)
            : base(a_material, a_anchor, a_graphics)
        {
            m_model = a_model;
        }
    }
}