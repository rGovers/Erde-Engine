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

        internal static Frame ReadStream (Stream a_stream)
        {
            Frame frame = new Frame();

            uint length;

            bool littleEndian = BitConverter.IsLittleEndian;

            byte[] bytes = new byte[4];

            int count = a_stream.Read(bytes, 0, 4);
            if (count != 4)
            {
                return null;
            }

            if (!littleEndian)
            {
                Array.Reverse(bytes);
            }
            frame.m_opCode = (e_OpCode)BitConverter.ToUInt32(bytes, 0);

            count = a_stream.Read(bytes, 0, 4);
            if (count != 4)
            {
                return null;
            }

            if (!littleEndian)
            {
                Array.Reverse(bytes);
            }
            length = BitConverter.ToUInt32(bytes, 0);

            frame.m_data = new byte[length];

            uint bytesRead = 0;
            while (bytesRead < length)
            {
                bytesRead += (uint)a_stream.Read(frame.m_data, 0, (int)Math.Min(int.MaxValue, length - bytesRead));
            }

            return frame;
        }
        internal void WriteToStream (Stream a_stream)
        {
            byte[] op = ConvertBytes((uint)m_opCode);
            byte[] length = ConvertBytes((uint)m_data.LongLength);

            byte[] buffer = new byte[op.Length + length.Length + m_data.LongLength];
            op.CopyTo(buffer, 0);
            length.CopyTo(buffer, op.Length);
            m_data.CopyTo(buffer, op.Length + length.Length);

            a_stream.Write(buffer, 0, buffer.Length);
        }
    }
}
