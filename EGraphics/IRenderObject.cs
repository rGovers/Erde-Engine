
using System.Collections.Generic;

namespace Erde.Graphics
{
    public interface IRenderObject
    {
        void AddObject (LinkedList<Graphics.DrawingContainer> a_objects);
        void RemoveObject (LinkedList<Graphics.DrawingContainer> a_objects);
    }
}
