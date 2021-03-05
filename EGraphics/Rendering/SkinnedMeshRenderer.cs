using Erde.Graphics.Lights;
using Erde.Graphics.Variables;
using OpenTK;
using System; 
using System.Collections.Generic;

namespace Erde.Graphics.Rendering
{
    public class SkinnedMeshRenderer : Renderer
    {
        Skeleton    m_skeleton;

        Model       m_model;

        Matrix4[]   m_matrices;
        Transform[] m_transforms;

        public Model Model
        {
            get
            {
                return m_model;
            }
            set
            {
                lock (this)
                {
                    m_model = value;
                }
            }
        }

        public override uint Indices
        {
            get
            {
                if (m_model != null)
                {
                    lock (this)
                    {
                        return m_model.Indices;
                    }
                }

                return 0;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible && Indices > 0;
            }
        }

        public override float Radius
        {
            get
            {
                return m_model.Radius;
            }
        }

        public SkinnedMeshRenderer() : base()
        {
            m_model = null;
            m_skeleton = null;

            m_transforms = null;

            m_matrices = null;
        }

        void GenerateSkeleton(GameObject a_parent, SkeletonNode a_node)
        {
            GameObject obj = a_parent.GetChild(a_node.Name);

            if (obj == null)
            {
                obj = new GameObject();

                obj.Transform.Parent = a_parent.Transform;
                obj.Name = a_node.Name;
            }

            m_transforms[a_node.Index] = obj.Transform;

            IEnumerable<SkeletonNode> children = a_node.Children;
            foreach (SkeletonNode child in children)
            {
                GenerateSkeleton(obj, child);
            }
        }

        public void SetSkeleton(Skeleton a_skeleton)
        {
            lock (this)
            {
                m_skeleton = a_skeleton;

                int count = Math.Min(127, m_skeleton.GetLastIndex() + 1);

                m_transforms = new Transform[count];
                m_matrices = new Matrix4[count];

                SkeletonNode node = m_skeleton.RootNode;

                GenerateSkeleton(GameObject, node);
            }
        }

        public override void Update()
        {
            int count = m_matrices.Length;

            Matrix4 mat = Transform.ToMatrix();
            Matrix4 inverse = Matrix4.Invert(mat);

            for (int i = 0; i < count; ++i)
            {
                m_matrices[i] = m_transforms[i].ToMatrix() * inverse;
            }
        }

        public override void Draw (Camera a_camera)
        {
            if (m_model != null)
            {
                lock (this)
                {
                    // Dirty but works
                    if (m_matrices != null)
                    {
                        GraphicsCommand.BindMatrix4(Material.Program, 128, m_matrices);
                    }

                    GraphicsCommand.BindModel(m_model);

                    GraphicsCommand.DrawElementsUInt(m_model.Indices);
                }
            }
        }
        public override void DrawShadow (Light a_light)
        {
            if (m_model != null)
            {
                lock (this)
                {
                    if (m_matrices != null)
                    {
                        GraphicsCommand.BindMatrix4(Material.Program, 128, m_matrices);
                    }

                    GraphicsCommand.BindModel(m_model);

                    GraphicsCommand.DrawElementsUInt(m_model.Indices);
                }
            }
        }

        public override void AddObject (LinkedList<DrawingContainer> a_objects, LinkedList<Renderer> a_renderers)
        {
            base.AddObject(a_objects, a_renderers);

            a_renderers.AddLast(this);
        }
        public override void RemoveObject(LinkedList<DrawingContainer> a_objects, LinkedList<Renderer> a_renderers)
        {
            base.RemoveObject(a_objects, a_renderers);

            a_renderers.Remove(this);
        }
    }
}