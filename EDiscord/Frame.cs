using Erde.Discord.Payload;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Erde.Discord
{
    public class Frame
    {
        // No I will not listen to your coding conventions Visual Studio 
        public enum e_OpCode
        {
            Null = -1,
            Handshake,
            Frame,
            Close,
            Ping,
            Pong
        };

        e_OpCode m_opCode;
        byte[]   m_data;

        public e_OpCode OpCode
        {
            get
            {
                return m_opCode;
            }
            internal set
            {
                m_opCode = value;
            }
        }

        public byte[] Data
        {
            get
            {
                return m_data;
            }
        }

        public Frame ()
        {
            m_opCode = e_OpCode.Null;
            m_data = null;
        }
        public Frame (e_OpCode a_OpCode, object a_data)
        {
            m_opCode = a_OpCode;
            SetObject(a_data);
        }

        byte[] ConvertBytes (uint a_uint)
        {
            byte[] bytes = BitConverter.GetBytes(a_uint);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        internal void SetObject (object a_obj)
        {
            string data = JsonConvert.SerializeObject(a_obj);

            m_data = Encoding.UTF8.GetBytes(data);
        }
        internal T GetObject<T> () where T : IPayload
        {
            string json = Encoding.UTF8.GetString(m_data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal static Frame FromBytes(byte[] a_bytes)
        {
            Frame frame = new Frame();

            frame.m_opCode = (e_OpCode)BitConverter.ToUInt32(a_bytes, 0);

            uint length = BitConverter.ToUInt32(a_bytes, 4);
            frame.m_data = new byte[length];

            frame.m_data = new byte[length];
            Array.Copy(a_bytes, 8, frame.m_data, 0, length);

            return frame;
        }

        internal byte[] ToBytes()
        {
            byte[] op = ConvertBytes((uint)m_opCode);
            long opLength = op.Length;

            byte[] length = ConvertBytes((uint)m_data.LongLength);
            long lengthLength = length.Length;

            byte[] buffer = new byte[opLength + lengthLength + m_data.LongLength];
            op.CopyTo(buffer, 0);
            length.CopyTo(buffer, opLength);
            m_data.CopyTo(buffer, opLength + lengthLength);

            return buffer;
        }
    }
}
