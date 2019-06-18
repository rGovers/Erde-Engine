namespace Erde.Discord.Message
{
    public enum e_MessageType
    {
        ConnectionEstablished,
        Ready,
        Close,
        Error
    }

    public interface IMessage
    {
        e_MessageType MessageType
        {
            get;
        }
    }
}
