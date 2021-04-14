using Erde.Application;
using Erde.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Erde.Graphics.GUI
{
    public class Canvas : IGraphicsObject
    {
        string                      m_name;

        Vector2                     m_resolution;

        ConcurrentBag<Element>      m_elements;

        Pipeline                    m_pipeline;

        Element                     m_rootElement;

        Dictionary<string, Element> m_namedElements;

        bool                        m_visible;

        MouseState                  m_prevMouseState;

        Canvas                      m_parentCanvas;
        Canvas                      m_childCanvas;
        bool                        m_paintedCanvas;

        IFileSystem                 m_fileSystem;

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public Vector2 Resolution
        {
            get
            {
                return m_resolution;
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

        public Canvas ChildCanvas
        {
            get
            {
                return m_childCanvas;
            }
        }
        public Canvas ParentCanvas
        {
            get
            {
                return m_parentCanvas;
            }
        }

        internal ConcurrentBag<Element> Elements
        {
            get
            {
                return m_elements;
            }
        }

        Canvas () : this(Vector2.Zero, null) { }

        internal Canvas (Vector2 a_resolution, Pipeline a_pipeline)
        {
            m_name = string.Empty;

            m_resolution = a_resolution;
            m_pipeline = a_pipeline;

            m_rootElement = new Element();

            m_visible = true;
            m_elements = new ConcurrentBag<Element>();
            m_namedElements = new Dictionary<string, Element>();

            m_fileSystem = null;
        }

        Element.Interaction GetAssemblyFunction(string a_typeName, string a_functionName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly asm in assemblies)
            {
                Type type = asm.GetType(a_typeName);

                if (type != null)
                {
                    MethodInfo methodInfo = type.GetMethod(a_functionName, new Type[] { typeof(Canvas), typeof(Element) });

                    if (methodInfo != null)
                    {
                        return methodInfo.CreateDelegate(typeof(Element.Interaction)) as Element.Interaction;
                    }
                }
            }

            return null;
        }

        void PopulateElements (XmlNode a_node, Element a_parent)
        {
            if (a_node.NodeType == XmlNodeType.Comment)
            {
                return;
            }

            Type type = Type.GetType(a_node.Name);

            if (type == null)
            {
                type = Type.GetType("Erde.Graphics.GUI." + a_node.Name);

                if (type == null)
                {
                    InternalConsole.Error("Canvas: Invalid Element Type: " + a_node.Name);

                    return;
                }
            }

            MethodInfo methodInfo = type.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);

            Element element = methodInfo.Invoke(null, new object[] { a_node, m_fileSystem, m_pipeline }) as Element;

            if (element != null)
            {
                foreach (XmlAttribute att in a_node.Attributes)
                {
                    switch (att.Name.ToLower())
                    {
                    case "visible":
                    {
                        element.Visible = bool.Parse(att.Value); 

                        break;
                    }
                    case "width":
                        {
                            Vector2 size = element.Size;
                            size.X = float.Parse(att.Value);
                            element.Size = size;

                            break;
                        }
                    case "height":
                        {
                            Vector2 size = element.Size;
                            size.Y = float.Parse(att.Value);
                            element.Size = size;

                            break;
                        }
                    case "xpos":
                        {
                            Vector2 pos = element.Position;
                            pos.X = float.Parse(att.Value);
                            element.Position = pos;

                            break;
                        }
                    case "ypos":
                        {
                            Vector2 pos = element.Position;
                            pos.Y = float.Parse(att.Value);
                            element.Position = pos;

                            break;
                        }
                    case "xlock":
                        {
                            for (int i = 0; i < (int)e_XLockMode.End; ++i)
                            {
                                e_XLockMode lockMode = (e_XLockMode)i;

                                if (lockMode.ToString().ToLower() == att.Value.ToLower())
                                {
                                    element.XLockMode = lockMode;

                                    break;
                                }
                            }

                            break;
                        }
                    case "ylock":
                        {
                            for (int i = 0; i < (int)e_YLockMode.End; ++i)
                            {
                                e_YLockMode lockMode = (e_YLockMode)i;

                                if (lockMode.ToString().ToLower() == att.Value.ToLower())
                                {
                                    element.YLockMode = lockMode;

                                    break;
                                }
                            }

                            break;
                        }
                    case "name":
                        {
                            if (!string.IsNullOrEmpty(att.Value))
                            {
                                m_namedElements.Add(att.Value, element);
                            }

                            break;
                        }
                    case "onhover":
                        {
                            string[] vals = att.Value.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                            if (vals.Length > 1)
                            {
                                Element.Interaction dele = GetAssemblyFunction(vals[0].TrimEnd(), vals[1].TrimEnd());
                                if (dele != null)
                                {
                                    element.Hover += dele;
                                }
                            }

                            break;
                        }
                    case "onnormal":
                        {
                            string[] vals = att.Value.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                            if (vals.Length > 1)
                            {
                                Element.Interaction dele = GetAssemblyFunction(vals[0].TrimEnd(), vals[1].TrimEnd());
                                if (dele != null)
                                {
                                    element.Normal += dele;
                                }
                            }

                            break;
                        }
                    case "onclick":
                        {
                            string[] vals = att.Value.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                            if (vals.Length > 1)
                            {
                                Element.Interaction dele = GetAssemblyFunction(vals[0].TrimEnd(), vals[1].TrimEnd());
                                if (dele != null)
                                {
                                    element.Click += dele;
                                }
                            }

                            break;
                        }
                    case "onrelease":
                        {
                            string[] vals = att.Value.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                            if (vals.Length > 1)
                            {
                                Element.Interaction dele = GetAssemblyFunction(vals[0].TrimEnd(), vals[1].TrimEnd());
                                if (dele != null)
                                {
                                    element.Release += dele;
                                }
                            }

                            break;
                        }
                    case "submenu":
                        {
                            element.SubMenuDirectory = att.Value;

                            break;
                        }
                    }
                }

                m_elements.Add(element);

                foreach (XmlNode node in a_node.ChildNodes)
                {
                    PopulateElements(node, element);
                }

                if (a_parent != null)
                {
                    element.Parent = a_parent;
                }

                element.State = Element.e_State.Normal;
                if (element.Normal != null)
                {
                    element.Normal.Invoke(this, element);
                }
            }
        }

        static Canvas LoadCanvasInternal (XmlDocument a_xmlDocument, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            if (a_xmlDocument.FirstChild != null)
            {
                XmlNode root = a_xmlDocument.FirstChild.NextSibling;
                if (root != null)
                {
                    Canvas canvas = new Canvas();
                    canvas.m_fileSystem = a_fileSystem;
                    canvas.m_pipeline = a_pipeline;

                    foreach (XmlAttribute att in root.Attributes)
                    {
                        switch (att.Name.ToLower())
                        {
                        case "name":
                        {
                            canvas.m_name = att.Value;

                            break;
                        }
                        case "xres":
                        {
                            canvas.m_resolution.X = float.Parse(att.Value);

                            break;
                        }
                        case "yres":
                        {
                            canvas.m_resolution.Y = float.Parse(att.Value);

                            break;
                        }
                        }
                    }

                    foreach (XmlNode node in root.ChildNodes)
                    {
                        canvas.PopulateElements(node, canvas.m_rootElement);
                    }
                    
                    return canvas;
                }
            }

            return null;
        }

        static Canvas Load (XmlDocument a_xmlDocument, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            Canvas canvas = LoadCanvasInternal(a_xmlDocument, a_fileSystem, a_pipeline);

            if (canvas != null)
            {
                a_pipeline.AddObject(canvas);
            }

            return canvas;
        }
        public static Canvas LoadGUI (string a_filename, IFileSystem a_fileSystem, Pipeline a_pipeline)
        {
            if (a_fileSystem != null)
            {
                Stream stream;
                if (a_fileSystem.Load(a_filename, out stream))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(stream);

                    Canvas canv = Load(xmlDocument, a_fileSystem, a_pipeline);

                    if (canv != null)
                    {
                        canv.m_fileSystem = a_fileSystem;
                    }

                    return canv;
                }
                else
                {
                    InternalConsole.Error("Cannot access canvas file");
                }
            }
            else
            {
                InternalConsole.Error("No filesystem to load from for canvas");
            }

            return null;
        }

        void DrawElement (Element a_element, Vector2 a_size, Vector2 a_trueSize)
        {
            if (a_element.Visible)
            {
                Vector2 newSize = a_element.Draw(a_size, a_trueSize);

                foreach (Element child in a_element.Children)
                {
                    DrawElement(child, newSize, a_trueSize);
                }

                a_element.PostDraw(a_size, a_trueSize);
            }
        }

        void UpdateElement(Element a_element, Vector2 a_resolution, MouseState a_mouseState, Vector2 a_cursorPos, Rectangle a_activeRegion)
        {
            if (a_element.Visible)
            {
                Vector2 truePos = a_element.TruePosition;

                Vector2 pos = (truePos + a_resolution) * 0.5f;
                Vector2 size = a_element.TrueSize;
                Vector2 halfSize = size * 0.5f;

                if (pos == Vector2.Zero && size == Vector2.Zero)
                {
                    return;
                }

                Vector2 regionPos = new Vector2(a_activeRegion.X, a_activeRegion.Y);
                Vector2 regionSize = new Vector2(a_activeRegion.Width, a_activeRegion.Height);
                Vector2 regionHalfSize = regionSize * 0.5f;

                pos.X += regionPos.X;

                Vector2 regionDiff = a_cursorPos - (regionPos + regionHalfSize);

                if (Math.Abs(regionDiff.X) > regionHalfSize.X || Math.Abs(regionDiff.Y) > regionHalfSize.Y) 
                {
                    a_element.State = Element.e_State.Normal;
                    if (a_element.Normal != null)
                    {
                        a_element.Normal(this, a_element);
                    }

                    return;
                }

                Vector2 cursorDiff = pos - a_cursorPos;

                if (Math.Abs(cursorDiff.X) < halfSize.X && Math.Abs(cursorDiff.Y) < halfSize.Y)
                {
                    if (a_mouseState.IsButtonDown(MouseButton.Left) && m_prevMouseState.IsButtonUp(MouseButton.Left))
                    {
                        Input.BlockButton(MouseButton.Left);

                        a_element.State = Element.e_State.Click;
                        if (a_element.Click != null)
                        {
                            a_element.Click(this, a_element);
                        }
                    }
                    else if (a_mouseState.IsButtonUp(MouseButton.Left) && m_prevMouseState.IsButtonDown(MouseButton.Left))
                    {
                        Input.BlockButton(MouseButton.Left);

                        a_element.State = Element.e_State.Release;
                        if (a_element.Release != null)
                        {
                            a_element.Release(this, a_element);
                        }

                        if (!string.IsNullOrEmpty(a_element.SubMenuDirectory))
                        {
                            Stream stream;

                            if (m_fileSystem != null && m_fileSystem.Load(a_element.SubMenuDirectory, out stream))
                            {
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load(stream);

                                stream.Dispose();

                                m_childCanvas = LoadCanvasInternal(xmlDocument, m_fileSystem, m_pipeline);
                                m_childCanvas.m_parentCanvas = this;
                                m_childCanvas.m_paintedCanvas = false;
                            }
                        }
                    }
                    else if (a_element.State == Element.e_State.Release || a_element.State == Element.e_State.Normal)
                    {
                        a_element.State = Element.e_State.Hover;
                        if (a_element.Hover != null)
                        {
                            a_element.Hover.Invoke(this, a_element);
                        }
                    }
                }
                else if (a_element.State != Element.e_State.Normal)
                {
                    a_element.State = Element.e_State.Normal;
                    if (a_element.Normal != null)
                    {
                        a_element.Normal(this, a_element);
                    }
                }

                foreach (Element child in a_element.Children)
                {
                    UpdateElement(child, a_resolution, a_mouseState, a_cursorPos, a_element.GetActiveRect(a_resolution));
                }
            }
        }

        internal void Update (Vector2 a_resolution)
        {
            if (m_childCanvas != null)
            {
                m_childCanvas.Update (a_resolution);
            }
            else
            {
                MouseState currMouseState = Mouse.GetState(); 
                Vector2 cursorPos = Input.GetCursorPosition();

                foreach (Element child in m_rootElement.Children)
                {
                    UpdateElement(child, a_resolution, currMouseState, cursorPos, m_rootElement.GetActiveRect(a_resolution));
                }

                m_prevMouseState = currMouseState;

                foreach (Element element in m_elements)
                {
                    element.Update(a_resolution);
                }
            }
        }
        internal void Draw (Vector2 a_size)
        {
            if (!m_paintedCanvas)
            {
                m_paintedCanvas = true;

                RepaintCanvas();
            }

            if (m_childCanvas == null)
            {
                GL.Enable(EnableCap.Blend);

                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                m_rootElement.Position = Vector2.Zero;
                m_rootElement.TruePosition = Vector2.Zero;
                m_rootElement.Size = m_resolution;
                m_rootElement.TrueSize = a_size;

                DrawElement(m_rootElement, a_size, a_size);

                GL.Disable(EnableCap.Blend);
            }
            else
            {
                m_childCanvas.Draw(a_size);
            }
        }

        public void ModifyObject ()
        {
            m_pipeline.GUIs.Add(this);
        }

        public void AddElement (Element a_element, string a_name = null)
        {
            m_elements.Add(a_element);

            if (a_element.Parent == null)
            {
                a_element.Parent = m_rootElement;
            }

            if (!string.IsNullOrEmpty(a_name))
            {
                m_namedElements.Add(a_name, a_element);
            }
        }

        public Element GetElement (string a_name)
        {
            if (m_namedElements.ContainsKey(a_name))
            {
                return m_namedElements[a_name];
            }

            return null;
        }
        public T GetElement<T> (string a_name) where T : Element
        {
            if (m_namedElements.ContainsKey(a_name))
            {
                return m_namedElements[a_name] as T;
            }

            return null;
        }
        public void RemoveElement (string a_name)
        {
            if (m_namedElements.ContainsKey(a_name))
            {
                Element removalElement = m_namedElements[a_name];
                removalElement.Parent = null;

                Element[] elements = m_elements.ToArray();

                m_elements = new ConcurrentBag<Element>();
                foreach (Element element in elements)
                {
                    if (removalElement != element)
                    {
                        m_elements.Add(element);
                    }
                }

                m_namedElements.Remove(a_name);

                IDisposable disposable = removalElement as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public void PopStack ()
        {
            if (m_parentCanvas != null)
            {
                m_parentCanvas.m_childCanvas = null;
                m_parentCanvas = null;
            }
        }

        void RepaintCanvas ()
        {
            Element[] elements = m_elements.ToArray();
            foreach (Element element in elements)
            {
                element.Repaint();
            }
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            PopStack();

            if (m_childCanvas != null)
            {
                m_childCanvas.Dispose();
            }

            foreach (Element element in m_elements)
            {
                IDisposable disp = element as IDisposable;

                if (disp != null)
                {
                    disp.Dispose();
                }
            }

            m_pipeline.RemoveObject(this);
        }
        ~Canvas ()
        {
            Dispose(false);
        }
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void DisposeObject ()
        {
            m_pipeline.GUIs.Remove(this);
        }
    }
}