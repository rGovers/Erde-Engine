using Newtonsoft.Json;
using System;

namespace Erde.Discord.RichPresence
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Party
    {
        // Having to work around weird quirks with the Discord backend when it comes to party sizes
        // Offical SDK has the max size optional but server throws errors if it is not filled
        string m_id;
        int?   m_size;
        int?   m_maxSize;

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        public int Size
        {
            get
            {
                if (m_size.HasValue)
                {
                    return m_size.Value;
                }

                return 0;
            }
            set
            {
                m_size = value;
            }
        }
        public int MaxSize
        {
            get
            {
                if (m_maxSize.HasValue)
                {
                    return m_maxSize.Value;
                }

                return 0;
            }
            set
            {
                m_maxSize = value;
            }
        }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        int[] _Size
        {
            get
            {
                if (m_size.HasValue && m_maxSize.HasValue)
                {
                    return new int[] { m_size.Value, m_maxSize.Value }; 
                }

                return null;
            }
            set
            {
                if (value.Length != 2)
                {
                    m_size = null;
                    m_maxSize = null;
                }
                else
                {
                    m_size = value[0];
                    m_maxSize = value[1];
                }
            }
        }
    }
}
