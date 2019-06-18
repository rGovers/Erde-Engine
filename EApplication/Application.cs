using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Threading;

namespace Erde.Application
{
    public class Application : NativeWindow
    {
        public delegate void UpdateCycle ();

        bool               m_shutdown;

        static Application m_application = null;

        UpdateCycle        m_update;

        Input              m_input;

        public UpdateCycle Update
        {
            get
            {
                return m_update;
            }
            set
            {
                m_update = value;
            }
        }

        public static Application Active
        {
            get
            {
                return m_application;
            }
        }

        // Creates the window
        public Application (string a_title, GameWindowFlags a_displayMode)
            : base(DisplayDevice.Default.Width / 2, DisplayDevice.Default.Height / 2, a_title, a_displayMode, GraphicsMode.Default, DisplayDevice.Default)
        {
            WindowState = WindowState.Normal;
            // Fixes window drawing issues on Linux
            Visible = true;

            // Fixes wierd bug with cursor being seen as visible and invisible
            CursorVisible = true;

            m_shutdown = false;

            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            if (m_application == null)
            {
                m_application = this;
            }

            m_input = new Input(this);
        }

        void Application_Closing (object sender, EventArgs e)
        {
            m_shutdown = true;
        }

        public void SetActive ()
        {
            m_application = this;
        }

        public void Run ()
        {
            Closing += Application_Closing;

            Time time = new Time();

            while (Exists && !m_shutdown)
            {
                time.Update();

                if (WindowState != WindowState.Minimized)
                {
                    m_input.Update();

                    switch (Environment.OSVersion.Platform)
                    {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        {
                            // For some reason the library does not handle alt+f4 properly and I can not be bothered writing another message loop
                            // This seems to occur only on windows
                            KeyboardState kState = Keyboard.GetState();

                            if (kState.IsConnected && kState.IsKeyDown(Key.F4) && (kState.IsKeyDown(Key.AltLeft) || kState.IsKeyDown(Key.AltRight)))
                            {
                                Close();
                            }

                            break;
                        }
                    }

                    m_update.Invoke();
                }
                else
                {
                    Thread.Sleep(100);
                }

                ProcessEvents();
            }
        }
    }
}