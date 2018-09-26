using GameMath;

namespace Game
{
    public struct OcTreeObject
    {
        public Vector2 Position { get; }
        public float Radius { get; }
        public BoundingBox Box { get; }
        public PhysicalObject LinkedObject { get; }

        public OcTreeObject(PhysicalObject obj)
        {
            Position = obj.Position;
            Radius = obj.Radius;
            Box = new BoundingBox(Position, Radius);
            LinkedObject = obj;
        }

        public bool Intersect(OcTreeObject other)
        {
            var sqrDist = (Position - other.Position).SqrLength();
            var sqrRad = (Radius + other.Radius); sqrRad *= sqrRad;
            return sqrDist < sqrRad;
        }
    }
}
