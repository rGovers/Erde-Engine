using Erde.Graphics.Shader;

namespace Erde.Graphics
{
    public class Binding
    {
        int               m_bindingIndex;
        IMaterialBindable m_object;

        Program           m_program;

        internal int Handle
        {
            get
            {
                return m_bindingIndex;
            }
            set
            {
                m_bindingIndex = value;
            }
        }

        internal Program Program
        {
            get
            {
                return m_program;
            }
        }

        public IMaterialBindable Target
        {
            get
            {
                return m_object;
            }
            set
            {
                m_object = value;
            }
        }

        public Binding (int a_binding, IMaterialBindable a_object, Program a_program)
        {
            m_bindingIndex = a_binding;
            m_program = a_program;
            m_object = a_object;
        }
    }
}
    