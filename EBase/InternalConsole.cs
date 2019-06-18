using System;
using System.Collections.Generic;

namespace Erde
{
    public static class InternalConsole
    {
        public enum e_Alert
        {
            Normal,
            Warning,
            Error
        }

        public struct Message
        {
            public string  Text;
            public e_Alert Alert;
            public float   TimeOut;
        }

        static List<Message> m_messages = new List<Message>();

        public static List<Message> Messages
        {
            get
            {
                return m_messages;
            }
        }

        public static void Warning (string a_message)
        {
            if (!string.IsNullOrEmpty(a_message))
            {
                AddMessage(new Message()
                {
                    Text = a_message,
                    Alert = e_Alert.Warning
                });
            }
        }

        public static void Error (string a_message)
        {
            if (!string.IsNullOrEmpty(a_message))
            {
                AddMessage(new Message()
                {
                    Text = a_message,
                    Alert = e_Alert.Error
                });
            }
        }

        public static void AddMessage (string a_message, e_Alert a_alert = e_Alert.Normal)
        {
            if (!string.IsNullOrEmpty(a_message))
            {
                AddMessage(new Message()
                {
                    Text = a_message,
                    Alert = a_alert
                });
            }
        }

        public static void AddMessage (Message a_message)
        {
#if DEBUG
            Console.WriteLine(a_message.Text);
#endif
            a_message.TimeOut = 0;

            m_messages.Add(a_message);
        }
    }
}
