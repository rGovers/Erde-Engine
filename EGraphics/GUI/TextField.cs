using Erde.Application;
using Erde.IO;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

namespace Erde.Graphics.GUI
{
    public class TextField : TextBox
    {
        Pipeline   m_pipeline;
        List<char> m_internalKeys;

        bool       m_focused;

        public List<char> Keys
        {
            get
            {
                return m_internalKeys;
            }
        }

        public TextField (Brush a_brush, Font a_font, int a_width, int a_height, Pipeline a_pipeline) : base(a_brush, a_font, a_width, a_height, a_pipeline)
        {
            m_pipeline = a_pipeline;

            a_pipeline.Window.KeyPress += Window_KeyPress;

            Click += Focus;

            m_internalKeys = new List<char>()
            {
                '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '=',
                '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '+',
                'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '[', ']', '\\',
                'Q', 'W', 'E','R', 'T', 'Y', 'U', 'I', 'O', 'P', '{', '}', '|',
                'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';', '\'',
                'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', ':', '"',
                'z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', '/',
                'Z', 'X', 'C', 'V', 'B', 'N', 'M', '<', '>', '?',
                ' '
            };
        }

        void Focus (Canvas a_canvas, Element a_element)
        {
            // Sanity check I dont trust myself
            if (a_element == this)
            {
                m_focused = true;
            }
        }

        void Window_KeyPress (object sender, KeyPressEventArgs e)
        {
            if (m_focused)
            {
                char chr = e.KeyChar;

                if (m_internalKeys.Contains(chr))
                {
                    Text += chr;
                }
            }
        }

        internal new static TextField Create (XmlNode a_node, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            string text;
            string fontFamily;
            float fontSize;
            int width;
            int height;
            Brush brush;

            ExtractData(a_node, out text, out fontFamily, out fontSize, out width, out height, out brush);
            Font font = CompileFontData(fontFamily, fontSize);

            return new TextField(brush, font, width, height, a_pipeline)
            {
                Text = text
            };
        }

        internal override void Update (Vector2 a_resolution)
        {
            if (m_focused)
            {
                MouseState mouseState = Mouse.GetState();

                if (mouseState.IsButtonDown(MouseButton.Left))
                {
                    Vector2 cursorPos = Input.GetCursorPosition();

                    Vector2 pos = (TruePosition + a_resolution) / 2;
                    Vector2 size = TrueSize;

                    Vector2 halfSize = size / 2;

                    if (cursorPos.X < pos.X - halfSize.X || cursorPos.Y < pos.Y - halfSize.Y && cursorPos.X > pos.X + halfSize.X || cursorPos.Y > pos.Y + halfSize.Y)
                    {
                        m_focused = false;
                    }
                }

                if (Input.IsKeyPressed(Key.BackSpace) && Text.Length != 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                }
            }
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_pipeline.Window.KeyPress -= Window_KeyPress;
        }

        ~TextField ()
        {
            Dispose(false);
        }

        public new void Dispose ()
        {
            base.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
