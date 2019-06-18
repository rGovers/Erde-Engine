using Newtonsoft.Json;

namespace Erde.Discord.Payload
{
    internal class ClosePayload : IPayload
    {
        int    m_code;
        string m_reason;

        [JsonProperty("code")]
        public int Code
        {
            get
            {
                return m_code;
            }
            set
            {
                m_code = value;
            }
        }

        [JsonProperty("message")]
        public string Reason
        {
            get
            {
                return m_reason;
            }
            set
            {
                m_reason = value;
            }
        }

        public e_Command Command { get; set; }
        public string Nonce { get; set; }
    }
}
