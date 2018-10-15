using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

namespace NecroServer.Utils
{
    public class RandomPosition
    {
        private static RandomPosition _inst = new RandomPosition();
        public static Vector2 GetRandomPosition(int offset = 0) =>
            _inst.GetPos(offset);

        private Vector2[] _positions = new Vector2[1000];
        private DateTime _startTime = DateTime.Now;

        private RandomPosition()
        {
            for (int i = 0; i < _positions.Length; i++)
                _positions[i] = new Vector2(GameMath.MathF.RandomFloat(-1f, 1f), GameMath.MathF.RandomFloat(-1f, 1f)).Normalize();
        }

        private Vector2 GetPos(int offset)
        {
            float t = (float)(DateTime.Now - _startTime).TotalSeconds / 5f;
            int i1 = (int)System.MathF.Floor(t) + offset;
            int i2 = i1 + 1;
            i1 %= _positions.Length;
            i2 %= _positions.Length;
            float frac = System.MathF.Abs(t - System.MathF.Truncate(t));
            return _positions[i1] * (1f - frac) + _positions[i2] * frac;
        }
    }
}
