using Erde.Discord.Commands;
using Erde.Discord.Message;
using Erde.Discord.Payload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Erde.Discord
{
    public delegate void Event (int a_errorCode, string a_message);
    public delegate void Join (string a_secret);

    public class DiscordClient : IDisposable
    {
        public enum e_RPCState
        {
            Disconnected,
            Connecting,
            Connected
        }

        const string PIPENAME = @"discord-ipc-{0}";

        public Event Disconnected;
        public Event Error;
        public Join Join;
        public Join Spectate;

        e_RPCState                m_state;
        byte[]                    m_buffer;

        int                       m_connectedPipe;

        Thread                    m_mainLoop;

        bool                      m_shutdown;
        bool                      m_join;

        NamedPipeClientStream     m_stream;

        Queue<Frame>              m_frameQueue;
        Queue<IMessage>           m_messageQueue;
        Queue<ICommand>           m_commandQueue;

        int                       m_version;
        string                    m_clientID;

        User?                     m_user;
        Configuration             m_config;

        int                       m_processID;

        RichPresence.RichPresence m_richPresence;

        long                      m_nounce;

        public RichPresence.RichPresence RichPresence
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

        public User? User
        {
            get
            {
                return m_user;
            }
        }

        public DiscordClient ()
        {
            m_processID = Process.GetCurrentProcess().Id;

            m_frameQueue = new Queue<Frame>();
            m_messageQueue = new Queue<IMessage>();
            m_commandQueue = new Queue<ICommand>();

            m_state = e_RPCState.Disconnected;

            m_nounce = 0;

            m_buffer = new byte[1024 * 16];
        }

        void EnqueueMessage (IMessage a_message)
        {
            lock (m_messageQueue)
            {
                m_messageQueue.Enqueue(a_message);
            }
        }
        void EnqueueCommand (ICommand a_command)
        {
            lock (m_commandQueue)
            {
                m_commandQueue.Enqueue(a_command);
            }
        }
        void EnqueueFrame (Frame a_frame)
        {
            lock (m_frameQueue)
            {
                m_frameQueue.Enqueue(a_frame);
            } 
        }

        bool WriteFrame (Frame a_frame)
        {
            if (!m_stream.IsConnected)
            {
                InternalConsole.AddMessage("Failed to write to disconnected stream");
            }

            try
            {
                a_frame.WriteToStream(m_stream);

                return true;
            }
            catch (IOException e)
            {
                InternalConsole.AddMessage("Discord Client: Failed to write frame: IOException: " + e.Message, InternalConsole.e_Alert.Error);
            }
            catch (ObjectDisposedException)
            {
                InternalConsole.AddMessage("Discord Client: Failed to write frame: Disposed", InternalConsole.e_Alert.Error);
            }
            catch (InvalidOperationException)
            {
                InternalConsole.AddMessage("Discord Client: Invalid Operaion when writing frame", InternalConsole.e_Alert.Error);
            }

            return false;
        }
        bool ReadFrame (out Frame a_frame)
        {
            lock (m_frameQueue)
            {
                if (m_frameQueue.Count == 0)
                {
                    a_frame = new Frame();

                    return false;
                }

                a_frame = m_frameQueue.Dequeue();
            }

            return true;
        }

        void Handshake ()
        {
            InternalConsole.AddMessage("Attempting Discord Handshake");

            if (m_state != e_RPCState.Disconnected)
            {
                InternalConsole.AddMessage("Discord Client: State must be disconnected to handshake", InternalConsole.e_Alert.Warning);

                return;
            }

            if (WriteFrame(new Frame(Frame.e_OpCode.Handshake, new Handshake() { Version = m_version, ClientID = m_clientID })))
            {
                m_state = e_RPCState.Connecting;
            }
        }

        void ProcessEvent (EventPayload a_event)
        {
            if (a_event.Event.HasValue && a_event.Event == EventPayload.e_ServerEvent.Error)
            {
                ErrorMessage error = a_event.GetObject<ErrorMessage>();

                InternalConsole.AddMessage(string.Format("Discord responded with error: ({0}) {1}", error.ErrorCode, error.Message), InternalConsole.e_Alert.Error);
                EnqueueMessage(error);
            }

            if (m_state == e_RPCState.Connecting)
            {
                if (a_event.Command == e_Command.Dispatch && a_event.Event.HasValue && a_event.Event.Value == EventPayload.e_ServerEvent.Ready)
                {
                    m_state = e_RPCState.Connected;

                    ReadyMessage ready = a_event.GetObject<ReadyMessage>();

                    m_config = ready.Configuration;

                    User user = ready.User;

                    user.Configuration = ready.Configuration;

                    ready.User = user;
                    m_user = user;

                    EnqueueMessage(ready);

                    return;
                }
            }

            // TODO: Implement Connected Commands
            if (m_state == e_RPCState.Connected)
            {
                switch (a_event.Command)
                {
                case e_Command.Dispatch:
                    {

                        break;
                    }
                }
            }
        }
        void ProcessCommandQueue ()
        {
            if (m_state != e_RPCState.Connected)
            {
                return;
            }

            bool write = m_commandQueue.Count > 0;
            ICommand command = null;

            while (write && m_stream.IsConnected)
            {
                command = m_commandQueue.Peek();

                if (command is CloseCommand)
                {
                    m_state = e_RPCState.Disconnected;

                    if (!WriteFrame(new Frame(Frame.e_OpCode.Close, new Handshake() { Version = m_version, ClientID = m_clientID })))
                    {
                        InternalConsole.AddMessage("Discord Client: Handwave Failed", InternalConsole.e_Alert.Error);
                    }

                    return;
                }
                else
                {
                    IPayload payload = command.PreparePayload(m_nounce++);

                    Frame frame = new Frame();

                    if (!m_shutdown)
                    {
                        frame.OpCode = Frame.e_OpCode.Frame;
                        frame.SetObject(payload);

                        WriteFrame(frame);
                    }
                }

                m_commandQueue.Dequeue();
                write = m_commandQueue.Count > 0;
            }
        }

        void DiscordLoop ()
        {
            bool connected = false;
            m_join = false;

            for (int i = 0; i < 10; ++i)
            {
                if (AttemptConnection(i))
                {
                    connected = true;

                    break;
                }

                if (m_shutdown)
                {
                    break;
                }
            }

            if (!connected)
            {
                InternalConsole.AddMessage("Failed to connect to Discord", InternalConsole.e_Alert.Error);

                m_shutdown = true;
            }
            else
            {
                Read();

                EnqueueMessage(new ConnectionEstablishedMessage(m_connectedPipe));

                Handshake();

                while (!m_shutdown)
                {
                    Frame frame;

                    if (ReadFrame(out frame))
                    {
                        switch (frame.OpCode)
                        {
                        case Frame.e_OpCode.Close:
                            {
                                ClosePayload close = frame.GetObject<ClosePayload>();
                                InternalConsole.AddMessage("Discord Client Remotely Terminated");
                                EnqueueMessage(new CloseMessage(close.Code, close.Reason));

                                m_shutdown = true;

                                break;
                            }
                        case Frame.e_OpCode.Ping:
                            {
                                WriteFrame(new Frame(Frame.e_OpCode.Pong, frame.Data));

                                break;
                            }
                        case Frame.e_OpCode.Pong:
                            {
                                InternalConsole.Warning("Got a pong from Discord?");

                                break;
                            }
                        case Frame.e_OpCode.Frame:
                            {
                                if (m_shutdown)
                                {
                                    break;
                                }

                                if (frame.Data == null)
                                {
                                    InternalConsole.AddMessage("Discord Client: No data in frame", InternalConsole.e_Alert.Error);
                                }

                                EventPayload response = frame.GetObject<EventPayload>();
                                ProcessEvent(response);

                                break;
                            }
                        default:
                            {
                                InternalConsole.AddMessage("Discord Client: Invalid Operation", InternalConsole.e_Alert.Error);
                                m_shutdown = true;

                                break;
                            }
                        }
                    }

                    ProcessCommandQueue();
                }

                ProcessCommandQueue();
            }
            
            m_join = true;
        }

        void Read ()
        {
            if (!m_stream.IsConnected)
            {
                return;
            }

            try
            {
                m_stream.BeginRead(m_buffer, 0, m_buffer.Length, EndRead, m_stream.IsConnected);
            }
            catch (ObjectDisposedException)
            {
                InternalConsole.AddMessage("Discord Client: Attempted to read from disposed stream", InternalConsole.e_Alert.Error);
                return;
            }
            catch (InvalidOperationException)
            {
                InternalConsole.AddMessage("Discord Client: Attempted to read from closed pipe", InternalConsole.e_Alert.Error);
                return;
            }
        }
        void EndRead (IAsyncResult a_callback)
        {
            int bytes = -1;

            try
            {
                bytes = m_stream.EndRead(a_callback);
            }
            catch (IOException)
            {
                InternalConsole.AddMessage("Discord Client: Attempted to read from closed pipe", InternalConsole.e_Alert.Error);
                return;
            }
            catch (ObjectDisposedException)
            {
                InternalConsole.AddMessage("Discord Client: Attempted to end reading from a disposed pipe", InternalConsole.e_Alert.Error);
                return;
            }
            catch (NullReferenceException)
            {
                InternalConsole.AddMessage("Discord Client: Attempted to connect to null pipe", InternalConsole.e_Alert.Error);
                return;
            }

            if (bytes > 0)
            {
                using (MemoryStream stream = new MemoryStream(m_buffer, 0, bytes))
                {
                    Frame frame = Frame.ReadStream(stream);

                    if (frame != null)
                    {
                        EnqueueFrame(frame);
                    }
                }
            }

            if (m_stream.IsConnected)
            {
                Read();
            }
        }

        bool AttemptConnection (int a_pipe)
        {
            string pipename = string.Format(PIPENAME, a_pipe);

            try
            {
                m_stream = new NamedPipeClientStream(".", pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
                m_stream.Connect(1000);

                do
                {
                    Thread.Sleep(10);
                }
                while (!m_stream.IsConnected);

                InternalConsole.AddMessage("Connected: " + pipename);
                m_connectedPipe = a_pipe;

                return true;
            }
            catch (Exception e)
            {
                InternalConsole.AddMessage(string.Format("Failed to connect to {0}: {1}", pipename, e.Message), InternalConsole.e_Alert.Error);
            }

            return false;
        }

        public void InitDiscordClient (int a_version, string a_clientID)
        {
            if (a_version <= 0)
            {
                throw new IndexOutOfRangeException("Version cannot be less than 1");
            }

            m_shutdown = false;

            m_mainLoop = new Thread(DiscordLoop)
            {
                Name = "Discord Intergration"
            };

            m_version = a_version;
            m_clientID = a_clientID;

            m_mainLoop.Start();
        }

        void HandleMessage (IMessage a_message)
        {
            if (a_message == null)
            {
                return;
            }

            switch (a_message.MessageType)
            {
            case e_MessageType.Ready:
                {
                    ReadyMessage ready = a_message as ReadyMessage;

                    SynchronizeState();

                    break;
                }
            }
        }

        public void SynchronizeState ()
        {
            EnqueueCommand(new PresenceCommand()
            {
                ProcessID = m_processID,
                Presence = m_richPresence
            });
        }

        public void InvokeFunctions ()
        {
            while (m_messageQueue.Count != 0)
            {
                IMessage message = m_messageQueue.Dequeue();

                HandleMessage(message);

                switch (message.MessageType)
                {
                case e_MessageType.Error:
                    {
                        ErrorMessage error = message as ErrorMessage;

                        Error((int)error.ErrorCode, error.Message);

                        break;
                    }
                }
            }
        }

        void Dispose (bool a_state)
        {
            Tools.Verify(this, a_state);

            m_commandQueue.Clear();
            EnqueueCommand(new PresenceCommand()
            {
                ProcessID = m_processID,
                Presence = null
            });
            EnqueueCommand(new CloseCommand());

            m_shutdown = true;

            while (!m_join)
            {
                Thread.Yield();
            }

            m_mainLoop.Join();
        }
        ~DiscordClient ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);    
        }
    }
}
