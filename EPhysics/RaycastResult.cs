using OpenTK;

namespace Erde.Physics
{
    public struct RaycastResultClosest
    {
        public Vector3 HitPosition;
        public Vector3 HitNormal;

        public CollisionObject HitObject;
    }
}
