using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

namespace NecroServer.Utils
{
    public class RandomPosition
    {
        private static RandomPosition _inst = new RandomPosition();
        public static Vector2 GetRandomPosition(int offset = 0, float speed = 1f) =>
            _inst.GetPos(offset, speed);
        public static float GetRandomFloat(int offset = 0, float speed = 1f) =>
            _inst.GetFloat(offset, speed);

        private Vector2[] _positions = new Vector2[2048];
        private readonly DateTime _startTime = DateTime.Now;

        private RandomPosition()
        {
            var rand = new Random(123);
            float nextFloat() => (float)((rand.NextDouble() * 2f) - 1f);
            for (int i = 0; i < _positions.Length; i++)
                _positions[i] = new Vector2(nextFloat(), nextFloat()).Normalize();
        }

        private Vector2 GetPos(int offset, float speed)
        {
            offset = Math.Abs(offset);
            float t = (float)(DateTime.Now - _startTime).TotalSeconds / 3f * speed;
            int i1 = (int)System.MathF.Floor(t) + offset;
            int i2 = i1 + 1;
            i1 %= _positions.Length;
            i2 %= _positions.Length;
            float frac = System.MathF.Abs(t - System.MathF.Truncate(t));
            return (_positions[i1] * (1f - frac) + _positions[i2] * frac).Normalize();
        }

        private float GetFloat(int offset, float speed)
        {
            offset = Math.Abs(offset);
            float t = (float)(DateTime.Now - _startTime).TotalSeconds / 3f * speed;
            int i1 = (int)System.MathF.Floor(t) + offset;
            int i2 = i1 + 1;
            i1 %= _positions.Length;
            i2 %= _positions.Length;
            float frac = System.MathF.Abs(t - System.MathF.Truncate(t));
            return _positions[i1].X * (1f - frac) + _positions[i2].X * frac;
        }
    }
}
