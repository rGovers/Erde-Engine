using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Erde.Application
{
    public class Input
    {
        static Input      m_input;

        KeyboardState     m_keyboardState;
        KeyboardState     m_prevKeyboardState;
                          
        MouseState        m_mouseState;
        MouseState        m_prevMouseState;
                          
        List<Key>         m_blockedKeys;
        List<MouseButton> m_blockedMouseButtons;

        Application       m_application;

        internal Input (Application a_application)
        {
            m_keyboardState = Keyboard.GetState();
            m_prevKeyboardState = Keyboard.GetState();

            m_mouseState = Mouse.GetState();
            m_prevMouseState = Mouse.GetState();

            m_blockedKeys = new List<Key>();
            m_blockedMouseButtons = new List<MouseButton>();

            m_application = a_application;

            if (m_input == null)
            {
                m_input = this;
            }
        }

        internal void Update ()
        {
            m_prevKeyboardState = m_keyboardState;
            m_keyboardState = Keyboard.GetState();

            m_prevMouseState = m_mouseState;
            m_mouseState = Mouse.GetState();

            lock (m_blockedKeys)
            {
                foreach (Key key in m_blockedKeys)
                {
                    if (m_keyboardState.IsKeyUp(key) && m_prevKeyboardState.IsKeyUp(key))
                    {
                        m_blockedKeys.Remove(key);
                    }
                }
            }

            lock (m_blockedMouseButtons)
            {
                for (int i = 0; i < m_blockedMouseButtons.Count; ++i)
                {
                    MouseButton mouseButton = m_blockedMouseButtons[i];

                    if (m_mouseState.IsButtonUp(mouseButton) && m_prevMouseState.IsButtonUp(mouseButton))
                    {
                        m_blockedMouseButtons.RemoveAt(i);
                    }
                }
            }
        }

        public static void BlockKey (Key a_key)
        {
            if (!m_input.m_blockedKeys.Contains(a_key))
            {
                m_input.m_blockedKeys.Add(a_key);
            }
        }
        public static void BlockButton (MouseButton a_button)
        {
            if (!m_input.m_blockedMouseButtons.Contains(a_button))
            {
                m_input.m_blockedMouseButtons.Add(a_button);
            }
        }

        public static bool IsMousePressed (MouseButton a_button)
        {
            foreach (MouseButton mouseButton in m_input.m_blockedMouseButtons)
            {
                if (mouseButton == a_button)
                {
                    return false;
                }
            }

            return m_input.m_mouseState.IsButtonDown(a_button) && m_input.m_prevMouseState.IsButtonUp(a_button);
        }
        public static bool IsMouseReleased (MouseButton a_button)
        {
            foreach (MouseButton mouseButton in m_input.m_blockedMouseButtons)
            {
                if (mouseButton == a_button)
                {
                    return false;
                }
            }

            return m_input.m_mouseState.IsButtonUp(a_button) && m_input.m_prevMouseState.IsButtonDown(a_button);
        }
        public static bool IsMouseDown (MouseButton a_button)
        {
            foreach (MouseButton mouseButton in m_input.m_blockedMouseButtons)
            {
                if (mouseButton == a_button)
                {
                    return false;
                }
            }

            return m_input.m_mouseState.IsButtonDown(a_button);
        }
        public static bool IsMouseUp (MouseButton a_button)
        {
            foreach (MouseButton mouseButton in m_input.m_blockedMouseButtons)
            {
                if (mouseButton == a_button)
                {
                    return true;
                }
            }

            return m_input.m_mouseState.IsButtonUp(a_button);
        }

        public static Vector2 GetCursorPosition ()
        {
            Point point = Cursor.Position;

            point = m_input.m_application.PointToClient(point);

            return new Vector2(point.X, m_input.m_application.Height - point.Y);
        }

        public static bool IsKeyReleased (Key a_key)
        {
            foreach (Key key in m_input.m_blockedKeys)
            {
                if (key == a_key)
                {
                    return false;
                }
            }

            return m_input.m_keyboardState.IsKeyUp(a_key) && m_input.m_prevKeyboardState.IsKeyDown(a_key);
        }
        public static bool IsKeyPressed (Key a_key)
        {
            foreach (Key key in m_input.m_blockedKeys)
            {
                if (key == a_key)
                {
                    return false;
                }
            }

            return m_input.m_keyboardState.IsKeyDown(a_key) && m_input.m_prevKeyboardState.IsKeyUp(a_key);
        }
        public static bool IsKeyDown (Key a_key)
        {
            foreach (Key key in m_input.m_blockedKeys)
            {
                if (key == a_key)
                {
                    return false;
                }
            }

            return m_input.m_keyboardState.IsKeyDown(a_key);
        }
        public static bool IsKeyUp (Key a_key)
        {
            foreach (Key key in m_input.m_blockedKeys)
            {
                if (key == a_key)
                {
                    return true;
                }
            }

            return m_input.m_keyboardState.IsKeyUp(a_key);
        }
    }
}
