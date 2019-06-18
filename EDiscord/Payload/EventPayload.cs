using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Erde.Discord.Payload
{
    internal class EventPayload : IPayload
    {
        internal enum e_ServerEvent
        {
            Ready,
            Error,
            ActivityJoin,
            ActivitySpectate,
            ActivityJoinRequest,
            GuildStatus,
            GuildCreate,
            ChannelCreate,
            VoiceChannelSelect,
            VoiceStateCreated,
            VoiceStateUpdated,
            VoiceStateDelete,
            VoiceSettingsUpdate,
            VoiceConnectionStatus,
            SpeakingStart,
            SpeakingStop,
            MessageCreate,
            MessageUpdate,
            MessageDelete,
            NotificationCreate,
            CaptureShortcutChange
        }

        JObject        m_data;
        e_ServerEvent? m_event;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Data
        {
            get
            {
                return m_data;
            }
            set
            {
                m_data = value;
            }
        }

        // It is nullable which makes me think discord uses low level servers which makes sense
        [JsonProperty("evt"), JsonConverter(typeof(EnumConverter))]
        public e_ServerEvent? Event
        {
            get
            {
                return m_event;
            }
            set
            {
                m_event = value;
            }
        }

        public e_Command Command { get; set; }
        public string Nonce { get; set; }

        public T GetObject<T> ()
        {
            if (Data == null)
            {
                return default(T);
            }

            return Data.ToObject<T>();
        }
    }
}
