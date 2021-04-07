using System;

namespace Erde.Graphics.Variables
{
    public interface ITexture : IGraphicsObject
    {   
        bool Initialized
        {
            get;
        }

        void WriteData(IntPtr a_ptr, e_PixelType a_pixelType);
        void Bind(int a_slot, int a_index);
    }
}