using Erde.Graphics.Lights;
using Erde.Graphics.Variables;

namespace Erde.Graphics.Rendering
{
    public class MaterialRenderer : Renderer
    {
        public override uint Indices
        {
            get
            {
                return 6;
            }
        }

        public override float Radius
        {
            get
            {
                return -1;
            }
        }

        public MaterialRenderer ()
            : base()
        {
        }

        public override void Draw (Camera a_camera)
        {
            GraphicsCommand.Draw();
        }

        public override void DrawShadow (Light a_light)
        {
            
        }
    }
}