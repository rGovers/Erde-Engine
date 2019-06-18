using Newtonsoft.Json;
using System;
using System.Text;

namespace Erde.Discord.RichPresence
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class RichPresence
    {
        string    m_state;
        string    m_details;

        TimeStamp m_timeStamp;

        Assets    m_assets;

        Party     m_party;

        Secret    m_secret;

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State
        {
            get
            {
                return m_state;
            }
            set
            {
                if (ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_state = value;
                }
            }
        }
        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Details
        {
            get
            {
                return m_details;
            }
            set
            {
                if (ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_details = value;
                }
            }
        }

        [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public TimeStamp TimeStamp
        {
            get
            {
                return m_timeStamp;
            }
            set
            {
                m_timeStamp = value;
            }
        }

        [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
        public Assets Assets
        {
            get
            {
                return m_assets;
            }
            set
            {
                m_assets = value;
            }
        }

        [JsonProperty("party", NullValueHandling = NullValueHandling.Ignore)]
        public Party Party
        {
            get
            {
                return m_party;
            }
            set
            {
                m_party = value;
            }
        }

        [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
        public Secret Secret
        {
            get
            {
                return m_secret;
            }
            set
            {
                m_secret = value;
            }
        }

        internal static bool ValidateString (ref string a_string, int a_bytes, Encoding a_encoding)
        {
            if (string.IsNullOrEmpty(a_string))
            {
                return true;
            }

            a_string = a_string.Trim();

            if (a_encoding.GetByteCount(a_string) > a_bytes)
            {
                return false;
            }

            return true;
        }
    }
}
