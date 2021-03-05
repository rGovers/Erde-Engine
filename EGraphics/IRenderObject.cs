using System.Collections.Generic;
using Erde.Graphics.Rendering;

namespace Erde.Graphics
{
    public interface IRenderObject
    {
        void AddObject (LinkedList<DrawingContainer> a_objects, LinkedList<Renderer> a_renderers);
        void RemoveObject (LinkedList<DrawingContainer> a_objects, LinkedList<Renderer> a_renderers);
    }
}
