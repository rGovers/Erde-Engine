using Erde.Application;
using Erde.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Erde.Graphics.GUI
{
    public class Canvas : IGLObject
    {
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
            Element element = null;

            switch (a_node.Name.ToLower())
            {
            case "image":
                {
                    element = Image.Create(a_node, m_fileSystem, m_pipeline);

                    break;
                }
            case "textbox":
                {
                    element = TextBox.Create(a_node, m_pipeline);

                    break;
                }
            case "textfield":
                {
                    element = TextField.Create(a_node, m_pipeline);

                    break;
                }
            }

            if (element != null)
            {
                m_elements.Add(element);

                foreach (XmlAttribute att in a_node.Attributes)
                {
                    switch (att.Name.ToLower())
                    {
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
                a_pipeline.InputQueue.Enqueue(canvas);
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

        void DrawElement (Element a_element, Vector2 a_size)
        {
            if (a_element.Visible)
            {
                a_element.Draw(a_size);

                foreach (Element child in a_element.Children)
                {
                    DrawElement(child, a_size);
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

                foreach (Element element in m_elements)
                {
                    if (element != m_rootElement)
                    {
                        Vector2 pos = (element.TruePosition + a_resolution) / 2;
                        Vector2 size = element.TrueSize;

                        Vector2 halfSize = size / 2;

                        if (pos != Vector2.Zero && size != Vector2.Zero)
                        {
                            if (cursorPos.X > pos.X - halfSize.X && cursorPos.Y > pos.Y - halfSize.Y && cursorPos.X < pos.X + halfSize.X && cursorPos.Y < pos.Y + halfSize.Y)
                            {
                                const MouseButton left = MouseButton.Left;

                                if (currMouseState.IsButtonDown(left) && m_prevMouseState.IsButtonUp(left))
                                {
                                    Input.BlockButton(left);

                                    element.State = Element.e_State.Click;
                                    if (element.Click != null)
                                    {
                                        element.Click(this, element);
                                    }
                                }
                                else if (currMouseState.IsButtonUp(left) && m_prevMouseState.IsButtonDown(left))
                                {
                                    Input.BlockButton(left);

                                    element.State = Element.e_State.Release;
                                    if (element.Release != null)
                                    {
                                        element.Release(this, element);
                                    }
                                    if (!string.IsNullOrEmpty(element.SubMenuDirectory))
                                    {
                                        Stream stream;

                                        if (m_fileSystem != null && m_fileSystem.Load(element.SubMenuDirectory, out stream))
                                        {
                                            XmlDocument xmlDocument = new XmlDocument();
                                            xmlDocument.Load(stream);

                                            m_childCanvas = LoadCanvasInternal(xmlDocument, m_fileSystem, m_pipeline);
                                            m_childCanvas.m_parentCanvas = this;
                                            m_childCanvas.m_paintedCanvas = false;
                                        }
                                    }
                                }
                                else if (element.State == Element.e_State.Release || element.State == Element.e_State.Normal)
                                {
                                    element.State = Element.e_State.Hover;
                                    if (element.Hover != null)
                                    {
                                        element.Hover.Invoke(this, element);
                                    }
                                }
                            }
                            else if (element.State != Element.e_State.Normal)
                            {
                                element.State = Element.e_State.Normal;
                                if (element.Normal != null)
                                {
                                    element.Normal(this, element);
                                }
                            }
                        }
                    }
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
                GL.Disable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                m_rootElement.Position = Vector2.Zero;
                m_rootElement.TruePosition = Vector2.Zero;
                m_rootElement.Size = m_resolution;
                m_rootElement.TrueSize = a_size;

                DrawElement(m_rootElement, a_size);

                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest);
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
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

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

            m_pipeline.DisposalQueue.Enqueue(this);
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