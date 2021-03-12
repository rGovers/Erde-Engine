using OpenTK;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Erde.Graphics
{
    public enum e_FieldType : int
    {
        Float = 0,
        Int = 1,
        UnsignedInt = 2,
        UnsignedByte = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ModelVertexInfo
    {
        public IntPtr Offset;
        public uint Count;
        public e_FieldType Type;
        public bool Normalize;

        public static ModelVertexInfo[] GetVertexInfo<T>()
        {
            List<ModelVertexInfo> vertexInfoCollection = new List<ModelVertexInfo>();

            Type type = typeof(T);

            FieldInfo[] fieldInfo = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fieldInfo)
            {
                Type fieldType = field.FieldType;

                if (fieldType == typeof(float))
                {
                    vertexInfoCollection.Add(new ModelVertexInfo()
                    {
                        Offset = Marshal.OffsetOf<T>(field.Name),
                        Count = 1,
                        Type = e_FieldType.Float,
                        Normalize = false
                    });
                } 
                else if (fieldType == typeof(int))
                {
                    vertexInfoCollection.Add(new ModelVertexInfo()
                    {
                        Offset = Marshal.OffsetOf<T>(field.Name),
                        Count = 1,
                        Type = e_FieldType.Int,
                        Normalize = false
                    });
                }  
                else if (fieldType == typeof(Vector2))
                {
                    vertexInfoCollection.Add(new ModelVertexInfo()
                    {
                        Offset = Marshal.OffsetOf<T>(field.Name),
                        Count = 2,
                        Type = e_FieldType.Float,
                        Normalize = false
                    });
                }
                else if (fieldType == typeof(Vector3))
                {
                    vertexInfoCollection.Add(new ModelVertexInfo()
                    {
                        Offset = Marshal.OffsetOf<T>(field.Name),
                        Count = 3,
                        Type = e_FieldType.Float,
                        Normalize = false
                    });
                }
                else if (fieldType == typeof(Vector4))
                {
                    vertexInfoCollection.Add(new ModelVertexInfo()
                    {
                        Offset = Marshal.OffsetOf<T>(field.Name),
                        Count = 4,
                        Type = e_FieldType.Float,
                        Normalize = false
                    });
                }
                else if (fieldType == typeof(uint))
                {
                    vertexInfoCollection.Add(new ModelVertexInfo()
                    {
                        Offset = Marshal.OffsetOf<T>(field.Name),
                        Count = 1,
                        Type = e_FieldType.UnsignedInt,
                        Normalize = false
                    });
                }
            }

            return vertexInfoCollection.ToArray();
        }
    }
}