namespace Erde.Graphics.Variables
{
    public interface IModel : IGraphicsObject
    {
        void SetData<T>(T[] a_data, ushort[] a_indices) where T : struct;
        void Bind();
    }
}