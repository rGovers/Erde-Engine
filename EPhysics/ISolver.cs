namespace Erde.Physics
{
    public interface ISolver
    {
        bool Collision (out Engine.Resolution a_resolution, Collider a_colliderA, Collider a_colliderB);
    }
}