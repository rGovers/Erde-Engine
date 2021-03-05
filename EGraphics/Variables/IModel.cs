namespace Erde.Graphics.Variables
{
    public interface IModel : IGraphicsObject
    {
        void SetData<T>(T[] a_data, uint[] a_indices, ModelVertexInfo[] a_vertexInfo) where T : struct;
        void Bind();
    }
}