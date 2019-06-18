using Erde.Discord.Payload;
using Newtonsoft.Json;

namespace Erde.Discord.Commands
{
    internal interface ICommand
    {
        [JsonProperty("pid")]
        int ProcessID
        {
            get;
            set;
        }

        IPayload PreparePayload (long a_nonce);
    }
}
