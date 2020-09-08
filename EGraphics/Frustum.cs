using OpenTK;

namespace Erde.Graphics
{
    public class Frustum
    {
        private Vector4[] m_planes;

        public Frustum (Matrix4 a_viewProjection)
        {
            m_planes = new Vector4[6];

            for (int i = 0; i < 3; ++i)
            {
                m_planes[i * 2] = new Vector4
                (
                    a_viewProjection.Row0[3] - a_viewProjection.Row0[i],
                    a_viewProjection.Row1[3] - a_viewProjection.Row1[i],
                    a_viewProjection.Row2[3] - a_viewProjection.Row2[i],
                    a_viewProjection.Row3[3] - a_viewProjection.Row3[i]
                );

                m_planes[i * 2] /= m_planes[i * 2].Xyz.Length;

                m_planes[i * 2 + 1] = new Vector4
                (
                    a_viewProjection.Row0[3] + a_viewProjection.Row0[i],
                    a_viewProjection.Row1[3] + a_viewProjection.Row1[i],
                    a_viewProjection.Row2[3] + a_viewProjection.Row2[i],
                    a_viewProjection.Row3[3] + a_viewProjection.Row3[i]
                );

                m_planes[i * 2 + 1] /= m_planes[i * 2 + 1].Xyz.Length;
            }
        }

        public bool CompareSphere (Vector3 a_position, float a_radius)
        {
            for (int i = 0; i < 6; ++i)
            {
                float dot = Vector3.Dot(m_planes[i].Xyz, a_position) + m_planes[i].W;

                if (dot < -a_radius)
                {
                    return false;
                }
            }

            return true;
        }
    }
}