using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Erde.Discord
{
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct Configuration
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        string m_apiEndpoint;                         
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        string m_cdnHost;                             
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        string m_environment;

        [JsonProperty("api_endpoint")]
        public string ApiEndpoint
        {
            get
            {
                return m_apiEndpoint;
            }
            set
            {
                m_apiEndpoint = value;
            }
        }

        [JsonProperty("cdn_host")]
        public string CdnHost
        {
            get
            {
                return m_cdnHost;
            }
            set
            {
                m_cdnHost = value;
            }
        }

        [JsonProperty("environment")]
        public string Environment
        {
            get
            {
                return m_environment;
            }
            set
            {
                m_environment = value;
            }
        }
    }
}
