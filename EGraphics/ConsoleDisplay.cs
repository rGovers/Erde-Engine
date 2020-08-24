using Erde.Graphics.GUI;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Erde.Graphics
{
    public class ConsoleDisplay : IDisposable
    {
        Pipeline      m_pipeline;
        Canvas        m_canvas;

        List<TextBox> m_textBoxes;

        internal ConsoleDisplay (Pipeline a_pipeline)
        {
            m_canvas = new Canvas(new Vector2(1280, 720), a_pipeline);
            m_textBoxes = new List<TextBox>();

            m_pipeline = a_pipeline;
        }

        internal void Draw (Vector2 a_size, bool a_display)
        {
            int displayedElements = 0;
            List<InternalConsole.Message> messages = InternalConsole.Messages;

            TextBox textBox = null;

            for (int i = 0; i < m_textBoxes.Count; ++i)
            {
                InternalConsole.Message message = messages[i];
                message.TimeOut += (float)PipelineTime.DeltaTime;
                messages[i] = message;

                textBox = m_textBoxes[i];

                if (!a_display)
                {
                    textBox.Visible = message.TimeOut < 10.0f;

                    if (textBox.Visible)
                    {
                        textBox.XLockMode = e_XLockMode.Right;
                        textBox.YLockMode = e_YLockMode.Top;
                        textBox.Position = new Vector2(320, (displayedElements++ + 1) * 32);
                    }
                }
                else
                {
                    textBox.Visible = true;
                    textBox.XLockMode = e_XLockMode.Left;
                    textBox.YLockMode = e_YLockMode.Bottom;
                    textBox.Position = new Vector2(160, (i + 1) * 32);
                }
            }

            m_canvas.Draw(a_size);

            for (int i = m_textBoxes.Count; i < messages.Count; ++i)
            {
                InternalConsole.Message message = messages[i];

                Brush brush = Brushes.White;

                switch (message.Alert)
                {
                case InternalConsole.e_Alert.Error:
                {
                    brush = Brushes.Red;

                    break;
                }
                case InternalConsole.e_Alert.Warning:
                {
                    brush = Brushes.Yellow;

                    break;
                }
                }

                textBox = new TextBox(brush, SystemFonts.DefaultFont, 400, 24, m_pipeline)
                {
                    Text = message.Text
                };

                m_canvas.AddElement(textBox);

                m_textBoxes.Add(textBox);
            }
        }

        void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            m_canvas.Dispose();
        }
        ~ConsoleDisplay ()
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