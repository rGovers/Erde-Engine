using OpenTK;
using System.Collections.Concurrent;
using System.Drawing;

namespace Erde.Graphics.GUI
{
    public enum e_XLockMode : byte
    {
        Left,
        Right,
        Middle,
        Stretch,
        End
    }

    public enum e_YLockMode : byte
    {
        Top,
        Bottom,
        Middle,
        Stretch,
        End
    }

    public class Element
    {
        public delegate void Interaction (Canvas a_canvas, Element a_element);

        public enum e_State
        {
            Normal,
            Hover,
            Click,
            Release
        }

        Vector2                m_position;
        Vector2                m_size;

        Element                m_parent;
        ConcurrentBag<Element> m_children;

        e_XLockMode            m_xLockMode;
        e_YLockMode            m_yLockMode;

        Vector2                m_trueDrawingPos;
        Vector2                m_truePosition;
        Vector2                m_trueSize;

        bool                   m_visible;

        Interaction            m_hover;
        Interaction            m_normal;
        Interaction            m_click;
        Interaction            m_release;

        e_State                m_state;

        string                 m_subMenuDirectory;

        public string SubMenuDirectory
        {
            get
            {
                return m_subMenuDirectory;
            }
            internal set
            {
                m_subMenuDirectory = value;
            }
        }

        public e_State State
        {
            get
            {
                return m_state; 
            }
            internal set
            {
                m_state = value;
            }
        }

        public Interaction Hover
        {
            get
            {
                return m_hover;
            }
            set
            {
                m_hover = value;
            }
        }
        public Interaction Normal
        {
            get
            {
                return m_normal;
            }
            set
            {
                m_normal = value;
            }
        }
        public Interaction Click
        {
            get
            {
                return m_click;
            }
            set
            {
                m_click = value;
            }
        }
        public Interaction Release
        {
            get
            {
                return m_release;
            }
            set
            {
                m_release = value;
                ;
            }
        }

        public e_XLockMode XLockMode
        {
            get
            {
                return m_xLockMode;
            }
            set
            {
                m_xLockMode = value;
            }
        }
        public e_YLockMode YLockMode
        {
            get
            {
                return m_yLockMode;
            }
            set
            {
                m_yLockMode = value;
            }
        }

        public Vector2 Size
        {
            get
            {
                return m_size;
            }
            set
            {
                m_size = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }

        public bool Visible
        {
            get
            {
                return m_visible;
            }
            set
            {
                m_visible = value;
            }
        }

        internal Vector2 TruePosition
        {
            get
            {
                return m_truePosition;
            }
            set
            {
                m_truePosition = value;
            }
        }
        internal Vector2 TrueDrawingPosition
        {
            get
            {
                return m_trueDrawingPos;
            }
            set
            {
                m_trueDrawingPos = value;
            }
        }
        internal Vector2 TrueSize
        {
            get
            {
                return m_trueSize;
            }
            set
            {
                m_trueSize = value;
            }
        }

        public Element Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                if (m_parent != value)
                {
                    if (m_parent != null)
                    {
                        Element[] children = m_parent.Children.ToArray();
                        m_parent.m_children = new ConcurrentBag<Element>();

                        foreach (Element element in children)
                        {
                            if (element != this)
                            {
                                m_parent.m_children.Add(element);
                            }
                        }
                    }

                    m_parent = value;

                    if (m_parent != null)
                    {
                        m_parent.Children.Add(this);
                    }
                }
            }
        }

        public ConcurrentBag<Element> Children
        {
            get
            {
                return m_children;
            }
        }

        internal Vector2 GetTrueSize()
        {
            return GetTrueSize(m_parent.m_size, m_parent.m_trueSize);
        }
        internal Vector2 GetTrueSize(Vector2 a_parentSize, Vector2 a_parentTrueSize)
        {
            Vector2 size = Vector2.One;

            if (m_xLockMode == e_XLockMode.Stretch)
            {
                size.X = m_size.X * a_parentTrueSize.X / a_parentSize.X;    
            }
            else
            {
                size.X = m_size.X;
            }

            if (m_yLockMode == e_YLockMode.Stretch)
            {
                size.Y = m_size.Y * a_parentTrueSize.Y / a_parentSize.Y;    
            }
            else
            {
                size.Y = m_size.Y;
            }

            return size;
        }

        internal Vector2 GetTruePosition()
        {
            return GetTruePosition(m_trueSize, m_parent.m_truePosition, m_parent.m_trueSize);
        }
        internal Vector2 GetTruePosition(Vector2 a_trueSize, Vector2 a_parentTruePosition, Vector2 a_parentTrueSize)
        {
            Vector2 position = Vector2.Zero;

            switch (m_xLockMode)
            {
                case e_XLockMode.Left:
                {
                    position.X = (a_parentTruePosition.X - a_parentTrueSize.X) + (m_position.X + a_trueSize.X / 2);

                    break;
                }
                case e_XLockMode.Middle:
                {
                    position.X = a_parentTruePosition.X + (m_position.X + a_trueSize.X / 2);

                    break;
                }
                case e_XLockMode.Right:
                {
                    position.X = (a_parentTruePosition.X + a_parentTrueSize.X) - (m_position.X + a_trueSize.X / 2);

                    break;
                }
                case e_XLockMode.Stretch:
                {
                    position.X = a_parentTruePosition.X + (m_position.X + a_trueSize.X / 2);

                    break;
                }
            }

            switch (m_yLockMode)
            {
                case e_YLockMode.Top:
                {
                    position.Y = (a_parentTruePosition.Y + a_parentTrueSize.Y) - (m_position.Y + a_trueSize.Y / 2);
                    
                    break;
                }
                case e_YLockMode.Middle:
                {
                    position.Y = a_parentTruePosition.Y + (m_position.Y + a_trueSize.Y / 2);
                    
                    break;
                }
                case e_YLockMode.Bottom:
                {
                    position.Y = (a_parentTruePosition.Y - a_parentTrueSize.Y) + (m_position.Y + a_trueSize.Y / 2);
                    
                    break;
                }
                case e_YLockMode.Stretch:
                {
                    position.Y = a_parentTruePosition.Y + (m_position.Y + a_trueSize.Y / 2);

                    break;
                }
            }

            return position;
        }

        internal virtual Rectangle GetActiveRect(Vector2 a_resolution)
        {
            return new Rectangle(0, 0, (int)a_resolution.X, (int)a_resolution.Y);
        }

        protected void CalculateTrueTransform ()
        {
            if (m_parent != null)
            {
                Vector2 pTDP = m_parent.m_trueDrawingPos;
                Vector2 pTP = m_parent.m_truePosition;
                Vector2 pTS = m_parent.m_trueSize;
                Vector2 pS = m_parent.m_size;

                m_trueSize = GetTrueSize(pS, pTS);
                m_truePosition = GetTruePosition(m_trueSize, pTP, pTS);
                m_trueDrawingPos = GetTruePosition(m_trueSize, pTDP, pTS);
            }
        }

        internal Matrix4 ToMatrix (Vector2 a_resolution, Vector2 a_trueResolution)
        {
            float len = a_trueResolution.Length;

            float scale = 1.0f + (a_resolution - a_trueResolution).Length / len;

            Vector3 trueSize = new Vector3(m_trueSize.X / a_resolution.X, m_trueSize.Y / a_resolution.Y, 0) * scale;
            Vector3 truePosition = new Vector3(m_trueDrawingPos.X / a_resolution.X, m_trueDrawingPos.Y / a_resolution.Y, 0);

            return Matrix4.CreateScale(trueSize) * Matrix4.CreateTranslation(truePosition);
        }

        internal Element ()
        {
            m_visible = true;
            m_parent = null;
            m_children = new ConcurrentBag<Element>();
            m_position = Vector2.Zero;
            m_size = Vector2.One;
            m_state = e_State.Normal;
            m_subMenuDirectory = string.Empty;
        }

        internal virtual void Update (Vector2 a_resolution) { }
        internal virtual Vector2 Draw (Vector2 a_resolution, Vector2 a_trueResolution) 
        { 
            return a_resolution; 
        }
        internal virtual void PostDraw(Vector2 a_resolution, Vector2 a_trueResolution) { }
        internal virtual void Repaint () { }
    }
}