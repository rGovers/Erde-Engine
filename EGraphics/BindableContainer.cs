using Erde.Graphics.Rendering;

namespace Erde.Graphics
{
    public class BindableContainer
    {
        int m_texture = 0;
        int m_ubo = 0;

        public int Textures
        {
            get
            {
                return m_texture;
            }
            set
            {
                m_texture = value;
            }
        }

        public int UniformBufferObjects
        {
            get
            {
                return m_ubo;
            }
            set
            {
                m_ubo = value;
            }
        }
    }

    public interface IMaterialBindable
    {
        void Bind (BindableContainer a_container, Material.Binding a_binding);
    }
}