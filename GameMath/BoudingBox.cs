using System;
using System.Collections.Generic;
using System.Text;

namespace GameMath
{
    public struct BoundingBox
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public BoundingBox(Vector2 center, float radius)
            : this(center + new Vector2(-radius, -radius), radius * 2f, radius * 2f) { }

        public BoundingBox(Vector2 leftUp, float width, float height)
        {
            X = leftUp.X;
            Y = leftUp.Y;
            Width = width;
            Height = height;
        }

        public BoundingBox(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersect(BoundingBox other) =>
            X < other.X + other.Width &&
            X + Width > other.X &&
            Y < other.Y + other.Height &&
            Height + Y > other.Y;

        public bool Contains(BoundingBox other) =>
            other.X > X && other.Y > Y &&
            other.X + other.Width < X + Width &&
            other.Y + other.Height < Y + Height;
    }
}
