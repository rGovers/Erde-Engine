using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Erde.Discord.Payload
{
    public class ArgumentPayload : IPayload
    {
        JObject m_arg;

        [JsonProperty("args", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Arguments
        {
            get
            {
                return m_arg;
            }
            set
            {
                m_arg = value;
            }
        }

        public void SetObject (object a_obj)
        {
            m_arg = JObject.FromObject(a_obj);
        }

        public e_Command Command { get; set; }
        public string Nonce { get; set; }
    }
}
