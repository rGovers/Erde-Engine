using Newtonsoft.Json;

namespace Erde.Discord.Message
{
    public class ErrorMessage : IMessage
    {
        public enum e_ErrorCode
        {
            Success = 0,
            PipeException = 1,
            ReadCorrupt = 2,

            NotImplemented = 10,

            UnknownError = 1000,
            InvalidPayload = 4000,
            InvalidCommand = 4002,
            InvalidGuild = 4003,
            InvalidEvent = 4004,
            InvalidChannel = 4005,
            InvalidPermissions = 4006,
            InvalidClientID = 4007,
            InvalidOrigin = 4008,
            InvalidToken = 4009,
            InvalidUser = 4010,

            OAuth2Error = 5000,
            SelectChannelTimeout = 5001,
            GetGuildTimeout = 5002,
            SelectVoiceForceRequired = 5003,
            CaptureShortcurAlreadyListening = 5004
        }

        e_ErrorCode m_errorCode;
        string      m_message;

        [JsonProperty("code")]
        public e_ErrorCode ErrorCode
        {
            get
            {
                return m_errorCode;
            }
            set
            {
                m_errorCode = value;
            }
        }

        [JsonProperty("message")]
        public string Message
        {
            get
            {
                return m_message;
            }
            set
            {
                m_message = value;
            }
        }

        public e_MessageType MessageType
        {
            get
            {
                return e_MessageType.Error;
            }
        }
    }
}
