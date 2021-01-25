using Erde.Graphics.Lights;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Erde.Graphics.Rendering
{
    // Just used as something that classes that want to draw can inherit off
    public abstract class Renderer : Component, IRenderObject, IDisposable
    {
        Material           m_material;

        bool               m_visible;

        protected Graphics m_graphics;

        public abstract uint Indices
        {
            get;
        }

        public abstract float Radius
        {
            get;
        }

        public Graphics Graphics
        {
            get
            {
                return m_graphics;
            }
        }

        public bool Visible
        {
            get
            {
                return m_visible && Indices > 0;
            }
            set
            {
                m_visible = value;
            }
        }

        public struct Cleanup
        {
            private Material m_material;

            private Renderer m_renderer;

            public Material Material
            {
                get
                {
                    return m_material;
                }
            }

            public Renderer Renderer
            {
                get
                {
                    return m_renderer;
                }
            }

            public Cleanup (Renderer a_renderer, Material a_material)
            {
                m_renderer = a_renderer;
                m_material = a_material;
            }
        }

        public struct MaterialRebind : IRenderObject
        {
            Material m_newMaterial;
            Renderer m_renderer;

            public MaterialRebind (Renderer a_renderer, Material a_newMaterial)
            {
                m_renderer = a_renderer;
                m_newMaterial = a_newMaterial;
            }

            public void AddObject (LinkedList<DrawingContainer> a_objects)
            {
                // Finds the renderer linked to the old material and removes it
                if (m_renderer.Material != null)
                {
                    foreach (DrawingContainer cont in a_objects)
                    {
                        if (cont.Material == m_renderer.Material)
                        {
                            for (LinkedListNode<DrawingContainer.RenderingContainer> iter = cont.Renderers.First; iter != null; iter = iter.Next)
                            {
                                if (iter.Value.Renderer == m_renderer)
                                {
                                    cont.Renderers.Remove(iter);
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }

                // Sets the new material
                m_renderer.Material = m_newMaterial;

                m_renderer.AddObject(a_objects);
            }

            public void RemoveObject (LinkedList<DrawingContainer> a_objects)
            {
               
            }
        }

        public Material Material
        {
            get
            {
                return m_material;
            }
            internal set
            {
                m_material = value;
            }
        }

        public abstract void Draw (Camera a_camera);

        public abstract void DrawShadow (Light a_light);

        public void SetMaterial (Material a_material, Graphics a_graphics)
        {
            m_graphics = a_graphics;

            m_graphics.AddObject(new MaterialRebind(this, a_material));
        }

        public Renderer ()
        {
            m_material = null;
            m_visible = true;
        }

        void Dispose (bool a_state)
        {
#if DEBUG_INFO
            Tools.VerifyObjectMemoryState(this, a_state);
#endif

            m_graphics.RemoveObject(this);
        }

        ~Renderer ()
        {
            Dispose(false);
        }

        public virtual void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void AddObject (LinkedList<DrawingContainer> a_objects)
        {
            if (m_material == null)
            {
                return;
            }

            // Finds the material linked to the Renderer object and adds it to the materials render list
            foreach (DrawingContainer container in a_objects)
            {
                if (container.Material == m_material)
                {
                    container.Renderers.AddLast(new DrawingContainer.RenderingContainer()
                    {
                        Renderer = this
                    });

                    return;
                }
            }

            // The material was not found so add the material to the list and add the renderer to the materials list
            DrawingContainer cont = new DrawingContainer(m_material);
            cont.Renderers.AddLast(new DrawingContainer.RenderingContainer()
            {
                Renderer = this
            });
            a_objects.AddLast(cont);
        }
        public void RemoveObject (LinkedList<DrawingContainer> a_objects)
        {
            foreach (DrawingContainer cont in a_objects)
            {
                if (cont.Material == m_material)
                {
                    foreach (DrawingContainer.RenderingContainer rend in cont.Renderers)
                    {
                        if (rend.Renderer == this)
                        {
                            cont.Renderers.Remove(rend);

                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}