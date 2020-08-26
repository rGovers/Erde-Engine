using Erde.Graphics.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Erde.Graphics.Variables
{
    public class UnsignedIntGValue : IMaterialBindable
    {
        uint     m_value;

        public uint Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        public void Bind (BindableContainer a_container, Material.Binding a_binding)
        {
            if (a_binding.Handle != -1)
            {
                GL.Uniform1(a_binding.Handle, m_value);
            }
        }
    }
}
