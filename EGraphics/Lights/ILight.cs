using Erde.Graphics;

namespace Erde.Graphics.Lights
{
    public interface ILight : IGraphicsObject
    {
        void BindShadowMap (BindableContainer a_bindableContainer);
        void BindShadowDrawing ();
    }
}