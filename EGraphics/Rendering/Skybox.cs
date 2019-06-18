namespace Erde.Graphics.Rendering
{
    // Just a container the logic is in the rendering pipeline
    public class Skybox
    {
        Material m_material;

        public Material Material
        {
            get
            {
                return m_material;
            }
        }

        public Skybox (Material a_material)
        {
            m_material = a_material;
        }
    }
}