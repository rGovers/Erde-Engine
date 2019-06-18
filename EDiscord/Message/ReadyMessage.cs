using Newtonsoft.Json;

namespace Erde.Discord.Message
{
    public class ReadyMessage : IMessage
    {
        int           m_version;
        Configuration m_configuration;
        User          m_user;

        [JsonProperty("v")]
        public int Version
        {
            get
            {
                return m_version;
            }
            set
            {
                m_version = value;
            }
        }

        [JsonProperty("config")]
        public Configuration Configuration
        {
            get
            {
                return m_configuration;
            }
            set
            {
                m_configuration = value;
            }
        }

        [JsonProperty("user")]
        public User User
        {
            get
            {
                return m_user;
            }
            set
            {
                m_user = value;
            }
        }

        public e_MessageType MessageType
        {
            get
            {
                return e_MessageType.Ready;
            }
        }
    }
}
