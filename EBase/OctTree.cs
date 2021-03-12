using System.Collections.Generic;

namespace Erde
{
    public class OctTreeNode<T>
    {
        OctTree<T>        m_octTree;

        int               m_depth;

        T                 m_data;

        OctTreeNode<T>    m_parent;
        OctTreeNode<T>[]  m_children;

        public OctTreeNode<T> Parent
        {
            get
            {
                return m_parent;
            }
        }

        public OctTreeNode<T>[] Children
        {
            get
            {
                return m_children;
            }
        }

        public T Data
        {
            get
            {
                return m_data;
            }
        }

        public int Depth
        {
            get
            {
                return m_depth;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return m_children == null;
            }
        }

        internal OctTreeNode(OctTree<T> a_octTree)
        {
            m_octTree = a_octTree;

            m_depth = 0;

            m_parent = null;
            m_children = null;
        }

        public void ClearChildren()
        {
            for (int i = 0; i < 8; ++i)
            {
                m_children[i].m_parent = null;
            }

            m_children = null;
        }

        public void Spit()
        {
            if (m_depth < m_octTree.MaxDepth)
            {   
                m_children = new OctTreeNode<T>[8];

                for (int i = 0; i < 8; ++i)
                {
                    OctTreeNode<T> child = new OctTreeNode<T>();
                    child.m_parent = this;
                    child.m_depth = m_depth + 1;
                    m_children[i] = child;
                }   
            }
        }
    }

    public class OctTree<T>
    {
        int            m_maxDepth;

        OctTreeNode<T> m_rootNode;

        public OctTreeNode<T> RootNode
        {
            get
            {
                return m_rootNode;
            }
        }

        public int MaxDepth
        {
            get
            {
                return m_maxDepth;
            }
        }

        public OctTree(int a_maxDepth)
        {
            m_rootNode = new OctTreeNode<T>(this);
        }

        public OctTreeNode<T> GetChild(int a_x, int a_y, int a_z)
        {
            int size = m_maxDepth * m_maxDepth;

            OctTreeNode<T> node = m_rootNode;

            int x = a_x;
            int y = a_y;
            int z = a_z;

            while (!node.IsLeaf)
            {
                int depth = node.Depth;
                int depthSqr = depth * depth;

                int gridSize = size / depthSqr;
                int halfGridSize = gridSize / 2;

                int index = 0;

                if (x >= halfGridSize) 
                {
                    index |= 1 << 0;
                    x -= halfGridSize;
                }
                if (y >= halfGridSize)
                {
                    index |= 1 << 1;
                    y -= halfGridSize;
                }
                if (z >= halfGridSize)
                {
                    index |= 1 << 2;
                    z -= halfGridSize;
                }

                node = node.Children[index];
            }

            return node;
        }
    }
}