namespace Erde.Graphics.Variables
{
    public interface IUniformBufferObject : IGraphicsObject, IMaterialBindable
    {
        void UpdateData(object a_object);
        void UpdateBuffer();
    }
}