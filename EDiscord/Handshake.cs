using Newtonsoft.Json;

namespace Erde.Discord
{
    internal class Handshake
    {
        int    m_version;
        string m_clientID;

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

        [JsonProperty("client_id")]
        public string ClientID
        {
            get
            {
                return m_clientID;
            }
            set
            {
                m_clientID = value;
            }
        }
    }
}
