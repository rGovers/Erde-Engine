using OpenTK;
using System;

namespace Erde.Application
{
    public interface IApplication : IDisposable
    {
        bool WindowExists
        {
            get;
        }

        int Width
        {
            get;
        }
        int Height
        {
            get;
        }

        e_WindowState State
        {
            get;
        }

        void Update();

        void Close();

        Vector2 PointToClient(Vector2 a_point);

        void ResizeWindow(Vector2 a_size);
    }
}