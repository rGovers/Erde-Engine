using Newtonsoft.Json;
using System;
using System.Text;

namespace Erde.Discord.RichPresence
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Assets
    {
        string m_largeImage;
        string m_largeText;

        string m_smallImage;
        string m_smallText;

        [JsonProperty("large_image", NullValueHandling = NullValueHandling.Ignore)]
        public string LargeImage
        {
            get
            {
                return m_largeImage;
            }
            set
            {
                if (RichPresence.ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_largeImage = value;
                }
            }
        }
        [JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
        public string LargeImageText
        {
            get
            {
                return m_largeText;
            }
            set
            {
                if (RichPresence.ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_largeText = value;
                }
            }
        }

        [JsonProperty("small_image", NullValueHandling = NullValueHandling.Ignore)]
        public string SmallImage
        {
            get
            {
                return m_smallImage;
            }
            set
            {
                if (RichPresence.ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_smallImage = value;
                }
            }
        }
        [JsonProperty("small_text", NullValueHandling = NullValueHandling.Ignore)]
        public string SmallText
        {
            get
            {
                return m_smallText;
            }
            set
            {
                if (RichPresence.ValidateString(ref value, 128, Encoding.UTF8))
                {
                    m_smallText = value;
                }
            }
        }
    }
}
