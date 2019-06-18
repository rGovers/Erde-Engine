using Erde.Discord.Payload;
using Newtonsoft.Json;

namespace Erde.Discord.Commands
{
    internal class CloseCommand : ICommand
    {
        string m_reason = "Terminating";

        public int ProcessID
        {
            get;
            set;
        }

        [JsonProperty("close_reason")]
        public string Reason
        {
            get
            {
                return m_reason;
            }
            set
            {
                m_reason = value;
            }
        }

        public IPayload PreparePayload (long a_nonce)
        {
            return new ArgumentPayload()
            {
                Command = e_Command.Dispatch,
                Nonce = null,
                Arguments = null
            };
        }
    }
}
