namespace Erde.Discord.Client
{
    internal abstract class InternalClient
    {
        DiscordClient m_client;

        public DiscordClient Client
        {
            get
            {
                return m_client;
            }
        }

        public abstract bool IsConnected
        {
            get;
        }

        internal InternalClient(DiscordClient a_client)
        {
            m_client = a_client;
        }

        public abstract void BeginRead();

        public abstract bool AttemptConnection (string a_pipe);

        public abstract void WriteToStream(byte[] a_bytes);
        public abstract byte[] FromStream();
    }
}
