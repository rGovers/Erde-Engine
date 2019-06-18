using System;

namespace Erde.Graphics
{
    public interface IGLObject : IDisposable
    {
        void ModifyObject ();

        void DisposeObject ();
    }
}