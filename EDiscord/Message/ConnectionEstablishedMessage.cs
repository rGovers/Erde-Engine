namespace Erde.Discord.Message
{
    public class ConnectionEstablishedMessage : IMessage
    {
        int m_pipe;

        public int Pipe
        {
            get
            {
                return m_pipe;
            }
        }

        public e_MessageType MessageType
        {
            get
            {
                return e_MessageType.ConnectionEstablished;
            }
        }

        public ConnectionEstablishedMessage (int a_pipe)
        {
            m_pipe = a_pipe;
        }
    }
}
