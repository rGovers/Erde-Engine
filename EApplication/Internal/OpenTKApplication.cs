using OpenTK;
using OpenTK.Graphics;
using System.Drawing;

namespace Erde.Application.Internal
{
    public class OpenTKApplication : NativeWindow, IApplication
    {
        Application m_application;

        public bool WindowExists
        {
            get
            {
                return Exists;
            }
        }

        public e_WindowState State
        {
            get
            {
                switch (base.WindowState)
                {
                    case WindowState.Fullscreen:
                    {
                        return e_WindowState.Fullscreen;
                    }
                    case WindowState.Maximized:
                    {
                        return e_WindowState.Maximized;
                    }
                    case WindowState.Minimized:
                    {
                        return e_WindowState.Minimized;
                    }
                }

                return e_WindowState.Normal;
            }
        }

        public OpenTKApplication(Application a_application, string a_title, GameWindowFlags a_displayMode) 
            : base(DisplayDevice.Default.Width / 2, DisplayDevice.Default.Height / 2, a_title, a_displayMode, GraphicsMode.Default, DisplayDevice.Default)
        {
            m_application = a_application;

            WindowState = WindowState.Normal;
            // Fixes window drawing issues on Linux
            Visible = true;

            // Fixes weird bug with cursor being seen as visible and invisible
            CursorVisible = true;

            Closing += m_application.CloseWindowEvent;
            Resize += m_application.ResizeWindowEvent;
        }   

        public void Update()
        {
            ProcessEvents();
        }

        public void SetCursor(Cursor a_cursor)
        {
            Cursor = new MouseCursor(a_cursor.ActiveX, a_cursor.ActiveY, a_cursor.Width, a_cursor.Height, a_cursor.Bytes);
        }

        public Vector2 PointToClient(Vector2 a_point)
        {
            Point point = PointToClient(new Point((int)a_point.X, (int)a_point.Y));

            return new Vector2(point.X, point.Y);
        }

        public void ResizeWindow(Vector2 a_size)
        {
            Width = (int)a_size.X;
            Height = (int)a_size.Y;
        }
    }
}