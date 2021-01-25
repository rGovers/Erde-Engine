using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Erde.Discord
{
    // I am aware of UnixDomainSocketEndPoint but would not compile on some systems saying that the class was missing not sure
    // Credit: https://stackoverflow.com/questions/40195290/how-to-connect-to-a-unix-domain-socket-in-net-core-in-c-sharp
    class UnixEndPoint : EndPoint
    {
        string m_path;

        public string Path
        {
            get
            {
                return m_path;
            }
        }

        UnixEndPoint()
        {
            m_path = string.Empty;
        }
        public UnixEndPoint(string a_path)
        {
            if (string.IsNullOrEmpty(a_path))
            {
                throw new ArgumentNullException("Invalid Path");
            }

            m_path = a_path;
        }

        public override AddressFamily AddressFamily
        {
            get
            {
                return AddressFamily.Unix;
            }
        }

        public override EndPoint Create (SocketAddress a_socketAddress)
        {
            if (a_socketAddress.Size == 2)
            {
                return new UnixEndPoint();
            }

            int size = a_socketAddress.Size - 2;
            byte[] bytes = new byte[size];
            for (int i = 0; i < bytes.Length; ++i) 
            {
                bytes[i] = a_socketAddress[i + 2];

                if (bytes [i] == 0) 
                {
                    size = i;

                    break;
                }
            }

            return new UnixEndPoint(Encoding.UTF8.GetString(bytes, 0, size));
        }

        public override SocketAddress Serialize ()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(m_path);
            int len = bytes.Length;

            SocketAddress sa = new SocketAddress(AddressFamily, len + 3);
            for (int i = 0; i < len; ++i)
            {
                sa[2 + i] = bytes[i];
            }

            sa[len + 2] = 0;

            return sa;
        }

        public override string ToString() 
        {
            return m_path;
        }

        public override int GetHashCode ()
        {
            return m_path.GetHashCode ();
        }

        public override bool Equals (object o)
        {
            UnixEndPoint other = o as UnixEndPoint;
            if (other == null)
            {
                return false;
            }

            return (other.m_path == m_path);
        }
    }
}