namespace Erde.Physics
{
    public abstract class Collider
    {
        public abstract string ColliderType
        {
            get;
        }

        public abstract PhysicsObject PhysicsObject
        {
            get;
            set;
        }
    }
}