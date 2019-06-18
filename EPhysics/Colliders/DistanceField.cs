using OpenTK;

namespace Erde.Physics.Colliders
{
    public class DistanceField : Collider
    {
        IDistance m_distanceField;

        public IDistance Field
        {
            get
            {
                return m_distanceField;
            }
            set
            {
                m_distanceField = value;
            }
        }

        public override string ColliderType
        {
            get
            {
                return "DistanceField";
            }
        }

        public override PhysicsObject PhysicsObject
        {
            get;
            set;
        }

        static bool DistanceFieldSphere (out Engine.Resolution a_resolution, Collider a_colliderA, Collider a_colliderB)
        {
            a_resolution = new Engine.Resolution();

            DistanceField distanceField = a_colliderA as DistanceField;
            Sphere sphere = a_colliderB as Sphere;

            Vector3 fieldPos = distanceField.PhysicsObject.Transform.Translation;
            Vector3 spherePos = sphere.PhysicsObject.Transform.Translation;

            IDistance field = distanceField.Field;

            int width = field.Width;
            int depth = field.Depth;
            int height = field.Height;

            float voxelSize = field.Spacing;

            float chunkWidth = voxelSize * width;
            float chunkHalfWidth = chunkWidth / 2;

            float chunkDepth = voxelSize * depth;
            float chunkHalfDepth = chunkDepth / 2;

            float chunkHeight = voxelSize * height;
            float chunkHalfHeight = chunkHeight / 2;

            Vector3 relPos = spherePos - fieldPos;

            if (relPos.X >= -chunkHalfWidth && relPos.X <= chunkHalfWidth &&
                relPos.Z >= -chunkHalfDepth && relPos.Z <= chunkHalfDepth &&
                relPos.Y >= -chunkHalfHeight && relPos.Y <= chunkHalfHeight)
            {
                int halfWidth = width / 2;
                int halfDepth = depth / 2;
                int halfHeight = height / 2;

                int x = (int)(relPos.X / voxelSize) + halfWidth;
                int y = (int)(relPos.Y / voxelSize) + halfHeight;
                int z = (int)(relPos.Z / voxelSize) + halfDepth;

                float dist = field.GetDistance(x, y, z);
                float rad = sphere.Radius;

                if (dist < rad)
                {
                    a_resolution.IntersectDistance = rad - dist;

                    // All this is used to find the normal of the distance field
                    // I go through this because there is no shape to the field so I have to figure it out as I go
                    Vector3 final = Vector3.Zero;
                    
                    for (int xI = 0; xI < 3; ++xI)
                    {
                        int unitX = xI - 1;
                        int xInd = x + unitX;

                        if (xInd < 0 || xInd >= width)
                        {
                            continue;
                        }

                        for (int yI = 0; yI < 3; ++yI)
                        {
                            int unitY = yI - 1;
                            int yInd = y + unitY;

                            if (yInd <= 0 || yInd >= height)
                            {
                                continue;
                            }

                            for (int zI = 0; zI < 3; ++zI)
                            {
                                int unitZ = zI - 1;
                                int zInd = z + unitZ;

                                if (zInd < 0 || zInd >= depth)
                                {
                                    continue;
                                }

                                final += new Vector3(unitX, unitY, unitZ) * field.GetDistance(xInd, yInd, zInd);
                            }
                        }
                    }

                    // In the event that there is not a normal push the object up
                    if (final == Vector3.Zero)
                    {
                        final = Vector3.UnitY;
                    }

                    a_resolution.Normal = final.Normalized();

                    return true;
                }
            }

            return false;
        }
    }
}
