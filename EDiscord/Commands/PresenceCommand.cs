using Erde.Discord.Payload;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Erde.Discord.Commands
{
    internal class PresenceCommand : ICommand
    {
        RichPresence.RichPresence m_richPresence;

        public int ProcessID
        {
            get;
            set;
        }

        [JsonProperty("activity")]
        public RichPresence.RichPresence Presence
        {
            get
            {
                return m_richPresence;
            }
            set
            {
                m_richPresence = value;
            }
        }

        public IPayload PreparePayload (long a_nonce)
        {
            return new ArgumentPayload()
            {
                Arguments = JObject.FromObject(this),
                Command = e_Command.SetActivity,
                Nonce = a_nonce.ToString()
            };
        }
    }
}
