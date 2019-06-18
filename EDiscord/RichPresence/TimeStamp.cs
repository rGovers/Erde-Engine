using Newtonsoft.Json;
using System;

namespace Erde.Discord.RichPresence
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TimeStamp
    {
        DateTime? m_start;
        DateTime? m_end;

        public DateTime? Start
        {
            get
            {
                return m_start;
            }
            set
            {
                m_start = value;
            }
        }    
        public DateTime? End
        {
            get
            {
                return m_end;
            }
            set
            {
                m_end = value;
            }
        }

        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochStart
        {
            get
            {
                if (m_start.HasValue)
                {
                    return ToUnixTime(m_start.Value);
                }

                return null;
            }
            set
            {
                m_start = value.HasValue ? FromUnixTime(value.Value) : (DateTime?)null;
            }
        }
        [JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochEnd
        {
            get
            {
                if (m_end.HasValue)
                {
                    return ToUnixTime(m_end.Value);
                }

                return null;
            }
            set
            {
                m_end = value.HasValue ? FromUnixTime(value.Value) : (DateTime?)null;
            }
        }

        static DateTime UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime (long a_time)
        {
            return UnixStart.AddMilliseconds(a_time);
        }

        public static long ToUnixTime (DateTime a_time)
        {
            return Convert.ToInt64((a_time - UnixStart).TotalSeconds);
        }
    }
}
