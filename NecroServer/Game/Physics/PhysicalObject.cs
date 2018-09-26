using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

namespace Game
{
    public class PhysicalObject
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }

        private bool CheckIntersect(OcTree[] trees)
        {
            foreach (var tree in trees)
                if (tree.Intersect(this))
                    return true;
            return false;
        }

        public virtual void Move(Vector2 newPosition, params OcTree[] trees)
        {
            float oldX = Position.X;
            float oldY = Position.Y;
            float newX = newPosition.X;
            float newY = newPosition.Y;

            Position = new Vector2(oldX, newY);
            if (CheckIntersect(trees))
            {
                newY = 2 * oldY - newY;
                Position = new Vector2(oldX, newY);
                if (CheckIntersect(trees))
                    newY = oldY;
            }

            Position = new Vector2(newX, newY);
            if (CheckIntersect(trees))
            {
                newX = 2 * oldX - newX;
                Position = new Vector2(newX, newY);
                if (CheckIntersect(trees))
                    newX = oldX;
            }

            Position = new Vector2(newX, newY);
        }

        public virtual bool TryMove(Vector2 newPosition, params OcTree[] trees)
        {
            var oldPos = Position;
            Position = newPosition;
            if (CheckIntersect(trees))
                Position = oldPos;
            else
                return true;
            return false;
        }
    }
}
