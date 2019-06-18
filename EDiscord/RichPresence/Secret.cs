using Newtonsoft.Json;
using System;
using System.Text;

namespace Erde.Discord.RichPresence
{
    [Serializable, JsonObject]
    public class Secret
    {
        string m_joinSecret;
        string m_spectateSecret;

        [JsonProperty("join", NullValueHandling = NullValueHandling.Ignore)]
        public string JoinSecret
        {
            get
            {
                return m_joinSecret;
            }
            set
            {
                if (RichPresence.ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_joinSecret = value;
                }
            }
        }
        [JsonProperty("spectate", NullValueHandling = NullValueHandling.Ignore)]
        public string SpectateSecret
        {
            get
            {
                return m_spectateSecret;
            }
            set
            {
                if (RichPresence.ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_spectateSecret = value;
                }
            }
        }
    }
}
