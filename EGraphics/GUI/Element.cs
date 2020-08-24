using OpenTK;
using System.Collections.Concurrent;

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

        protected void CalculateTrueTransform ()
        {
            if (m_parent != null)
            {
                Vector2 pTP = m_parent.m_truePosition;
                Vector2 pP = m_parent.m_position;
                Vector2 pTS = m_parent.m_trueSize;
                Vector2 pS = m_parent.m_size;

                if (m_xLockMode == e_XLockMode.Stretch)
                {
                    m_trueSize.X = m_size.X * pTS.X / pS.X;
                    m_truePosition.X = pTP.X + (m_position.X + m_trueSize.X / 2);
                }
                else
                {
                    m_trueSize.X = m_size.X;

                    switch (m_xLockMode)
                    {
                    case e_XLockMode.Left:
                        {
                            m_truePosition.X = (pTP.X - pTS.X) + (m_position.X + m_trueSize.X / 2);

                            break;
                        }
                    case e_XLockMode.Middle:
                        {
                            m_truePosition.X = pTP.X + (m_position.X + m_trueSize.X / 2);

                            break;
                        }
                    case e_XLockMode.Right:
                        {
                            m_truePosition.X = (pTP.X + pTS.X) - (m_position.X + m_trueSize.X / 2);

                            break;
                        }
                    }
                }

                if (m_yLockMode == e_YLockMode.Stretch)
                {
                    m_trueSize.Y = m_size.Y * pTS.Y / pTS.Y;
                    m_truePosition.Y = pTP.Y + (m_position.Y + m_trueSize.Y / 2);
                }
                else
                {
                    m_trueSize.Y = m_size.Y;

                    switch (m_yLockMode)
                    {
                    case e_YLockMode.Top:
                        {
                            m_truePosition.Y = (pTP.Y + pTS.Y) - (m_position.Y + m_trueSize.Y / 2);
                    
                            break;
                        }
                    case e_YLockMode.Middle:
                        {
                            m_truePosition.Y = pTP.Y + (m_position.Y + m_trueSize.Y / 2);
                    
                            break;
                        }
                    case e_YLockMode.Bottom:
                        {
                            m_truePosition.Y = (pTP.Y - pTS.Y) + (m_position.Y + m_trueSize.Y / 2);
                    
                            break;
                        }
                    }
                }
            }
        }

        internal Matrix4 ToMatrix (Vector2 a_resolution)
        {
            Vector3 trueSize = new Vector3(m_trueSize.X / a_resolution.X, m_trueSize.Y / a_resolution.Y, 0);
            Vector3 truePosition = new Vector3(m_truePosition.X / a_resolution.X, m_truePosition.Y / a_resolution.Y, 0);

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
        internal virtual void Draw (Vector2 a_resolution)
        {
        }
        internal virtual void Repaint () { }
    }
}