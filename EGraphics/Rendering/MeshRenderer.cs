using Erde.Graphics.Lights;
using Erde.Graphics.Variables;

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
                lock (this)
                {
                    m_model = value;
                }
            }
        }

        public override uint Indices
        {
            get
            {
                if (m_model != null)
                {
                    lock (this)
                    {
                        return m_model.Indices;
                    }
                }

                return 0;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible && Indices > 0; 
            }
        }

        public override float Radius
        {
            get
            {
                return m_model.Radius;
            }
        }

        public MeshRenderer ()
            : base()
        {
        }

        public override void Draw (Camera a_camera)
        {
            if (m_model != null)
            {
                lock (this)
                {
                    GraphicsCommand.BindModel(m_model);
                    
                    GraphicsCommand.DrawElements(m_model.Indices);
                }
            }
        }

        public override void DrawShadow (Light a_light)
        {
            if (m_model != null)
            {
                lock (this)
                {
                    GraphicsCommand.BindModel(m_model);
                    
                    GraphicsCommand.DrawElements(m_model.Indices);
                }
            }
        }
    }
}