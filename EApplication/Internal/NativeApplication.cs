using Erde;
using OpenTK;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Erde.Application.Internal
{
    public class NativeApplication : IApplication
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Event();

        static IntPtr Assembly = IntPtr.Zero;

        IntPtr      m_handle;

        Application m_application;

        public bool WindowExists
        {
            get
            {
                return !ApplicationShouldClose(m_handle);
            }
        }

        public int Width
        {
            get
            {
                return ApplicationWidth(m_handle);
            }
        }
        public int Height
        {
            get
            {
                return ApplicationHeight(m_handle);
            }
        }

        public e_WindowState State
        {
            get
            {
                return e_WindowState.Normal;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return m_handle;
            }
        }

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ApplicationNew(string a_title, int a_stateHint);        
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ApplicationDelete(IntPtr a_window);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern bool ApplicationShouldClose(IntPtr a_window);        

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern int ApplicationWidth(IntPtr a_window);
        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern int ApplicationHeight(IntPtr a_window);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void ApplicationResize(IntPtr a_window, int a_width, int a_height);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void ApplicationUpdate(IntPtr a_window, Event a_resize, Event a_close);

        [DllImport("ENativeApplication", CallingConvention = CallingConvention.Cdecl)]
        static extern void ApplicationClose(IntPtr a_window);

        public NativeApplication(Application a_application, string a_title, e_WindowFlags a_displayMode)
        {
            m_application = a_application;

            m_handle = ApplicationNew(a_title, (int)a_displayMode);
        }

        void ResizeApp()
        {
            m_application.ResizeWindowEvent(this, null);
        }
        void CloseApp()
        {
            m_application.CloseWindowEvent(this, null);
        }

        public void Update()
        {
            ApplicationUpdate(m_handle, ResizeApp, CloseApp);
        }

        public void Close()
        {
            ApplicationClose(m_handle);
        }

        public Vector2 PointToClient(Vector2 a_point)
        {
            return a_point;
        }

        public void ResizeWindow(Vector2 a_size)
        {
            ApplicationResize(m_handle, (int)a_size.X, (int)a_size.Y);
        }   

        public void Dispose()
        {
            ApplicationDelete(m_handle);
        }
    }
}