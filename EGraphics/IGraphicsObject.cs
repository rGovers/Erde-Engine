using System;

namespace Erde.Graphics
{
    public interface IGraphicsObject : IDisposable
    {
        void ModifyObject ();

        void DisposeObject ();
    }
}