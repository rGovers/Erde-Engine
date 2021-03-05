using Erde.Application.Internal;
using OpenTK;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.Threading;

namespace Erde.Application
{
    public enum e_ApplicationType
    {
        Managed,
        Native
    }

    public enum e_WindowFlags
    {
        Default,
        FixedWindow,
        Fullscreen
    }

    public enum e_WindowState
    {
        Fullscreen,
        Maximized,
        Minimized,
        Normal
    }

    public class Application : IDisposable
    {
        public delegate void UpdateCycle ();
        public delegate void Event();

        static Application Instance = null;
        
        bool               m_shutdown;

        UpdateCycle        m_update;
        Event              m_closing;
        Event              m_resize;

        Input              m_input;

        IApplication       m_internalApplication;

        string             m_title;

        e_ApplicationType  m_applicationType;

        public string Title
        {
            get
            {
                return m_title;
            }
        }

        public e_ApplicationType ApplicationType
        {
            get
            {
                return m_applicationType;
            }
        }

        public IApplication InternalApplication
        {
            get
            {
                return m_internalApplication;
            }
        }

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

        public Event Closing
        {
            get
            {
                return m_closing;
            }
            set
            {
                m_closing = value;
            }
        }
        public Event Resize
        {
            get
            {
                return m_resize;
            }
            set
            {
                m_resize = value;
            }
        }

        public static Application Active
        {
            get
            {
                return Instance;
            }
        }

        public int Width
        {
            get
            {
                return m_internalApplication.Width;
            }
        }
        public int Height
        {
            get
            {
                return m_internalApplication.Height;
            }
        }

        public void ResizeWindow(Vector2 a_size)
        {
            m_internalApplication.ResizeWindow(a_size);
        }

        public Application (string a_title, e_WindowFlags a_displayMode, e_ApplicationType a_applicationType = e_ApplicationType.Managed)
        {
#if DEBUG_INFO
            Debug.Assert(Instance == null);
#endif
            Instance = this;

            m_title = a_title;

            m_applicationType = a_applicationType;

            switch (m_applicationType)
            {
                case e_ApplicationType.Managed:
                {
                    GameWindowFlags flags = GameWindowFlags.Default;

                    switch (a_displayMode)
                    {
                        case e_WindowFlags.FixedWindow:
                        {
                            flags = GameWindowFlags.FixedWindow;

                            break;
                        }
                        case e_WindowFlags.Fullscreen:
                        {
                            flags = GameWindowFlags.Fullscreen;

                            break;
                        }
                    }

                    m_internalApplication = new OpenTKApplication(this, a_title, flags);

                    break;
                }
                case e_ApplicationType.Native:
                {
                    m_internalApplication = new NativeApplication(this, a_title, a_displayMode);

                    break;
                }
            }

            m_shutdown = false;

            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            m_input = new Input(this);
        }

        internal void ResizeWindowEvent(object a_sender, EventArgs a_e)
        {
            if (m_resize != null)
            {
                m_resize();
            }
        }
        internal void CloseWindowEvent (object a_sender, EventArgs a_e)
        {
            m_shutdown = true;

            if (m_closing != null)
            {
                m_closing();
            }
        }

        public void Close()
        {
            m_shutdown = true;

            m_internalApplication.Close();
        }

        public void SetCursor(Cursor a_cursor)
        {
            m_internalApplication.SetCursor(a_cursor);
        }

        public Vector2 PointToClient(Vector2 a_point)
        {
            return m_internalApplication.PointToClient(a_point);
        }

        public void Run ()
        {
            Time time = new Time();

            while (m_internalApplication.WindowExists && !m_shutdown)
            {
                time.Update();

                if (m_internalApplication.State != e_WindowState.Minimized)
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
                    Thread.Yield();
                }

                m_internalApplication.Update();
            }
        }

        public void Dispose()
        {
            m_internalApplication.Dispose();
        }
    }
}