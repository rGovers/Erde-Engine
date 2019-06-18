using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Erde
{
    public class Reflection
    {
        public static void CallTypeFunction (object a_base, string a_functionString, object a_object)
        {
            CallTypeFunction(a_base, a_functionString, a_object.GetType(), a_object);
        }
        public static void CallTypeFunction (object a_base, string a_functionString, Type a_type, object a_object)
        {
            string type = a_type.ToString();

            int index = type.LastIndexOf('.');
            if (index != -1)
            {
                type = type.Remove(0, index + 1);
            }

            type = type.Replace('+', '_');

            MethodInfo method = a_base.GetType().GetMethod(a_functionString + type, BindingFlags.NonPublic | BindingFlags.Instance);
#if DEBUG_INFO
            if (method != null)
            {
                method.Invoke(a_base, new object[] { a_object });
            }
            else
            {
                InternalConsole.AddMessage(a_base.GetType().ToString() + ": No Function Found: " + type, InternalConsole.e_Alert.Error);
            }
#else
            method.Invoke(a_base, new object[] { a_object });
#endif
        }

        public static byte[] StructureToByte<T> (T a_object, int a_offset)
        {
            int size = Marshal.SizeOf(a_object);

            byte[] bytes = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(a_object, ptr, true);
            Marshal.Copy(ptr, bytes, a_offset, size);
            Marshal.FreeHGlobal(ptr);

            return bytes;
        }
        public static byte[] StructureToByte<T> (T a_object)
        {
            return StructureToByte(a_object, 0);
        }
        public static byte[] StructureToByte<T> (T[] a_object)
        {
            return StructureToByte(a_object, Marshal.SizeOf<T>());
        }
        public static byte[] StructureToByte<T> (T[] a_object, int a_size, int a_offset)
        {
            byte[] bytes = new byte[a_size * a_object.Length];

            for (int i = 0; i < a_object.Length; ++i)
            {
                IntPtr ptr = Marshal.AllocHGlobal(a_size);
                Marshal.StructureToPtr(a_object[i], ptr, true);
                Marshal.Copy(ptr, bytes, a_offset + i * a_size, a_size);
                Marshal.FreeHGlobal(ptr);
            }

            return bytes;
        }

        public static void ByteToStructure<T> (byte[] a_bytes, out T a_object)
        {
            ByteToStructure<T>(a_bytes, 0, out a_object);
        }
        public static void ByteToStructure<T> (byte[] a_bytes, int a_offset, out T a_object)
        {
            int size = Marshal.SizeOf<T>();

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(a_bytes, a_offset, ptr, size);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            a_object = obj;
        }
        public static void ByteToStructure<T> (byte[] a_bytes, int a_offset, out T a_object, int a_size)
        {
            IntPtr ptr = Marshal.AllocHGlobal(a_size);
            Marshal.Copy(a_bytes, a_offset, ptr, a_size);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            a_object = obj;
        }
        public static void ByteToStructure<T> (byte[] a_bytes, T[] a_object)
        {
            int size = Marshal.SizeOf<T>();

            for (int i = 0; i < a_object.Length; ++i)
            {
                ByteToStructure(a_bytes, i * size, out a_object[i]);
            }
        }
        public static void ByteToStructure<T> (byte[] a_bytes, T[] a_object, int a_size)
        {
            for (int i = 0; i < a_object.Length; ++i)
            {
                ByteToStructure(a_bytes, i * a_size, out a_object[i], a_size);
            }
        }

        public static T GetMainAssemblyFunction<T> (string a_type, string a_method)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
           
            Type type = Type.GetType(string.Format("{0},{1}", a_type, assembly.FullName), false);

            if (type != null)
            {
                Type tType = typeof(T);

                MethodInfo methodInfo = type.GetMethod(a_method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (methodInfo != null)
                {
                    return (T)Convert.ChangeType(methodInfo.CreateDelegate(tType), tType);
                }
            }

            return default(T);
        }
    }
}