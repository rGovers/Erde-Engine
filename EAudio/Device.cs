using OpenTK;
using OpenTK.Audio.OpenAL;
using System;

namespace Erde.Audio
{
    public class Device
    {
        static Device m_activeDevice;

        public static Device Active
        {
            get
            {
                return m_activeDevice;
            }
        }

        IntPtr        m_device;
        ContextHandle m_context;

        unsafe void InitContext ()
        {
            m_device = Alc.OpenDevice(null);
            m_context = Alc.CreateContext(m_device, (int*)null);
        }

        public Device ()
        {
            if (m_activeDevice == null)
            {
                m_activeDevice = this;
            }

            InitContext();

            InternalConsole.AddMessage(string.Format("Init OpenAL: Version: {0}, Vendor {1}, Renderer {2}", AL.Get(ALGetString.Version), AL.Get(ALGetString.Vendor), AL.Get(ALGetString.Renderer)));
        }
    }
}
