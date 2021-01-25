using Erde;
using Erde.Discord;
using System;
using System.Net.Sockets;

namespace Erde.Discord.Client
{
    internal class UnixClient : InternalClient
    {
        Socket m_socket;

        string m_envPath;

        

        public override bool IsConnected
        {
            get
            {
                return m_socket.Connected;
            }
        }

        internal UnixClient(DiscordClient a_client) :
            base(a_client) 
        { 
            // Nicked environment variables from discord api
            string env = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            env = !string.IsNullOrEmpty(env) ? env : Environment.GetEnvironmentVariable("TMPDIR");
            env = !string.IsNullOrEmpty(env) ? env : Environment.GetEnvironmentVariable("TMP");
            env = !string.IsNullOrEmpty(env) ? env : Environment.GetEnvironmentVariable("TEMP");
            m_envPath = !string.IsNullOrEmpty(env) ? env : "/tmp";
        }

        public override void BeginRead()
        {
            if (!m_socket.Connected)
            {
                return;
            }

            try
            {
                byte[] buffer = new byte[1024 * 16];

                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                eventArgs.SetBuffer(buffer, 0, buffer.Length);
                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(EndRead);

                m_socket.ReceiveAsync(eventArgs);
            }
            catch (Exception e)
            {
                InternalConsole.Error(string.Format("Discord Client: Reading socket: {0}", e.Message));

                return;
            }
        }

        void EndRead(object a_sender, SocketAsyncEventArgs a_e)
        {
            int bytesTransferred = a_e.BytesTransferred;

            if (bytesTransferred > 0 && a_e.SocketError == SocketError.Success)
            {
                Frame frame = Frame.FromBytes(a_e.Buffer);

                if (frame != null)
                {
                    Client.EnqueueFrame(frame);
                }
            }
        }

        public override bool AttemptConnection (string a_pipe)
        {
            m_socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            m_socket.Blocking = false;

            UnixEndPoint endPoint = new UnixEndPoint(string.Format("{0}/{1}", m_envPath, a_pipe));
            try
            {
                m_socket.Connect(endPoint);
            }
            catch (Exception e)
            {
                InternalConsole.Error(string.Format("Failed to connect to {0}: {1}", a_pipe, e.Message));

                return false;
            }

            return true;
        }

        public override void WriteToStream(byte[] a_bytes)
        {
            long len = a_bytes.LongLength;
            long sent = 0;
        
            while (sent < len)
            {
                sent += m_socket.Send(a_bytes);
            }
        }
        public override byte[] FromStream()
        {
            bool littleEndian = BitConverter.IsLittleEndian;

            byte[] opCode = new byte[4];

            int count = m_socket.Receive(opCode);
            if (count != 4)
            {
                return null;
            }

            byte[] length = new byte[4];
            count = m_socket.Receive(length);

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
                bytesRead += (uint)m_socket.Receive(bytes, 0, (int)Math.Min(int.MaxValue, dataLen - bytesRead), SocketFlags.None);
            }

            byte[] data = new byte[dataLen + 8];
            Array.Copy(opCode, 0, data, 0, 4); 
            Array.Copy(length, 0, data, 4, 4);
            Array.Copy(bytes, 0, data, 8, dataLen);

            return data;
        }
    }
}
