using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Erde.Discord.Client
{
    internal class WindowsClient : InternalClient
    {
        byte[]                    m_buffer;

        NamedPipeClientStream     m_stream;

        public override bool IsConnected
        {
            get
            {
                return m_stream.IsConnected;
            }
        }

        internal WindowsClient(DiscordClient a_client) :
            base(a_client) 
        {
            m_buffer = new byte[1024 * 16];
        }

        public override void BeginRead ()
        {
            if (!m_stream.IsConnected)
            {
                return;
            }

            try
            {
                m_stream.BeginRead(m_buffer, 0, m_buffer.Length, EndRead, m_stream.IsConnected);
            }
            catch (ObjectDisposedException)
            {
                InternalConsole.Error("Discord Client: Attempted to read from disposed stream");

                return;
            }
            catch (InvalidOperationException)
            {
                InternalConsole.Error("Discord Client: Attempted to read from closed pipe");

                return;
            }
        }
        void EndRead (IAsyncResult a_callback)
        {
            int bytes = -1;

            try
            {
                bytes = m_stream.EndRead(a_callback);
            }
            catch (IOException)
            {
                InternalConsole.Error("Discord Client: Attempted to read from closed pipe");

                return;
            }
            catch (ObjectDisposedException)
            {
                InternalConsole.Error("Discord Client: Attempted to end reading from a disposed pipe");

                return;
            }
            catch (NullReferenceException)
            {
                InternalConsole.Error("Discord Client: Attempted to connect to null pipe");

                return;
            }

            if (bytes > 0)
            {
                Frame frame = Frame.FromBytes(m_buffer);

                if (frame != null)
                {
                    Client.EnqueueFrame(frame);
                }
            }

            if (m_stream.IsConnected)
            {
                BeginRead();
            }
        }

        public override bool AttemptConnection (string a_pipe)
        {
            try
            {
                m_stream = new NamedPipeClientStream(".", a_pipe, PipeDirection.InOut, PipeOptions.Asynchronous);
                m_stream.Connect(1000);

                do
                {
                    Thread.Sleep(10);
                }
                while (!m_stream.IsConnected);

                InternalConsole.AddMessage(string.Format("Connected: {0}", a_pipe));

                return true;
            }
            catch (Exception e)
            {
                InternalConsole.Error(string.Format("Failed to connect to {0}: {1}", a_pipe, e.Message));
            }

            return false;
        }

        public override void WriteToStream(byte[] a_bytes)
        {
            m_stream.Write(a_bytes, 0, a_bytes.Length);
        }
        public override byte[] FromStream()
        {
            bool littleEndian = BitConverter.IsLittleEndian;

            byte[] opCode = new byte[4];

            int count = m_stream.Read(opCode, 0, 4);
            if (count != 4)
            {
                return null;
            }

            byte[] length = new byte[4];
            count = m_stream.Read(length, 0, 4);

            if (!littleEndian)
            {
                Array.Reverse(opCode);
                Array.Reverse(length);
            }

            uint dataLen = BitConverter.ToUInt32(length, 0);

            byte[] bytes = new byte[dataLen];
            uint bytesRead = 0;

            while (bytesRead < dataLen)
            {
                bytesRead += (uint)m_stream.Read(bytes, 0, (int)Math.Min(int.MaxValue, dataLen - bytesRead));
            }

            byte[] data = new byte[dataLen + 8];
            Array.Copy(opCode, 0, data, 0, 4); 
            Array.Copy(length, 0, data, 4, 4);
            Array.Copy(bytes, 0, data, 8, dataLen);

            return data;
        }
    }
}