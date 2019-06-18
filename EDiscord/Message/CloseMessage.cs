namespace Erde.Discord.Message
{
    public class CloseMessage : IMessage
    {
        int    m_code;
        string m_reason;

        public int Code
        {
            get
            {
                return m_code;
            }
        }
        public string Reason
        {
            get
            {
                return m_reason;
            }
        }

        public e_MessageType MessageType
        {
            get
            {
                return e_MessageType.Close;
            }
        }

        public CloseMessage (int a_code, string a_reason)
        {
            m_reason = a_reason;
        }
    }
}
