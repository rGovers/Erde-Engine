using Newtonsoft.Json;

namespace Erde.Discord.Payload
{
    public enum e_Command
    {
        Dispatch,
        SetActivity,
        Subscribe,
        Unsubscribe,
        SendActivityJoinInvite,
        CloseActivityJoinRequest,
        Authorize,
        Authenticate,
        GetGuild,
        GetGuilds,
        GetChannel,
        GetChannels,
        SetUserVoiceSettings,
        SelectVoiceChannel,
        GetSelectedVoiceChannel,
        SelectTextChannel,
        GetVoiceSettings,
        SetVoiceSettings,
        CaptureShortcut
    }

    internal interface IPayload
    {
        [JsonProperty("cmd"), JsonConverter(typeof(EnumConverter))]
        e_Command Command
        {
            get;
            set;
        }

        [JsonProperty("nonce")]
        string Nonce
        {
            get;
            set;
        }
    }
}
