using System;

namespace Erde.Graphics
{
    public interface IPipeline : IDisposable
    {
        void Shutdown();

        void AddObject(IGraphicsObject a_object);
        void RemoveObject(IGraphicsObject a_object);

        Graphics GraphicsPipeline
        {
            get;
        }
    }
}