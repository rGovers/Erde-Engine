using System;
using System.Runtime.InteropServices;

namespace Erde
{
    public interface IDistanceField
    {
        int Width
        {
            get;
        }
        int Height
        {
            get;
        }
        int Depth
        {
            get;
        }

        float Spacing
        {
            get;
        }

        int Stride
        {
            get;
        }

        IntPtr CellPtr
        {
            get;
        }

        float GetDistance (int a_x, int a_y, int a_z);
        void SetDistance (int a_x, int a_y, int a_z, float a_distance);
    }

    public interface IDistanceCell
    {
        float Distance
        {
            get;
            set;
        }
    }

    

    public class DistanceField<T> : IDistanceField where T : struct
    {
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Cell
        {
            public float Distance;

            public T Data;
            
            public static int DataSize
            {
                get
                {
                    return sizeof(float) + Marshal.SizeOf<T>();
                }
            }

            public byte[] GetBytes ()
            {
                 const int offset = sizeof(float);
                 int dataSize = Marshal.SizeOf<T>();
                 int size = offset + dataSize;

                 byte[] bytes = new byte[size];

                 Array.Copy(BitConverter.GetBytes(Distance), bytes, offset);
                 Array.Copy(Reflection.StructureToByte(Data), 0, bytes, offset, dataSize);

                 return bytes;
            }

            public static Cell FromBytes (byte[] a_bytes, int a_offset = 0)
            {
                 T data;
                 Reflection.ByteToStructure(a_bytes, a_offset + sizeof(float), out data);

                 return new Cell()
                 {
                     Distance = BitConverter.ToSingle(a_bytes, a_offset),
                     Data = data
                 };
            }
        }
        int    m_width;
        int    m_height;
        int    m_depth;
               
        float  m_spacing;

        Cell[] m_distanceCell;

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
        public int Depth
        {
            get
            {
                return m_depth;
            }
        }

        public float Spacing
        {
            get
            {
                return m_spacing;
            }
        }

        public Cell[] Cells
        {
            get
            {
                return m_distanceCell;
            }
        }

        public int Stride
        {
            get
            {
                return sizeof(float) + Marshal.SizeOf<T>();
            }
        }

        // Future me problem I do not want to deal with this 
        public IntPtr CellPtr
        {
            get
            {
                return Marshal.UnsafeAddrOfPinnedArrayElement(m_distanceCell, 0);
            }
        }

        public DistanceField ()
        {
        }

        public DistanceField (int a_width, int a_height, int a_depth, float a_spacing)
        {
            m_width = a_width;
            m_height = a_height;
            m_depth = a_depth;

            m_spacing = a_spacing;

            m_distanceCell = new Cell[m_width * m_height * m_depth];
        }

        public float GetDistance (int a_x, int a_y, int a_z)
        {
            return m_distanceCell[(a_z * m_width * m_height) + (a_y * m_width) + a_x].Distance;
        }
        public void SetDistance (int a_x, int a_y, int a_z, float a_distance)
        {
            m_distanceCell[(a_z * m_width * m_height) + (a_y * m_width) + a_x].Distance = a_distance;
        }

        public Cell GetCell (int a_x, int a_y, int a_z)
        {
            return m_distanceCell[(a_z * m_width * m_height) + (a_y * m_width) + a_x];
        }

        public void SetCell (int a_x, int a_y, int a_z, Cell a_data)
        {
            m_distanceCell[(a_z * m_width * m_height) + (a_y * m_width) + a_x] = a_data;
        }
    }
}