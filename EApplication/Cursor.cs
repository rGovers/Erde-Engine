namespace Erde.Application
{
    public class Cursor
    {
        int m_width;
        int m_height;

        int m_activeX;
        int m_activeY;

        byte[] m_bytes;

        public int Width
        {
            get
            {
                return m_width;
            }
        }
        public int Height
        {
            get
            {
                return m_height;
            }
        }

        public int ActiveX
        {
            get
            {
                return m_activeX;
            }
        }
        public int ActiveY
        {
            get
            {
                return m_activeY;
            }
        }

        public byte[] Bytes
        {
            get
            {
                return m_bytes;
            }
        }

        public Cursor(int a_width, int a_height, int a_activeX, int a_activeY, byte[] a_bytes)
        {
            m_width = a_width;
            m_height = a_height;

            m_activeX = a_activeX;
            m_activeY = a_activeY;

            m_bytes = a_bytes;
        }
    }
}