using OpenTK;
using System;
using System.Collections.Generic;

namespace Erde.Graphics.Variables
{
    public class SkeletonNode
    {
        string             m_name;
        ushort             m_index;

        Skeleton           m_skeleton;

        SkeletonNode       m_parent;
        List<SkeletonNode> m_children;

        Matrix4            m_transform;

        public Skeleton Skeleton
        {
            get
            {
                return m_skeleton;
            }
            internal set
            {
                m_skeleton = value;
            }
        }

        public ushort Index
        {
            get
            {
                return m_index;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public SkeletonNode Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                if (m_parent != null)
                {
                    m_parent.m_children.Remove(this);
                }

                m_parent = value;

                if (m_parent != null)
                {
                    m_parent.m_children.Add(this);
                }
            }
        }
        public IEnumerable<SkeletonNode> Children
        {
            get
            {
                return m_children;
            }
        }

        public SkeletonNode(ushort a_index, string a_name)
        {
            m_skeleton = null;

            m_name = a_name;

            m_parent = null;
            m_children = new List<SkeletonNode>();

            m_index = a_index;
        }
    }

    public class Skeleton
    {
        SkeletonNode m_rootNode;

        public SkeletonNode RootNode
        {
            get
            {
                return m_rootNode;
            }
        }

        void SetSkeleton(SkeletonNode a_node)
        {
            a_node.Skeleton = this;

            foreach (SkeletonNode node in a_node.Children)
            {
                SetSkeleton(node);
            }
        }

        internal Skeleton(SkeletonNode a_node)
        {
            m_rootNode = a_node;

            SetSkeleton(m_rootNode);
        }

        ushort GetLastIndexRecursive(SkeletonNode a_node)
        {
            ushort max = a_node.Index;

            foreach (SkeletonNode node in a_node.Children)
            {
                max = Math.Max(max, GetLastIndexRecursive(node));
            }

            return max;
        }

        public ushort GetLastIndex()
        {
            return GetLastIndexRecursive(m_rootNode);
        }

        SkeletonNode GetNodeRecursive(SkeletonNode a_node, string a_name)
        {
            if (a_node.Name == a_name)
            {
                return a_node;
            }

            foreach (SkeletonNode node in a_node.Children)
            {
                SkeletonNode val = GetNodeRecursive(node, a_name);

                if (val != null)
                {
                    return val;
                }
            }

            return null;
        }

        public SkeletonNode GetNode(string a_name)
        {
            return GetNodeRecursive(m_rootNode, a_name);
        }
    }
}