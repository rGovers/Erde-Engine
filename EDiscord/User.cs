using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Erde.Discord
{
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct User
    {
        [MarshalAs (UnmanagedType.ByValTStr, SizeConst=128)]
        string        m_userName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        string        m_avatar;

        ulong         m_userID;
        int           m_discriminator;

        bool          m_bot;

        Configuration m_config;

        [JsonProperty("id")]
        public ulong UserID
        {
            get
            {
                return m_userID;
            }
            set
            {
                m_userID = value;
            }
        }

        [JsonProperty("username")]
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        [JsonProperty("discriminator")]
        public int Discriminator
        {
            get
            {
                return m_discriminator;
            }
            set
            {
                m_discriminator = value;
            }
        }

        [JsonProperty("avatar")]
        public string Avatar
        {
            get
            {
                return m_avatar;
            }
            set
            {
                m_avatar = value;
            }
        }

        [JsonProperty("bot")]
        public bool Bot
        {
            get
            {
                return m_bot;
            }
            set
            {
                m_bot = value;
            }
        }

        public Configuration Configuration
        {
            get
            {
                return m_config;
            }
            set
            {
                m_config = value;
            }
        }
    }
}
