using Erde.Graphics;
using Erde.Graphics.Rendering;

namespace Erde.Graphics.Lights
{
    public interface ILight : IGraphicsObject
    {
        void BindShadowMap (BindableContainer a_bindableContainer);
        void BindShadowDrawing ();

        Material BindLightDrawing ();
        Graphics.LightContainer GetLightData ();
    }
}