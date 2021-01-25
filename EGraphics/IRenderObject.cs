using System.Collections.Generic;

namespace Erde.Graphics
{
    public interface IRenderObject
    {
        void AddObject (LinkedList<DrawingContainer> a_objects);
        void RemoveObject (LinkedList<DrawingContainer> a_objects);
    }
}
